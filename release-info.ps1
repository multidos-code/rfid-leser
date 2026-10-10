# Liest version.txt und gibt die Angaben für den Release-Build aus:
#   version=1.2.0-SNAPSHOT
#   testversion=true
# Bei einer freigegebenen Version (ohne -SNAPSHOT) und angegebener -NotizenDatei wird der
# passende Abschnitt aus CHANGELOG.md in diese Datei geschrieben. Fehlt der Abschnitt, bricht
# das Skript mit Fehler ab – eine Freigabe braucht immer einen verständlichen Änderungstext.
# Nutzt dieselbe, getestete Logik wie das Programm (src\Versionsnummer.cs).
# Aufruf:  pwsh -File .\release-info.ps1 [-NotizenDatei release-notes.md]

param(
    [string]$NotizenDatei = ''
)

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot

Add-Type -Path (Join-Path $root 'src\Versionsnummer.cs')

$inhalt = Get-Content -Path (Join-Path $root 'version.txt') -Raw -Encoding utf8
try {
    $version = [RfidLeser.Versionsnummer]::AusVersionsdatei($inhalt)
} catch {
    Write-Host "FEHLER: $($_.Exception.InnerException.Message)"
    exit 1
}
$testversion = [RfidLeser.Versionsnummer]::IstTestversion($version)

if (-not $testversion -and $NotizenDatei -ne '') {
    $changelog = Get-Content -Path (Join-Path $root 'CHANGELOG.md') -Raw -Encoding utf8
    $abschnitt = [RfidLeser.Versionsnummer]::ChangelogAbschnitt($changelog, $version)
    if ($null -eq $abschnitt) {
        Write-Host "FEHLER: In CHANGELOG.md fehlt der Abschnitt '## $version' (oder er ist leer)."
        exit 1
    }
    Set-Content -Path $NotizenDatei -Value $abschnitt -Encoding utf8
}

"version=$version"
"testversion=$($testversion.ToString().ToLower())"

# Erfolg ausdrücklich melden (sonst bleibt der Exit-Code beim direkten Aufruf leer)
exit 0
