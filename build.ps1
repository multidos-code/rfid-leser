# Baut die Programm-exe bin\RFID-Leser.exe.
# QRCoder.dll (lib\) wird als Ressource eingebettet, damit die exe ohne DLL daneben läuft.
# Aufruf:  pwsh -File .\build.ps1                      (lokaler Build, Titel "Entwicklerversion")
#          pwsh -File .\build.ps1 -Version 1.1.1       (Release-Build, Version steht in Titel und Dateieigenschaften)
#          -Ausgabe <Pfad>  baut an einen anderen Ort als bin\RFID-Leser.exe (für Tests)

param(
    [string]$Version = '',
    [string]$Ausgabe = ''
)

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot

# Versionsnummer prüfen: Zahl.Zahl.Zahl (freigegeben) oder Zahl.Zahl.Zahl-SNAPSHOT (Testversion)
if ($Version -ne '' -and $Version -notmatch '^(\d+\.\d+\.\d+)(-SNAPSHOT)?$') {
    Write-Host "FEHLER: Ungültige Version '$Version'. Erwartet wird z. B. 1.2.0 oder 1.2.0-SNAPSHOT"
    exit 1
}
# Dateieigenschaften der exe erlauben nur Zahlen (1.2.0-SNAPSHOT -> 1.2.0)
$zahlenteil = if ($Version -ne '') { $Matches[1] } else { '' }

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
if ($Ausgabe -ne '') {
    $exe = $Ausgabe
    New-Item -ItemType Directory -Force -Path (Split-Path $exe) | Out-Null
}

$quellen = @(Get-ChildItem -Path (Join-Path $root 'src') -Filter '*.cs' | ForEach-Object { $_.FullName })

# Bei Release-Builds: Versionsangaben als zusätzliche Quelldatei erzeugen
# (erscheinen im Fenstertitel und in den Dateieigenschaften der exe)
if ($Version -ne '') {
    $versionDatei = Join-Path $bin 'VersionInfo.cs'
    @(
        'using System.Reflection;'
        "[assembly: AssemblyVersion(""$zahlenteil.0"")]"
        "[assembly: AssemblyFileVersion(""$zahlenteil.0"")]"
        "[assembly: AssemblyInformationalVersion(""$Version"")]"
        '[assembly: AssemblyProduct("RFID-Leser")]'
        '[assembly: AssemblyCompany("multidos Hamburg GmbH")]'
    ) | Set-Content -Path $versionDatei -Encoding utf8BOM
    $quellen += $versionDatei
}
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

$readme = Join-Path $root 'README.md'
if (-not (Test-Path $readme)) {
    Write-Host "FEHLER: README.md fehlt unter $readme"
    exit 1
}

$cscArgs = @(
    '/nologo', '/target:winexe', '/langversion:5', '/codepage:65001', '/optimize+',
    '/platform:anycpu', "/out:$exe", "/reference:$referenzen",
    "/resource:$qrDll,QRCoder.dll", "/resource:$readme,README.md"
) + $quellen

Write-Host ('Baue RFID-Leser.exe ' + $(if ($Version) { 'Version ' + $Version } else { '(Entwicklerversion)' }) + ' ...')
& $csc @cscArgs
if ($LASTEXITCODE -ne 0) {
    Write-Host 'FEHLER: Build fehlgeschlagen.'
    exit 1
}
Write-Host "Fertig: $exe"
exit 0
