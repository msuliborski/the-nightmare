#!/usr/bin/env bash
# Compiles all project C# (runtime + editor) against the Unity 2018.4 reference
# assemblies shipped in the GameCI editor image, then runs the UNET weaver on it. Needs no Unity license, so it
# runs on every pull request and catches compile errors in ~1 minute.
#
#   docker run --rm -v "$PWD:/project" -w /project \
#     unityci/editor:ubuntu-2018.4.3f1-base-3 bash .github/ci/compile-check.sh
set -euo pipefail

DATA=${UNITY_DATA:-/opt/unity/Editor/Data}
PROJECT=${1:-$PWD}
OUT=$(mktemp -d)
MONO="$DATA/MonoBleedingEdge/bin/mono"
CSC="$DATA/MonoBleedingEdge/lib/mono/4.5/csc.exe"
API="$DATA/MonoBleedingEdge/lib/mono/4.7.1-api"

refs=()
for f in "$DATA"/Managed/UnityEngine/*.dll; do refs+=("-r:$f"); done
refs+=("-r:$DATA/UnityExtensions/Unity/GUISystem/UnityEngine.UI.dll"
       "-r:$DATA/UnityExtensions/Unity/Networking/UnityEngine.Networking.dll")
for f in mscorlib System System.Core System.Xml System.Xml.Linq System.Runtime.Serialization Facades/netstandard Facades/System.Runtime; do
  refs+=("-r:$API/$f.dll")
done

defines="UNITY_2018_4;UNITY_2018_4_OR_NEWER;UNITY_2018_3_OR_NEWER;UNITY_2018_2_OR_NEWER;UNITY_2018_1_OR_NEWER"
defines+=";UNITY_2017_4_OR_NEWER;UNITY_2017_3_OR_NEWER;UNITY_2017_2_OR_NEWER;UNITY_2017_1_OR_NEWER"
defines+=";UNITY_5_6_OR_NEWER;UNITY_5_5_OR_NEWER;UNITY_5_4_OR_NEWER;UNITY_5_3_OR_NEWER"
defines+=";UNITY_STANDALONE;ENABLE_MONO;NET_4_6;CROSS_PLATFORM_INPUT;UNITY_POST_PROCESSING_STACK_V2"
# 0618 obsolete (UNET is deprecated), the rest are noise from third-party assets.
nowarn="0618,0414,0169,0649,0219,0067,0108,0114,0162,0168,0472,1635"

cd "$PROJECT"

# CI builds run the Linux editor, which cannot import H.264/MP4/MOV video: such clips end up empty in
# every build (the main menu showed a white screen). Video must be WebM (VP8) or OGV.
bad_videos=$(find Assets -type f \( -iname '*.mp4' -o -iname '*.mov' -o -iname '*.m4v' -o -iname '*.avi' \) | head -20)
if [ -n "$bad_videos" ]; then
  echo "error: video formats the Linux editor cannot import (convert to WebM/VP8):"
  echo "$bad_videos"
  exit 1
fi
# TextMesh Pro lives in a UPM package that is only resolved by the editor, so a tiny stub stands in for it.
{ find Assets -name '*.cs' -not -path '*/Editor/*'; echo .github/ci/TMProStub.cs; } | sed 's/.*/"&"/' > "$OUT/runtime.rsp"
find Assets -name '*.cs' -path '*/Editor/*' | sed 's/.*/"&"/' > "$OUT/editor.rsp"

compile() { # out defines extra-refs... @rsp
  local out=$1 defs=$2; shift 2
  "$MONO" "$CSC" -nologo -noconfig -nostdlib -target:library -langversion:7 -unsafe \
    -nowarn:$nowarn "-define:$defs" "${refs[@]}" "$@" -out:"$OUT/$out"
}

for platform in UNITY_STANDALONE_WIN UNITY_STANDALONE_OSX UNITY_STANDALONE_LINUX; do
  echo "== Assembly-CSharp ($platform)"
  compile "Assembly-CSharp-$platform.dll" "$defines;$platform" "@$OUT/runtime.rsp"
done

echo "== Assembly-CSharp-Editor"
compile Assembly-CSharp-EditorFlavor.dll "$defines;UNITY_STANDALONE_LINUX;UNITY_EDITOR" \
  "-r:$DATA/Managed/UnityEditor.dll" "@$OUT/runtime.rsp"
compile Assembly-CSharp-Editor.dll "$defines;UNITY_STANDALONE_LINUX;UNITY_EDITOR" \
  "-r:$DATA/Managed/UnityEditor.dll" "-r:$DATA/UnityExtensions/Unity/GUISystem/Editor/UnityEditor.UI.dll" \
  "-r:$OUT/Assembly-CSharp-EditorFlavor.dll" "@$OUT/editor.rsp"

# The weaver expects the assembly to be called Assembly-CSharp.dll.
echo "== UNET weaver"
mkdir -p "$OUT/weave/in" "$OUT/weave/out"
cp "$OUT/Assembly-CSharp-UNITY_STANDALONE_LINUX.dll" "$OUT/weave/in/Assembly-CSharp.dll"
"$MONO" "$CSC" -nologo -r:"$DATA/Managed/Unity.UNetWeaver.dll" -r:"$DATA/Managed/Unity.Cecil.dll" \
  -out:"$OUT/weave/UNetWeaveCheck.exe" .github/ci/UNetWeaveCheck.cs
cp "$DATA"/Managed/Unity.UNetWeaver.dll "$DATA"/Managed/Unity.Cecil*.dll "$OUT/weave/"
# The weaver's own trace output is very chatty; keep only the verdict, warnings and errors.
"$MONO" "$OUT/weave/UNetWeaveCheck.exe" "$DATA" "$OUT/weave/in/Assembly-CSharp.dll" "$OUT/weave/out" \
  | grep -E '^(error|warning): |UNET weaving' ; test "${PIPESTATUS[0]}" -eq 0

echo "All C# compiles and passes UNET weaving."
