# Testskript: baut bin\Tests.exe aus tests\ und den Logik-Dateien aus src\ und führt sie aus.
# Exit-Code 0 = alle Tests grün, Exit-Code 1 = Fehler (Kompilierung oder Test).
# Aufruf:  pwsh -File .\test.ps1

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

# Logik-Dateien aus src\ (Programmstart und Oberfläche werden NICHT mitkompiliert)
$ausgeschlossen = @('Program.cs', 'Hauptfenster.cs')
$quellen = @()
$quellen += @(Get-ChildItem -Path (Join-Path $root 'src') -Filter '*.cs' -ErrorAction SilentlyContinue |
    Where-Object { $ausgeschlossen -notcontains $_.Name } | ForEach-Object { $_.FullName })
$quellen += @(Get-ChildItem -Path (Join-Path $root 'tests') -Filter '*.cs' | ForEach-Object { $_.FullName })

# QRCoder liegt in lib\ und wird neben die Test-exe kopiert
$qrDll = Join-Path $root 'lib\QRCoder.dll'
Copy-Item -Path $qrDll -Destination $bin -Force

$exe = Join-Path $bin 'Tests.exe'
$referenzen = @(
    'System.dll', 'System.Core.dll', 'System.Drawing.dll', 'System.Windows.Forms.dll',
    'System.Management.dll', $qrDll
) -join ','

$cscArgs = @(
    '/nologo', '/target:exe', '/langversion:5', '/codepage:65001', '/debug-',
    "/out:$exe", "/reference:$referenzen"
) + $quellen

Write-Host 'Kompiliere Tests ...'
& $csc @cscArgs
if ($LASTEXITCODE -ne 0) {
    Write-Host 'FEHLER: Kompilierung der Tests ist fehlgeschlagen.'
    exit 1
}

Write-Host 'Starte Tests ...'
& $exe
exit $LASTEXITCODE
