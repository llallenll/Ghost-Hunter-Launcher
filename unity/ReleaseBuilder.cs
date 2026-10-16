using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Builds the Windows player into Builds/Windows for tools/publish-release.ps1 to zip and upload.
/// Lives outside the game project; publish-release.ps1 copies it into Assets/Editor for the build and removes it after.
/// To get the menu item in the editor, copy this file into the project's Assets/Editor folder.
/// </summary>
public static class ReleaseBuilder
{
    public const string OutputDir = "Builds/Windows";

    [MenuItem("Ghost Hunter/Build Windows Release")]
    static void BuildFromMenu()
    {
        if (Build(PlayerSettings.bundleVersion))
            EditorUtility.RevealInFinder(Path.Combine(OutputDir, PlayerSettings.productName + ".exe"));
    }

    public static void BuildFromCommandLine()
    {
        var args = Environment.GetCommandLineArgs();
        var index = Array.IndexOf(args, "-releaseVersion");
        var version = index >= 0 && index + 1 < args.Length ? args[index + 1] : PlayerSettings.bundleVersion;
        EditorApplication.Exit(Build(version) ? 0 : 1);
    }

    static bool Build(string version)
    {
        PlayerSettings.bundleVersion = version;
        AssetDatabase.SaveAssets();

        if (Directory.Exists(OutputDir)) Directory.Delete(OutputDir, true);

        var options = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = Path.Combine(OutputDir, PlayerSettings.productName + ".exe"),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
        };

        var report = BuildPipeline.BuildPlayer(options);
        var ok = report.summary.result == BuildResult.Succeeded;
        Debug.Log(ok
            ? $"[ReleaseBuilder] Built v{version} ({report.summary.totalSize / (1024 * 1024)} MB) to {OutputDir}"
            : $"[ReleaseBuilder] Build failed: {report.summary.result}, {report.summary.totalErrors} error(s)");
        return ok;
    }
}
