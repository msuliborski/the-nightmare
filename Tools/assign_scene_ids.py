#!/usr/bin/env python3
"""Gives every networked object placed in a scene a fixed Mirror sceneId, stored in the scene file.

Mirror matches scene objects between host and clients by NetworkIdentity.sceneId. The editor normally
assigns a random one when a scene is opened and saved, but scenes converted from UNET by
migrate_unet_to_mirror.py have none. They are written into the .unity files here so that every build
(Windows, macOS, Linux) carries the same ids and the platforms can play together. The ids are derived
from the scene path and the object's file id, so re-running the script never changes them.

Covers NetworkIdentity components placed directly in a scene and instances of prefabs whose root has
a NetworkIdentity (the id is written as a prefab-instance override). Objects that already have a
sceneId are left alone.

  python3 Tools/assign_scene_ids.py            # all scenes under Assets/
  python3 Tools/assign_scene_ids.py --check    # CI: fail if any networked scene object has no id
"""
import glob, os, re, sys, zlib

IDENTITY_GUID = '9b91ecbcc199f4492b9a91e820070131'
ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))


def blocks(text):
    """Split a Unity YAML file into (header, body) pairs; header is the '--- !u!X &id' line."""
    parts = re.split(r'(?m)^(--- !u!\d+ &-?\d+(?: stripped)?\n)', text)
    return parts[0], [(parts[i], parts[i + 1]) for i in range(1, len(parts), 2)]


def file_id(header):
    return re.search(r'&(-?\d+)', header).group(1)


def guid_of(path):
    m = re.search(r'guid: (\w+)', open(path + '.meta', encoding='utf-8').read())
    return m.group(1) if m else None


def prefab_identities():
    """prefab guid -> fileID of its NetworkIdentity (Mirror allows one per prefab, on the root)."""
    result = {}
    for path in glob.glob(os.path.join(ROOT, 'Assets', '**', '*.prefab'), recursive=True):
        text = open(path, encoding='utf-8', errors='replace').read()
        if not text.startswith('%YAML'):
            continue
        _, bl = blocks(text)
        ids = [file_id(h) for h, b in bl if 'guid: ' + IDENTITY_GUID in b and 'stripped' not in h]
        if len(ids) == 1:
            result[guid_of(path)] = ids[0]
    return result


def make_id(scene_rel, key, used):
    n = zlib.crc32((scene_rel + ':' + key).encode()) & 0xFFFFFFFF
    while n == 0 or n in used:
        n = (n * 1103515245 + 12345) & 0xFFFFFFFF
    used.add(n)
    return n


def process(path, prefabs, check):
    rel = os.path.relpath(path, ROOT).replace(os.sep, '/')
    text = open(path, encoding='utf-8', errors='replace').read()
    if not text.startswith('%YAML'):
        return 0
    head, bl = blocks(text)
    used = set(int(v) for v in re.findall(r'(?m)^  sceneId: (\d+)$', text))
    used |= set(int(v) for v in re.findall(r'propertyPath: sceneId\n\s+value: (\d+)', text))
    changed = 0
    out = []
    for h, b in bl:
        fid = file_id(h)
        if 'stripped' not in h and 'guid: ' + IDENTITY_GUID in b and re.search(r'(?m)^  m_Script:', b):
            m = re.search(r'(?m)^  sceneId: (\d+)\n', b)
            if not m or m.group(1) == '0':
                line = '  sceneId: %d\n' % make_id(rel, fid, used)
                b = b.replace(m.group(0), line) if m else b.rstrip('\n') + '\n' + line
                changed += 1
        elif re.match(r'PrefabInstance:|Prefab:', b):
            src = re.search(r'm_SourcePrefab: \{fileID: \d+, guid: (\w+)', b) or \
                  re.search(r'm_ParentPrefab: \{fileID: \d+, guid: (\w+)', b)
            if src and src.group(1) in prefabs:
                pguid, target = src.group(1), prefabs[src.group(1)]
                has = re.search(r'- target: \{fileID: %s, guid: %s,\s*type: 3\}\n\s+propertyPath: sceneId\n\s+value: (\d+)'
                                % (target, pguid), b)
                if not has or has.group(1) == '0':
                    value = make_id(rel, fid, used)
                    if has:
                        b = b.replace(has.group(0), has.group(0)[:-len(has.group(1))] + str(value))
                    else:
                        mod = ('    - target: {fileID: %s, guid: %s, type: 3}\n'
                               '      propertyPath: sceneId\n'
                               '      value: %d\n'
                               '      objectReference: {fileID: 0}\n') % (target, pguid, value)
                        if '    m_Modifications: []\n' in b:
                            b = b.replace('    m_Modifications: []\n', '    m_Modifications:\n' + mod, 1)
                        else:
                            b = b.replace('    m_Modifications:\n', '    m_Modifications:\n' + mod, 1)
                    changed += 1
        out.append(h + b)
    if changed and check:
        print('%-45s %d networked objects without a sceneId' % (rel, changed))
    elif changed:
        open(path, 'w', encoding='utf-8', newline='\n').write(head + ''.join(out))
        print('%-45s %d scene ids assigned' % (rel, changed))
    return changed


def main():
    prefabs = prefab_identities()
    check = '--check' in sys.argv
    scenes = [a for a in sys.argv[1:] if a != '--check'] or \
        glob.glob(os.path.join(ROOT, 'Assets', '**', '*.unity'), recursive=True)
    total = sum(process(os.path.abspath(s), prefabs, check) for s in sorted(scenes))
    if check:
        if total:
            print('error: run python3 Tools/assign_scene_ids.py (or open and save the scenes) and commit.')
            sys.exit(1)
        print('All networked scene objects have a sceneId.')
    else:
        print('%d scene ids assigned (%d networked prefabs known)' % (total, len(prefabs)))


if __name__ == '__main__':
    main()
