# Builds Launcher\bin\GhostHunterLauncher.exe with the C# compiler that ships with Visual Studio.
# Targets .NET Framework 4.x, which is built into Windows 10/11, so testers need nothing else installed.
$ErrorActionPreference = 'Stop'
$here = $PSScriptRoot

$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$csc = $null
if (Test-Path $vswhere) {
    $csc = & $vswhere -latest -products * -find 'MSBuild\**\Bin\Roslyn\csc.exe' | Select-Object -First 1
}
if (-not $csc) { throw "Couldn't find csc.exe. Install Visual Studio (any edition) with the .NET desktop workload." }

$out = Join-Path $here 'bin'
New-Item -ItemType Directory -Force $out | Out-Null
$exe = Join-Path $out 'GhostHunterLauncher.exe'

$compilerArgs = @(
    '/nologo', '/target:winexe', '/optimize+', '/langversion:latest',
    "/out:$exe",
    "/win32manifest:$(Join-Path $here 'app.manifest')",
    '/r:System.dll', '/r:System.Core.dll', '/r:System.Drawing.dll', '/r:System.Windows.Forms.dll',
    '/r:System.Net.Http.dll', '/r:System.IO.Compression.dll', '/r:System.IO.Compression.FileSystem.dll',
    '/r:System.Web.Extensions.dll'
)
$icon = Join-Path $here 'icon.ico'
if (Test-Path $icon) { $compilerArgs += "/win32icon:$icon" }
$compilerArgs += (Get-ChildItem (Join-Path $here 'src') -Filter *.cs).FullName

& $csc @compilerArgs
if ($LASTEXITCODE -ne 0) { throw "Launcher build failed." }
Write-Host "Built $exe"
