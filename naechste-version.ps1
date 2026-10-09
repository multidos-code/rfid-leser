# Gibt die nächste Versionsnummer aus (z. B. 1.1.1), berechnet aus den vorhandenen Git-Versionsmarken.
# Nutzt dieselbe, getestete Logik wie das Programm (src\Versionsnummer.cs).
# Aufruf:  pwsh -File .\naechste-version.ps1

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot

Add-Type -Path (Join-Path $root 'src\Versionsnummer.cs')

$tags = @(git -C $root tag --list 'v*')
if ($LASTEXITCODE -ne 0) {
    Write-Error 'Git-Versionsmarken konnten nicht gelesen werden.'
    exit 1
}

[RfidLeser.Versionsnummer]::Naechste([string[]]$tags)
