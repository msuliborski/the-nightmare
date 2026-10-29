using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Builds the game for desktop platforms. Used by CI (game-ci/unity-builder calls
// BuildFromCommandLine) and from the "The Nightmare/Build" menu for local builds.
public static class NightmareBuild
{
    private const string BUILD_NAME = "TheNightmare";

    [MenuItem("The Nightmare/Build/Windows (64-bit)")]
    public static void BuildWindows() { BuildLocal(BuildTarget.StandaloneWindows64); }

    [MenuItem("The Nightmare/Build/macOS")]
    public static void BuildMac() { BuildLocal(BuildTarget.StandaloneOSX); }

    [MenuItem("The Nightmare/Build/Linux (64-bit)")]
    public static void BuildLinux() { BuildLocal(BuildTarget.StandaloneLinux64); }

    [MenuItem("The Nightmare/Build/All desktop platforms")]
    public static void BuildAll()
    {
        BuildLocal(BuildTarget.StandaloneWindows64);
        BuildLocal(BuildTarget.StandaloneOSX);
        BuildLocal(BuildTarget.StandaloneLinux64);
    }

    private static void BuildLocal(BuildTarget target)
    {
        string path = Path.Combine("Builds", target.ToString(), BUILD_NAME + Extension(target));
        BuildReport report = Build(target, path, PlayerSettings.bundleVersion);
        if (report.summary.result == BuildResult.Succeeded)
            EditorUtility.RevealInFinder(path);
    }

    // Arguments passed by game-ci/unity-builder: -customBuildTarget, -customBuildPath, -buildVersion.
    public static void BuildFromCommandLine()
    {
        string[] args = Environment.GetCommandLineArgs();
        string targetName = GetArg(args, "-customBuildTarget") ?? GetArg(args, "-buildTarget");
        BuildTarget target = (BuildTarget) Enum.Parse(typeof(BuildTarget), targetName, true);
        string path = GetArg(args, "-customBuildPath")
                      ?? Path.Combine("build", target.ToString(), BUILD_NAME + Extension(target));
        string version = GetArg(args, "-buildVersion") ?? PlayerSettings.bundleVersion;

        BuildReport report = Build(target, path, version);
        EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }

    private static BuildReport Build(BuildTarget target, string path, string version)
    {
        string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        if (scenes.Length == 0) throw new InvalidOperationException("No scenes enabled in Build Settings.");

        PlayerSettings.bundleVersion = version;
        if (target == BuildTarget.StandaloneOSX && !path.EndsWith(".app")) path += ".app";
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));

        Debug.Log(string.Format("[NightmareBuild] Building {0} v{1} -> {2} ({3} scenes)", target, version, path, scenes.Length));
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = path,
            target = target,
            options = BuildOptions.None
        });
        Debug.Log(string.Format("[NightmareBuild] {0}: {1}, {2} errors, {3:F1} MB, {4}",
            target, report.summary.result, report.summary.totalErrors,
            report.summary.totalSize / (1024f * 1024f), report.summary.totalTime));
        return report;
    }

    private static string Extension(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64: return ".exe";
            case BuildTarget.StandaloneOSX: return ".app";
            default: return "";
        }
    }

    private static string GetArg(string[] args, string name)
    {
        int i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }
}
