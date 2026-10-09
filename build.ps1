# Baut die Programm-exe bin\RFID-Leser.exe.
# QRCoder.dll (lib\) wird als Ressource eingebettet, damit die exe ohne DLL daneben läuft.
# Aufruf:  pwsh -File .\build.ps1

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot

# Compiler von .NET Framework 4 (C# 5). Keine Downloads nötig.
$fwDir = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
$csc = Join-Path $fwDir 'csc.exe'
if (-not (Test-Path $csc)) {
    Write-Host "FEHLER: csc.exe nicht gefunden unter $csc"
    exit 1
}

$bin = Join-Path $root 'bin'
New-Item -ItemType Directory -Force -Path $bin | Out-Null
$exe = Join-Path $bin 'RFID-Leser.exe'

$quellen = @(Get-ChildItem -Path (Join-Path $root 'src') -Filter '*.cs' | ForEach-Object { $_.FullName })
if ($quellen.Count -eq 0) {
    Write-Host 'FEHLER: Keine Quelldateien in src\ gefunden.'
    exit 1
}

$qrDll = Join-Path $root 'lib\QRCoder.dll'
if (-not (Test-Path $qrDll)) {
    Write-Host "FEHLER: QRCoder.dll fehlt unter $qrDll"
    exit 1
}

$referenzen = @(
    'System.dll', 'System.Core.dll', 'System.Drawing.dll', 'System.Windows.Forms.dll',
    'System.Management.dll', $qrDll
) -join ','

$cscArgs = @(
    '/nologo', '/target:winexe', '/langversion:5', '/codepage:65001', '/optimize+',
    '/platform:anycpu', "/out:$exe", "/reference:$referenzen",
    "/resource:$qrDll,QRCoder.dll"
) + $quellen

Write-Host 'Baue RFID-Leser.exe ...'
& $csc @cscArgs
if ($LASTEXITCODE -ne 0) {
    Write-Host 'FEHLER: Build fehlgeschlagen.'
    exit 1
}
Write-Host "Fertig: $exe"
exit 0
