<#
.SYNOPSIS
  Builds Ghost Hunter for Windows, zips it, and publishes it as a GitHub release the launcher picks up.

.EXAMPLE
  .\tools\publish-release.ps1 -Version 0.2.0 -NotesFile notes.md
  .\tools\publish-release.ps1 -Version 0.2.1 -Notes "- Fixed door audio" -SkipBuild

.NOTES
  Needs the GitHub CLI, signed in once:  winget install GitHub.cli ; gh auth login
  Unity can't batch-build a project that's open in the editor. Either close Unity, or build the
  Windows player yourself into <project>\Builds\Windows and pass -SkipBuild.
  The build script (unity\ReleaseBuilder.cs) is copied into the project only for the build, then removed.
#>
param(
    [Parameter(Mandatory)] [string] $Version,
    [string] $Notes,
    [string] $NotesFile,
    [switch] $SkipBuild,
    [switch] $Stable,   # publish as a full release instead of an early-access pre-release
    [string] $Repo = 'llallenll/Ghost-Hunter-Releases',
    [string] $ProjectPath = 'D:\Unity\Ghost Hunter'
)
$ErrorActionPreference = 'Stop'
$project = (Resolve-Path $ProjectPath).Path
$buildDir = Join-Path $project 'Builds\Windows'
$zipPath = Join-Path $project 'Builds\GhostHunter-Windows.zip'
$tag = "v$($Version.TrimStart('v'))"

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    throw "GitHub CLI not found. Run: winget install GitHub.cli ; gh auth login"
}

if (-not $SkipBuild) {
    $editorVersion = (Select-String -Path (Join-Path $project 'ProjectSettings\ProjectVersion.txt') -Pattern '^m_EditorVersion: (.+)$').Matches[0].Groups[1].Value
    $unity = "C:\Program Files\Unity\Hub\Editor\$editorVersion\Editor\Unity.exe"
    if (-not (Test-Path $unity)) { throw "Unity $editorVersion not found at $unity" }

    $lock = Join-Path $project 'Temp\UnityLockfile'
    if (Test-Path $lock) {
        try { [IO.File]::Open($lock, 'Open', 'ReadWrite', 'None').Close() }
        catch { throw "The project is open in Unity. Close it, or build into Builds\Windows yourself and pass -SkipBuild." }
    }

    # Unity only runs editor scripts inside Assets, so drop the builder in for this build only.
    $builderDir = Join-Path $project 'Assets\Editor\ReleaseBuilderTemp'
    New-Item -ItemType Directory -Force $builderDir | Out-Null
    Copy-Item (Join-Path (Split-Path $PSScriptRoot -Parent) 'unity\ReleaseBuilder.cs') $builderDir

    $log = Join-Path $project 'Logs\release-build.log'
    Write-Host "Building $tag with Unity $editorVersion (log: $log)..."
    try {
        $p = Start-Process $unity -Wait -PassThru -ArgumentList @(
            '-batchmode', '-nographics', '-projectPath', "`"$project`"",
            '-executeMethod', 'ReleaseBuilder.BuildFromCommandLine', '-releaseVersion', $Version.TrimStart('v'),
            '-logFile', "`"$log`"")
    }
    finally {
        Remove-Item $builderDir -Recurse -Force -ErrorAction SilentlyContinue
        Remove-Item "$builderDir.meta" -Force -ErrorAction SilentlyContinue
    }
    if ($p.ExitCode -ne 0) { throw "Unity build failed (exit $($p.ExitCode)). See $log" }
}

if (-not (Get-ChildItem $buildDir -Filter *.exe -ErrorAction SilentlyContinue)) { throw "No build found in $buildDir" }

# Zip the build, leaving out the folders Unity marks as not-for-shipping.
Write-Host "Zipping $buildDir..."
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
if (Test-Path $zipPath) { Remove-Item $zipPath }
$zip = [IO.Compression.ZipFile]::Open($zipPath, 'Create')
try {
    $root = (Resolve-Path $buildDir).Path.TrimEnd('\') + '\'
    Get-ChildItem $buildDir -Recurse -File |
        Where-Object { $_.FullName -notmatch '_DoNotShip\\|_ButDontShipItWithYourGame\\' } |
        ForEach-Object {
            $entry = $_.FullName.Substring($root.Length).Replace('\', '/')
            [void][IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $_.FullName, $entry, 'Optimal')
        }
}
finally { $zip.Dispose() }
Write-Host ("Zip is {0:N1} MB" -f ((Get-Item $zipPath).Length / 1MB))

$ghArgs = @('release', 'create', $tag, $zipPath, '--repo', $Repo, '--title', "Ghost Hunter $tag")
if ($NotesFile) { $ghArgs += @('--notes-file', $NotesFile) }
elseif ($Notes) { $ghArgs += @('--notes', $Notes) }
else { $ghArgs += @('--notes', "Early access build $tag.") }
if (-not $Stable) { $ghArgs += '--prerelease' }

Write-Host "Publishing $tag to $Repo..."
& gh @ghArgs
if ($LASTEXITCODE -ne 0) { throw "gh release create failed." }
Write-Host "Done. Launchers will offer $tag on their next start."
