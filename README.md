# Ghost Hunter Launcher

Early-access launcher for Ghost Hunter. It checks GitHub Releases for the newest build, downloads and installs it, and launches the game. Players don't need to log in.

## How it works

- Reads releases from the public repo set in `src/LauncherConfig.cs` (`llallenll/Ghost-Hunter-Releases`) through the anonymous GitHub API.
- Picks the newest non-draft release (pre-releases included) that has a `GhostHunter-Windows.zip` asset.
- Installs to `%LOCALAPPDATA%\GhostHunter\Game`. The new build is extracted next to the old one and then swapped in, so a failed update never leaves a broken install. Unity saves in `LocalLow` aren't touched.
- Verifies the SHA-256 digest that GitHub reports for the asset.
- Still lets you play when offline if a build is already installed.

## Build

Requires Visual Studio 2022 (for its C# compiler). The output targets .NET Framework 4.x, which ships with Windows 10/11.

```powershell
.\build.ps1   # -> bin\GhostHunterLauncher.exe
```

Hand testers `bin\GhostHunterLauncher.exe`. Drop an `icon.ico` next to `build.ps1` to give it an icon.

## Publishing game builds

The game repo has `Tools/publish-release.ps1` and the **Ghost Hunter > Build Windows Release** menu item. Together they build the game, zip it and create the GitHub release.
