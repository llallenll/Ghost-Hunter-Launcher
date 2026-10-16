# Ghost Hunter Launcher

Early-access launcher for Ghost Hunter. It installs and updates the game from GitHub Releases, launches it, and keeps itself up to date. Players don't need to log in.

## Giving it to testers

Send them the installer:

https://github.com/llallenll/Ghost-Hunter-Launcher/releases/latest/download/GhostHunterLauncherSetup.exe

It installs per user (no admin prompt) to `%LOCALAPPDATA%\Programs\Ghost Hunter Launcher`, adds Start menu and desktop shortcuts, and appears in Apps & features. Uninstalling also removes the downloaded game. Unity saves in `AppData\LocalLow` are kept.

The exe isn't code-signed, so Windows SmartScreen shows "Windows protected your PC" the first time. Testers click **More info**, then **Run anyway**.

## How it works

- **Game updates:** reads releases from the public `llallenll/Ghost-Hunter-Releases` repo (set in `src/LauncherConfig.cs`) through the anonymous GitHub API. It picks the newest non-draft release, pre-releases included, that has a `GhostHunter-Windows.zip` asset, and installs it to `%LOCALAPPDATA%\GhostHunter\Game`. The new build is extracted next to the old one and then swapped in, so a failed update never leaves a broken install. It verifies the SHA-256 digest GitHub reports for the asset, and still lets you play offline.
- **Launcher updates:** on every start it checks this repo's releases. If the newest tag (for example `v1.0.1`) is higher than its own `AssemblyVersion`, it downloads `GhostHunterLauncher.exe` from that release, swaps it in for itself and restarts. Run the launcher with `--no-self-update` to skip this.

## Build

Requires Visual Studio 2022 (for its C# compiler) and Inno Setup (`winget install JRSoftware.InnoSetup`) for the installer.

```powershell
.\build.ps1            # -> bin\GhostHunterLauncher.exe and bin\GhostHunterLauncherSetup.exe
.\build.ps1 -NoInstaller
```

The output targets .NET Framework 4.x, which ships with Windows 10/11.

## Publishing a launcher update

1. Bump `AssemblyVersion` and `AssemblyFileVersion` in `src/AssemblyInfo.cs`.
2. Commit and push.
3. Run `.\tools\publish-launcher.ps1 -Notes "What changed"`.

The script builds the exe and the installer, then creates release `v<version>` here with both attached. Installed launchers update on their next start, and the installer link above now serves the new version.

## Logo

`assets/logo-source.png` is the original artwork. `tools/make-icon.ps1` cuts the hexagon out of its background and writes `assets/logo.png` (shown in the window) and `assets/icon.ico` (exe and installer icon). Rerun it after replacing the source.

## Publishing game builds

The game repo has `Tools/publish-release.ps1` and the **Ghost Hunter > Build Windows Release** menu item. Together they build the game, zip it and create the GitHub release.
