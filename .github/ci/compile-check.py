#!/usr/bin/env python3
"""Compiles all project C# the way the Unity 6 editor splits it (asmdef assemblies plus
Assembly-CSharp / -firstpass / -Editor) with the Roslyn compiler and reference assemblies that ship
inside the GameCI editor image, for the Windows, macOS and Linux player defines and for the editor.
Needs no Unity license, so it runs on every pull request and catches compile errors in a few minutes.

  docker run --rm -v "$PWD:/project" -w /project unityci/editor:ubuntu-6000.6.7f1-base-3 \
    python3 .github/ci/compile-check.py
"""
import json, os, re, subprocess, sys, glob, collections, tempfile

U = os.environ.get('UNITY_DATA', '/opt/unity/Editor/Data')
PROJECT = os.path.abspath(sys.argv[1] if len(sys.argv) > 1 and not sys.argv[1].startswith('--') else '.')
PLATFORM = os.environ.get('PLATFORM', 'LINUX')
OUT = os.path.join(os.environ.get('OUT') or tempfile.mkdtemp(), PLATFORM)
os.makedirs(OUT, exist_ok=True)
DOTNET = U + '/DotNetSdk/dotnet'
CSC = sorted(glob.glob(U + '/DotNetSdk/sdk/*/Roslyn/bincore/csc.dll'))[-1]
BUILTIN = U + '/Resources/PackageManager/BuiltInPackages/'
PACKAGES = [BUILTIN + p for p in os.environ.get('PACKAGES', 'com.unity.ugui').split(',') if p]

BASE_DEFINES = ['UNITY_6000_6', 'UNITY_6000_6_7', 'UNITY_6000', 'UNITY_STANDALONE', 'UNITY_STANDALONE_' + PLATFORM,
                'ENABLE_MONO', 'NET_STANDARD_2_1', 'NET_STANDARD', 'NETSTANDARD', 'NETSTANDARD2_1', 'CSHARP_7_3_OR_NEWER',
                'ENABLE_PHYSICS', 'UNITY_POST_PROCESSING_STACK_V2', 'CROSS_PLATFORM_INPUT', 'MIRROR']
for y in range(2017, 2024):
    for m in range(1, 5): BASE_DEFINES.append('UNITY_%d_%d_OR_NEWER' % (y, m))
for m in range(0, 7): BASE_DEFINES.append('UNITY_6000_%d_OR_NEWER' % m)
for v in ['5_3', '5_4', '5_5', '5_6']: BASE_DEFINES.append('UNITY_%s_OR_NEWER' % v)
NOWARN = '0618,0414,0169,0649,0219,0067,0108,0114,0162,0168,0472,1635,0672,0612,8632,0109,0105'

engine_refs = sorted(glob.glob(U + '/Managed/UnityEngine/UnityEngine*.dll')) + sorted(glob.glob(U + '/Managed/UnityEngine/Unity.*.dll'))
editor_refs = sorted(glob.glob(U + '/Managed/UnityEngine/UnityEditor*.dll'))
std_refs = [U + '/NetStandard/ref/2.1.0/netstandard.dll'] + sorted(glob.glob(U + '/NetStandard/compat/2.1.0/shims/netstandard/*.dll'))

def guid(path):
    try: return re.search(r'guid: (\w+)', open(path + '.meta').read()).group(1)
    except Exception: return None

# --- collect asmdefs
asm = {}  # name -> info
roots = [PROJECT + '/Assets'] + PACKAGES
for root in roots:
    for f in glob.glob(root + '/**/*.asmdef', recursive=True):
        if '/Tests/' in f or '/Examples/' in f: continue
        d = json.load(open(f))
        asm[d['name']] = dict(name=d['name'], dir=os.path.dirname(f), refs=d.get('references', []), guid=guid(f),
                              editor=(d.get('includePlatforms') == ['Editor']), unsafe=d.get('allowUnsafeCode', False),
                              defines=d.get('defineConstraints', []), precompiled=d.get('precompiledReferences', []),
                              override_refs=d.get('overrideReferences', False))
by_guid = {a['guid']: n for n, a in asm.items() if a['guid']}
asm_dirs = sorted([a['dir'] for a in asm.values()], key=len, reverse=True)

def owner(path):
    for d in asm_dirs:
        if path.startswith(d + '/'):
            return next(n for n, a in asm.items() if a['dir'] == d)
    return None

def resolve(ref):
    if ref.startswith('GUID:'): return by_guid.get(ref[5:])
    return ref if ref in asm else None

all_cs = []
for root in roots:
    all_cs += glob.glob(root + '/**/*.cs', recursive=True)
sources = collections.defaultdict(list)
for f in all_cs:
    if '/Tests/' in f or '/Examples/' in f: continue
    o = owner(f)
    if o: sources[o].append(f); continue
    if not f.startswith(PROJECT + '/Assets/'): continue
    rel = f[len(PROJECT) + 1:]
    is_editor = '/Editor/' in '/' + rel
    firstpass = rel.startswith(('Assets/Plugins/', 'Assets/Standard Assets/', 'Assets/Pro Standard Assets/'))
    name = 'Assembly-CSharp' + ('-firstpass' if firstpass else '') + ('-Editor' if is_editor else '')
    sources[name].append(f)

# One internal TextCore API is only visible to the real TMP build inside Unity; stub it here.
for f in list(sources.get('Unity.TextMeshPro.Editor', [])):
    if f.endswith('/TMPro_FontAssetCreatorWindow.cs'): sources['Unity.TextMeshPro.Editor'].remove(f)
for i, f in enumerate(sources.get('Unity.TextMeshPro', [])):
    if f.endswith('/TMP_FontAsset.cs'):
        patched = OUT + '/TMP_FontAsset.patched.cs'
        txt = open(f).read().replace('FontEngine.GetGlyphPairAdjustmentRecords(m_GlyphIndexList, out int recordCount)',
                                     'new GlyphPairAdjustmentRecord[0]; int recordCount = 0')
        open(patched, 'w').write(txt)
        sources['Unity.TextMeshPro'][i] = patched

dlls_in_assets = [f for f in glob.glob(PROJECT + '/Assets/**/*.dll', recursive=True) if '/Editor/' not in f]
editor_dlls_in_assets = [f for f in glob.glob(PROJECT + '/Assets/**/*.dll', recursive=True)]

built = {}
def build(name, editor_flavor):
    key = (name, editor_flavor)
    if key in built: return built[key]
    built[key] = None
    a = asm.get(name)
    is_editor = editor_flavor or (a and a['editor']) or name.endswith('-Editor')
    refs = []
    if a:
        # Unity 6 asmdefs implicitly see the built-in uGUI assembly
        if name not in ('UnityEngine.UI', 'Unity.InternalAPIEngineBridge.004') and sources.get('UnityEngine.UI'):
            p = build('UnityEngine.UI', is_editor)
            if p: refs.append(p)
            if is_editor and name != 'UnityEditor.UI' and sources.get('UnityEditor.UI'):
                p = build('UnityEditor.UI', True)
                if p: refs.append(p)
        for r in a['refs']:
            rn = resolve(r)
            if rn and sources.get(rn):
                if asm[rn]['editor'] and not is_editor: continue
                p = build(rn, is_editor)
                if p: refs.append(p)
    else:
        # predefined assemblies reference every auto-referenced asmdef and the earlier predefined ones
        for n2, a2 in asm.items():
            if a2['editor'] and not is_editor: continue
            if not sources.get(n2): continue
            p = build(n2, is_editor)
            if p: refs.append(p)
        if name.startswith('Assembly-CSharp') and not name.startswith('Assembly-CSharp-firstpass'):
            p = build('Assembly-CSharp-firstpass', is_editor)
            if p: refs.append(p)
        if name == 'Assembly-CSharp-Editor':
            for n2 in ['Assembly-CSharp', 'Assembly-CSharp-firstpass-Editor']:
                p = build(n2, True)
                if p: refs.append(p)
        if name == 'Assembly-CSharp-firstpass-Editor':
            p = build('Assembly-CSharp-firstpass', True)
            if p: refs.append(p)
    defines = BASE_DEFINES + (['UNITY_EDITOR', 'UNITY_EDITOR_LINUX'] if is_editor else [])
    outdir = OUT + ('/editor' if is_editor else '/player')
    os.makedirs(outdir, exist_ok=True)
    out = outdir + '/' + name + '.dll'
    allrefs = std_refs + engine_refs + (editor_refs if is_editor else []) + refs + (editor_dlls_in_assets if is_editor else dlls_in_assets)
    rsp = out + '.rsp'
    with open(rsp, 'w') as fh:
        fh.write('-nologo -target:library -nostdlib -noconfig -langversion:9.0 -deterministic -debug-\n')
        fh.write('-nowarn:%s\n' % NOWARN)
        if a and a['unsafe'] or not a: fh.write('-unsafe\n')
        fh.write('-define:%s\n' % ';'.join(defines))
        for r in allrefs: fh.write('-r:"%s"\n' % r)
        fh.write('-out:"%s"\n' % out)
        for s in sources[name]: fh.write('"%s"\n' % s)
    res = subprocess.run([DOTNET, 'exec', CSC, '@' + rsp], capture_output=True, text=True)
    errs = [l for l in res.stdout.splitlines() if ' error ' in l]
    tag = name + (' [editor]' if is_editor else '')
    if res.returncode != 0 and a and not a['dir'].startswith(PROJECT):
        print('WARN (package, harness limitation)', tag, len(errs), 'errors')
        return None
    if res.returncode != 0:
        print('FAIL', tag, len(errs), 'errors')
        for l in errs[:40]: print('   ', l.replace(PROJECT + '/', ''))
        return None
    if '--quiet' not in sys.argv: print('ok  ', tag)
    built[key] = out
    return out

ok = True
for target, ed in [('Assembly-CSharp', False), ('Assembly-CSharp-Editor', True)]:
    if not build(target, ed): ok = False
for n, a in asm.items():
    if sources.get(n) and not build(n, a['editor']) and a['dir'].startswith(PROJECT): ok = False
print('%s: %s' % (PLATFORM, 'ALL OK' if ok else 'FAILED'))
sys.exit(0 if ok else 1)
