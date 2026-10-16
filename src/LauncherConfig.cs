using System;
using System.IO;

namespace GhostHunterLauncher
{
    /// <summary>Everything a tester's launcher needs to know about where builds live.</summary>
    static class LauncherConfig
    {
        // Public GitHub repo whose Releases hold the game builds. Must be public: the launcher has no login.
        public const string RepoOwner = "llallenll";
        public const string RepoName = "Ghost-Hunter-Releases";

        // Zip asset attached to each release (see Tools/publish-release.ps1). Falls back to any .zip.
        public const string AssetName = "GhostHunter-Windows.zip";

        // Executable inside the zip (Unity names it after Player Settings > Product Name).
        public const string GameExeName = "Ghost Hunter.exe";

        // Early access builds are usually marked "pre-release" on GitHub; count them as updates.
        public const bool IncludePrereleases = true;

        public const string Title = "GHOST HUNTER";
        public const string Subtitle = "EARLY ACCESS";

        // %LOCALAPPDATA%\GhostHunter — per-user, no admin rights needed.
        public static readonly string InstallRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GhostHunter");
    }
}
