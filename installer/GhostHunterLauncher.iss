; Installer for the Ghost Hunter Launcher. Built by ..\build.ps1 into bin\GhostHunterLauncherSetup.exe.
; Per-user install: no admin prompt, and the launcher can replace itself when it self-updates.

#define AppName "Ghost Hunter Launcher"
#define AppExe "GhostHunterLauncher.exe"
#define AppVersion GetVersionNumbersString(AddBackslash(SourcePath) + "..\bin\" + AppExe)

[Setup]
AppId={{6F3B2C1E-8D4A-4E7B-9C5F-2A1D0E8B7F43}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName}
AppPublisher=Ghost Hunter
AppPublisherURL=https://github.com/llallenll/Ghost-Hunter-Launcher
VersionInfoVersion={#AppVersion}
DefaultDirName={localappdata}\Programs\Ghost Hunter Launcher
DisableDirPage=yes
DisableProgramGroupPage=yes
DisableReadyPage=yes
PrivilegesRequired=lowest
OutputDir=..\bin
OutputBaseFilename=GhostHunterLauncherSetup
SetupIconFile=..\assets\icon.ico
UninstallDisplayIcon={app}\{#AppExe}
UninstallDisplayName=Ghost Hunter
WizardStyle=modern
Compression=lzma2/max
SolidCompression=yes
; Matches the launcher's single-instance mutex, so setup asks to close a running launcher.
AppMutex=GhostHunterLauncher.SingleInstance

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Shortcuts:"

[Files]
Source: "..\bin\{#AppExe}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\Ghost Hunter"; Filename: "{app}\{#AppExe}"
Name: "{autodesktop}\Ghost Hunter"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExe}"; Description: "Launch Ghost Hunter"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Leftovers from launcher self-updates, and the game files the launcher downloaded.
; Unity saves live in AppData\LocalLow and are kept.
Type: files; Name: "{app}\{#AppExe}.old"
Type: files; Name: "{app}\{#AppExe}.new"
Type: filesandordirs; Name: "{localappdata}\GhostHunter"
