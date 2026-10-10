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

$readme = Join-Path $root 'README.md'
if (-not (Test-Path $readme)) {
    Write-Host "FEHLER: README.md fehlt unter $readme"
    exit 1
}

$cscArgs = @(
    '/nologo', '/target:exe', '/langversion:5', '/codepage:65001', '/debug-',
    "/out:$exe", "/reference:$referenzen", "/resource:$readme,README.md"
) + $quellen

Write-Host 'Kompiliere Tests ...'
& $csc @cscArgs
if ($LASTEXITCODE -ne 0) {
    Write-Host 'FEHLER: Kompilierung der Tests ist fehlgeschlagen.'
    exit 1
}

# Programm-exe bauen (build.ps1) und prüfen, ob die Hilfe-Datei README.md eingebettet ist.
# Die Prüfung läuft per Reflexion (Metadaten lesen), die exe wird dafür NICHT gestartet.
Write-Host 'Baue Programm-exe (build.ps1) ...'
& (Join-Path $root 'build.ps1')
$ergebnisExe = 0
if ($LASTEXITCODE -ne 0) {
    Write-Host 'FEHLER: build.ps1 ist fehlgeschlagen.'
    $ergebnisExe = 1
} else {
    $programmExe = Join-Path $bin 'RFID-Leser.exe'
    $ressourcen = [System.Reflection.Assembly]::LoadFile($programmExe).GetManifestResourceNames()
    if ($ressourcen -contains 'README.md') {
        Write-Host 'OK      RFID-Leser.exe enthält die Ressource README.md'
    } else {
        Write-Host 'FEHLER  RFID-Leser.exe enthält die Ressource README.md nicht'
        $ergebnisExe = 1
    }
}

# Versionsnummer: Ein Build mit -Version muss die Version in die exe eintragen,
# eine ungültige Version muss den Build abbrechen. Gebaut wird in einen eigenen Ordner,
# damit die normale bin\RFID-Leser.exe unverändert bleibt.
Write-Host 'Prüfe Versionsnummer im Build ...'
$versionsExe = Join-Path $bin 'versionstest\RFID-Leser.exe'
& (Join-Path $root 'build.ps1') -Version '9.8.7' -Ausgabe $versionsExe
if ($LASTEXITCODE -ne 0 -or -not (Test-Path $versionsExe)) {
    Write-Host 'FEHLER  Build mit -Version 9.8.7 ist fehlgeschlagen'
    $ergebnisExe = 1
} else {
    $attribut = [System.Reflection.Assembly]::LoadFile($versionsExe).GetCustomAttributes(
        [System.Reflection.AssemblyInformationalVersionAttribute], $false)
    if ($attribut.Count -eq 1 -and $attribut[0].InformationalVersion -eq '9.8.7') {
        Write-Host 'OK      Build mit -Version 9.8.7 trägt die Version in die exe ein'
    } else {
        Write-Host 'FEHLER  Build mit -Version 9.8.7 hat die Version nicht eingetragen'
        $ergebnisExe = 1
    }
}
$snapshotExe = Join-Path $bin 'versionstest\Snapshot.exe'
& (Join-Path $root 'build.ps1') -Version '9.8.7-SNAPSHOT' -Ausgabe $snapshotExe *> $null
if ($LASTEXITCODE -eq 0 -and (Test-Path $snapshotExe)) {
    $snapshot = [System.Reflection.Assembly]::LoadFile($snapshotExe)
    $info = $snapshot.GetCustomAttributes([System.Reflection.AssemblyInformationalVersionAttribute], $false)
    $datei = $snapshot.GetCustomAttributes([System.Reflection.AssemblyFileVersionAttribute], $false)
    if ($info.Count -eq 1 -and $info[0].InformationalVersion -eq '9.8.7-SNAPSHOT' -and
        $datei.Count -eq 1 -and $datei[0].Version -eq '9.8.7.0') {
        Write-Host 'OK      Build mit Testversion 9.8.7-SNAPSHOT trägt Version korrekt ein'
    } else {
        Write-Host 'FEHLER  Build mit Testversion 9.8.7-SNAPSHOT hat die Version falsch eingetragen'
        $ergebnisExe = 1
    }
} else {
    Write-Host 'FEHLER  Build mit Testversion 9.8.7-SNAPSHOT ist fehlgeschlagen'
    $ergebnisExe = 1
}
& (Join-Path $root 'build.ps1') -Version 'v1.x' -Ausgabe (Join-Path $bin 'versionstest\Ungueltig.exe') *> $null
if ($LASTEXITCODE -ne 0) {
    Write-Host 'OK      Build mit ungültiger Version wird abgebrochen'
} else {
    Write-Host 'FEHLER  Build mit ungültiger Version wurde nicht abgebrochen'
    $ergebnisExe = 1
}

# release-info.ps1 muss bei Erfolg ausdrücklich Exit-Code 0 melden – auch wenn es (wie im
# GitHub-Ablauf) direkt in einer PowerShell-Sitzung aufgerufen wird, in der vorher noch
# kein Exit-Code gesetzt war.
$global:LASTEXITCODE = $null
$null = & (Join-Path $root 'release-info.ps1')
if ($LASTEXITCODE -eq 0) {
    Write-Host 'OK      release-info.ps1 meldet bei Erfolg Exit-Code 0'
} else {
    Write-Host "FEHLER  release-info.ps1 meldet bei Erfolg Exit-Code <$LASTEXITCODE> statt 0"
    $ergebnisExe = 1
}

Write-Host 'Starte Tests ...'
& $exe
$ergebnisTests = $LASTEXITCODE

# Gesamtergebnis: nur grün, wenn Programm-exe UND Tests in Ordnung sind
if ($ergebnisExe -ne 0 -or $ergebnisTests -ne 0) {
    exit 1
}
exit 0
