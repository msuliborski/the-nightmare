#!/usr/bin/env bash
# License-free checks run on every pull request (a few minutes, no Unity license needed):
#  - asset rules the Linux build editor depends on,
#  - all C# compiled with the Unity 6 compiler and reference assemblies for Windows, macOS, Linux
#    and the editor (see compile-check.py).
#
#   docker run --rm -v "$PWD:/project" -w /project \
#     unityci/editor:ubuntu-6000.6.7f1-base-3 bash .github/ci/compile-check.sh
set -euo pipefail
PROJECT=${1:-$PWD}
cd "$PROJECT"

# CI builds run the Linux editor, which cannot import H.264/MP4/MOV video: such clips end up empty in
# every build (the main menu showed a white screen). Video must be WebM (VP8) or OGV.
bad_videos=$(find Assets -type f \( -iname '*.mp4' -o -iname '*.mov' -o -iname '*.m4v' -o -iname '*.avi' \) | head -20)
if [ -n "$bad_videos" ]; then
  echo "error: video formats the Linux editor cannot import (convert to WebM/VP8):"
  echo "$bad_videos"
  exit 1
fi

# UNET is gone from Unity 6; networking is Mirror (Assets/Mirror).
if grep -rlE 'using UnityEngine\.Networking;|UnityEngine\.Networking\.(NetworkBehaviour|NetworkManager)' --include='*.cs' Assets/Scripts; then
  echo "error: the files above still use UNET (UnityEngine.Networking); use Mirror instead."
  exit 1
fi

command -v python3 >/dev/null || { apt-get update -qq && apt-get install -y -qq python3 >/dev/null; }

# Every object in a text scene/prefab starts with "--- !u!<class> &<id>" followed by its class name line;
# without it Unity silently drops the component (a scripted edit once lost 51 of them).
python3 - <<'PY'
import glob, re, sys
bad = []
for f in glob.glob('Assets/**/*.unity', recursive=True) + glob.glob('Assets/**/*.prefab', recursive=True):
    text = open(f, encoding='utf-8', errors='replace').read()
    if text.startswith('%YAML'):
        bad += ['%s: %s' % (f, m.group(1)) for m in re.finditer(r'(?m)^(--- !u!\d+ &-?\d+(?: stripped)?)\n(?![A-Za-z]\w*:\s*$)', text)]
for b in bad[:20]: print('error: object without a class line in', b)
sys.exit(1 if bad else 0)
PY

# Mirror sceneIds must be saved in the scenes (not generated per build) so all platforms agree.
python3 Tools/assign_scene_ids.py --check
OUT=$(mktemp -d)
status=0
for platform in WIN OSX LINUX; do
  PLATFORM=$platform OUT=$OUT python3 .github/ci/compile-check.py "$PROJECT" --quiet || status=1
done
exit $status
