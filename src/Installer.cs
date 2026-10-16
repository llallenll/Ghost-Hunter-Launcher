using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace GhostHunterLauncher
{
    /// <summary>Owns the install folder: what version is there, swapping in a new build, finding the exe.</summary>
    static class Installer
    {
        public static string GameDir { get { return Path.Combine(LauncherConfig.InstallRoot, "Game"); } }
        static string StagingDir { get { return Path.Combine(LauncherConfig.InstallRoot, "Game.new"); } }
        static string OldDir { get { return Path.Combine(LauncherConfig.InstallRoot, "Game.old"); } }
        static string VersionFile { get { return Path.Combine(LauncherConfig.InstallRoot, "installed-version.txt"); } }
        public static string DownloadFile { get { return Path.Combine(LauncherConfig.InstallRoot, "download.zip.part"); } }

        /// <summary>Tag of the installed build, or null when nothing playable is installed.</summary>
        public static string InstalledTag
        {
            get
            {
                if (FindGameExe() == null || !File.Exists(VersionFile)) return null;
                var tag = File.ReadAllText(VersionFile).Trim();
                return tag.Length == 0 ? null : tag;
            }
        }

        public static string FindGameExe()
        {
            if (!Directory.Exists(GameDir)) return null;
            var preferred = Path.Combine(GameDir, LauncherConfig.GameExeName);
            if (File.Exists(preferred)) return preferred;
            return Directory.GetFiles(GameDir, "*.exe")
                .FirstOrDefault(p => !Path.GetFileName(p).StartsWith("UnityCrashHandler", StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsGameRunning()
        {
            var exe = FindGameExe();
            if (exe == null) return false;
            foreach (var process in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exe)))
            {
                try
                {
                    if (string.Equals(process.MainModule.FileName, exe, StringComparison.OrdinalIgnoreCase)) return true;
                }
                catch { return true; } // Can't inspect it; assume it's ours rather than corrupt a running install.
                finally { process.Dispose(); }
            }
            return false;
        }

        public static void EnsureRoot()
        {
            Directory.CreateDirectory(LauncherConfig.InstallRoot);
        }

        /// <summary>Extracts the zip beside the current install, then swaps it in so a failure never leaves a half-updated game.</summary>
        public static void Install(string zipPath, string tag, IProgress<double> progress)
        {
            DeleteDir(StagingDir);
            Directory.CreateDirectory(StagingDir);
            var stagingRoot = Path.GetFullPath(StagingDir) + Path.DirectorySeparatorChar;

            using (var zip = ZipFile.OpenRead(zipPath))
            {
                long total = zip.Entries.Sum(e => e.Length), done = 0;
                foreach (var entry in zip.Entries)
                {
                    var target = Path.GetFullPath(Path.Combine(StagingDir, entry.FullName));
                    if (!target.StartsWith(stagingRoot, StringComparison.OrdinalIgnoreCase))
                        throw new LauncherException("The download contains an invalid file path.");

                    if (entry.FullName.EndsWith("/") || entry.FullName.EndsWith("\\"))
                    {
                        Directory.CreateDirectory(target);
                        continue;
                    }
                    Directory.CreateDirectory(Path.GetDirectoryName(target));
                    entry.ExtractToFile(target, true);
                    done += entry.Length;
                    if (total > 0) progress.Report((double)done / total);
                }
            }

            // Tolerate zips that wrap everything in one top-level folder.
            var contentDir = StagingDir;
            var files = Directory.GetFiles(StagingDir);
            var dirs = Directory.GetDirectories(StagingDir);
            if (files.Length == 0 && dirs.Length == 1) contentDir = dirs[0];

            DeleteDir(OldDir);
            if (Directory.Exists(GameDir))
            {
                try { Directory.Move(GameDir, OldDir); }
                catch (IOException) { throw new LauncherException("Couldn't replace the old version. Close the game and try again."); }
                catch (UnauthorizedAccessException) { throw new LauncherException("Couldn't replace the old version. Close the game and try again."); }
            }

            Directory.Move(contentDir, GameDir);
            File.WriteAllText(VersionFile, tag);
            DeleteDir(StagingDir);
            DeleteDir(OldDir);
        }

        public static void CleanupLeftovers()
        {
            TryDelete(DownloadFile);
            DeleteDir(StagingDir);
            DeleteDir(OldDir);
        }

        public static void TryDelete(string file)
        {
            try { if (File.Exists(file)) File.Delete(file); } catch { }
        }

        static void DeleteDir(string dir)
        {
            try { if (Directory.Exists(dir)) Directory.Delete(dir, true); } catch { }
        }
    }
}
