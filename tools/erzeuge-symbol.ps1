# Erzeugt das Programmsymbol assets\RFID-Leser.ico (Größen 16, 24, 32, 48, 64, 128, 256 Pixel).
# Motiv: blaues Quadrat, weiße Chipkarte mit goldenem Chip und Barcode-Strichen, Funkwellen.
# Jede Größe wird einzeln gezeichnet, damit auch kleine Symbole scharf bleiben.
# Aufruf:  pwsh -File .\tools\erzeuge-symbol.ps1   (nur nötig, wenn das Symbol geändert werden soll)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$ziel = Join-Path $PSScriptRoot '..\assets\RFID-Leser.ico'
$groessen = @(16, 24, 32, 48, 64, 128, 256)

# Farben
$blau  = [System.Drawing.Color]::FromArgb(255, 31, 78, 121)
$weiss = [System.Drawing.Color]::White
$gold  = [System.Drawing.Color]::FromArgb(255, 230, 175, 46)
$dunkel = [System.Drawing.Color]::FromArgb(255, 25, 40, 60)

# Abgerundetes Rechteck als Pfad
function Neues-RundRechteck([single]$x, [single]$y, [single]$b, [single]$h, [single]$r) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = 2 * $r
    $p.AddArc($x, $y, $d, $d, 180, 90)
    $p.AddArc($x + $b - $d, $y, $d, $d, 270, 90)
    $p.AddArc($x + $b - $d, $y + $h - $d, $d, $d, 0, 90)
    $p.AddArc($x, $y + $h - $d, $d, $d, 90, 90)
    $p.CloseFigure()
    return $p
}

# Zeichnet das Motiv in Größe s x s und liefert es als PNG-Bytes
function Zeichne-Symbol([int]$s) {
    $bild = New-Object System.Drawing.Bitmap $s, $s, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bild)
    $g.SmoothingMode = 'AntiAlias'
    $g.PixelOffsetMode = 'HighQuality'
    $g.Clear([System.Drawing.Color]::Transparent)
    $e = $s / 32.0   # Zeichnung auf 32er-Raster, skaliert

    # Hintergrund
    $hg = Neues-RundRechteck 0 0 $s $s (6 * $e)
    $g.FillPath((New-Object System.Drawing.SolidBrush $blau), $hg)

    # Chipkarte (leicht links unten)
    $karte = Neues-RundRechteck (3 * $e) (11 * $e) (20 * $e) (15 * $e) (2.2 * $e)
    $g.FillPath((New-Object System.Drawing.SolidBrush $weiss), $karte)

    # Goldener Chip mit Kontaktlinien
    $chip = Neues-RundRechteck (5.5 * $e) (14 * $e) (6.5 * $e) (5.5 * $e) (1.2 * $e)
    $g.FillPath((New-Object System.Drawing.SolidBrush $gold), $chip)
    if ($s -ge 32) {
        $kontakt = New-Object System.Drawing.Pen $dunkel, ([single](0.5 * $e))
        $g.DrawLine($kontakt, [single](8.75 * $e), [single](14 * $e), [single](8.75 * $e), [single](19.5 * $e))
        $g.DrawLine($kontakt, [single](5.5 * $e), [single](16.75 * $e), [single](12 * $e), [single](16.75 * $e))
    }

    # Barcode-Striche unten auf der Karte
    $striche = @(@(5.5, 1.0), @(7.3, 0.6), @(8.7, 1.4), @(11, 0.6), @(12.4, 1.0), @(14.2, 0.6), @(15.6, 1.4), @(17.8, 0.6), @(19.2, 1.0))
    if ($s -lt 32) { $striche = @(@(5.5, 1.5), @(8.5, 1.5), @(11.5, 1.5), @(14.5, 1.5), @(17.5, 1.5)) }
    $pinsel = New-Object System.Drawing.SolidBrush $dunkel
    foreach ($st in $striche) {
        $g.FillRectangle($pinsel, [single]($st[0] * $e), [single](21.5 * $e), [single]($st[1] * $e), [single](2.8 * $e))
    }

    # Funkwellen oben rechts
    $welle = New-Object System.Drawing.Pen $weiss, ([single]([Math]::Max(1.0, 2.0 * $e)))
    $welle.StartCap = 'Round'; $welle.EndCap = 'Round'
    $mx = 20 * $e; $my = 12 * $e
    $radien = if ($s -lt 24) { @(4.5, 8.5) } else { @(3.5, 6.5, 9.5) }
    foreach ($r in $radien) {
        $rr = $r * $e
        $g.DrawArc($welle, [single]($mx - $rr), [single]($my - $rr), [single](2 * $rr), [single](2 * $rr), 280, 80)
    }

    $g.Dispose()
    $ms = New-Object System.IO.MemoryStream
    $bild.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $bild.Dispose()
    return ,$ms.ToArray()
}

# ICO-Datei schreiben: Kopf, Verzeichnis, dann die PNG-Bilder
$bilder = @(foreach ($s in $groessen) { ,(Zeichne-Symbol $s) })
$datei = New-Object System.IO.MemoryStream
$w = New-Object System.IO.BinaryWriter $datei
$w.Write([UInt16]0); $w.Write([UInt16]1); $w.Write([UInt16]$groessen.Count)
$versatz = 6 + 16 * $groessen.Count
for ($i = 0; $i -lt $groessen.Count; $i++) {
    $s = $groessen[$i]; $daten = $bilder[$i]
    $w.Write([byte]($(if ($s -ge 256) { 0 } else { $s })))
    $w.Write([byte]($(if ($s -ge 256) { 0 } else { $s })))
    $w.Write([byte]0); $w.Write([byte]0)
    $w.Write([UInt16]1); $w.Write([UInt16]32)
    $w.Write([UInt32]$daten.Length); $w.Write([UInt32]$versatz)
    $versatz += $daten.Length
}
foreach ($daten in $bilder) { $w.Write($daten) }
$w.Flush()
[System.IO.File]::WriteAllBytes((Resolve-Path (Split-Path $ziel)).Path + '\RFID-Leser.ico', $datei.ToArray())
Write-Host "Symbol erzeugt: $ziel"
