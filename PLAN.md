# Plan: RFID-Leser.exe (verbindlich)

## Ziel
Windows-Programm (eine einzige .exe, läuft ohne Installation auf Windows 10/11), das
die Nummer eines RFID-Chips einmalig von einem seriellen Reader liest, anzeigt und auf
Knopfdruck als Code-128-Barcode + QR-Code auf A4 (schwarz/weiß) druckt.
**Keine** Tastatur-Simulation.

## Fakten zum Reader (gemessen)
- FTDI USB-Serial (USB\VID_0403&PID_6001), am Testrechner COM3
- 9600 Baud, 8N1, kein Handshake
- Sendet ASCII-Hex-Nummer + CR LF, z. B. `0117ED48DB8\r\n` (11 Zeichen),
  ca. 3x pro Sekunde wiederholt, solange der Chip aufliegt.

## Technik (verbindlich)
- C# **5** (nur!), kompiliert mit `%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe`.
  Verboten, weil der Compiler sie nicht kennt: `$"..."`, `?.`, `=>`-Member, `nameof`,
  Auto-Property-Initialisierer, `out var`, Tupel, `using static`.
- WinForms + System.Drawing + System.Drawing.Printing, System.IO.Ports.
- QR-Code: `lib\QRCoder.dll` (QRCoder 1.4.3, net40, MIT). Wird mit `/resource:` in die exe
  eingebettet und per `AppDomain.CurrentDomain.AssemblyResolve` aus der Ressource geladen
  (Handler als ERSTE Anweisung in `Main`, QRCoder-Nutzung nur in eigener Klasse),
  sodass die exe ohne DLL daneben läuft.
- Kein NuGet, kein dotnet-SDK, keine weiteren Downloads.
- Quellcode-Dateien UTF-8 mit BOM, Kommentare Deutsch.

## Ordnerstruktur
```
rfid-leser/
  src/        Programmcode (Logik getrennt von Oberfläche)
  tests/      Testprogramm (eigene kleine Assert-Hilfe, Exit-Code 0 = alles grün)
  lib/        QRCoder.dll
  build.ps1   baut bin\RFID-Leser.exe
  test.ps1    baut + startet Tests, Exit-Code != 0 bei Fehler
  bin/        Ausgabe (in .gitignore)
```
Das Testprogramm kompiliert die Logik-Dateien aus `src/` (nicht `Program.cs`/Formular) mit.

## Bausteine (TDD: Test zuerst rot, dann grün, dann je 1 Commit)

1. **ChipParser** – `string ChipParser.ErsteNummer(string puffer)`:
   liefert die erste *vollständige* Zeile (abgeschlossen mit `\r\n` oder `\n`), getrimmt,
   die nur aus Hex-Zeichen 0-9A-F besteht (Großschreibung erzwingen) und 8–16 Zeichen lang ist;
   sonst `null`. Unvollständige Zeile am Anfang (Start mitten in einer Übertragung) wird
   übersprungen, d. h. die erste Zeile im Puffer wird nur akzeptiert, wenn sie gültig ist,
   ungültige/abgeschnittene werden ignoriert.
   Tests: normale Zeile, mehrere Zeilen, angeschnittene erste Zeile, fehlendes Zeilenende,
   Müllzeichen, leerer Puffer, Kleinbuchstaben.

2. **ReaderSuche** – `string ReaderSuche.FindePort(IEnumerable<PortInfo> ports)`:
   `PortInfo` hat `Name` (z. B. "COM3") und `GeraeteId` (PnP-ID). Bevorzugt Port, dessen
   GeraeteId `VID_0403` und `PID_6001` enthält (auch Schreibweise `VID_0403+PID_6001`);
   sonst null. Separat (nicht getestet) `ReaderSuche.AktuellePorts()` liest echte Ports über
   WMI `Win32_PnPEntity` (Name enthält "(COMx)").
   Tests: FTDI gefunden, mehrere Ports, kein passender Port, leere Liste.

3. **Code128** – `int[] Code128.Muster(string text)`: liefert die Modulbreiten
   (abwechselnd Strich/Lücke, beginnend mit Strich) inkl. Start-Code B, Daten,
   Prüfzeichen (Modulo 103), Stopp-Zeichen (13 Module inkl. Abschlussstrich).
   Vollständige Code-128-Mustertabelle (107 Einträge). Nur Code Set B (ASCII 32–127).
   Tests: Prüfzeichen für bekannte Beispiele ("PJJ123C" → Prüfwert 55: 104 + 48·1 + 42·2 + 42·3 + 17·4 + 18·5 + 19·6 + 35·7 = 879, 879 mod 103 = 55), Gesamtmodulzahl = 11*(n+2) + 13, Start B = 2,1,1,2,1,4,
   Stopp = 2,3,3,1,1,1,2, Exception bei ungültigen Zeichen.

4. **QrErzeuger** – `bool[,] QrErzeuger.Matrix(string text)`: über QRCoder, ECC-Level M,
   OHNE Ruhezone (Ruhezone macht das Layout). Tests: quadratisch, Größe 21 für
   "0117ED48DB8", die drei Positionsmuster (7x7-Rahmen) in den Ecken sind vorhanden.

5. **SeitenLayout** – `LayoutErgebnis SeitenLayout.Berechne(double barcodeBreiteMm,
   double barcodeHoeheMm, double qrKanteMm)`; A4 = 210 x 297 mm.
   - Jeder Code bekommt eine Schnittbox = Code-Fläche (+ Klartextzeile 6 mm darunter)
     + 20 mm Rand auf allen Seiten.
   - Barcode-Box oben, QR-Box darunter, beide horizontal zentriert, 10 mm Abstand
     zwischen den Boxen, Gesamtblock vertikal am oberen Seitenrand ab 10 mm.
   - Ergebnis enthält je Code: Rechteck des Codes, Rechteck der Klartextzeile,
     Rechteck der Schnittbox (alles in mm), plus `bool Passt` und `string Fehler`.
   - Schnittboxen müssen vollständig auf dem Blatt liegen (min. 5 mm zum Blattrand,
     Druckerrand) und dürfen sich nicht überlappen, sonst `Passt=false` mit
     verständlicher deutscher Fehlermeldung.
   - Ungültige Größen (<= 0) → `Passt=false`.
   Tests: Standardwerte (Barcode 80x25, QR 40) passen; Abstand Code↔Schnittlinie = 20 mm;
   keine Überlappung; zu groß (Barcode 200 mm breit) → passt nicht; 0 → passt nicht.

6. **Oberfläche + Druck** (nicht unit-getestet, manuell geprüft):
   - Fenster „RFID-Leser“: großes Anzeigefeld (Schrift ~28pt, nur lesen) für die Nummer,
     Statuszeile, Knöpfe **„Neu auslesen“** und **„Drucken“**.
   - Größenfelder (NumericUpDown, mm): Barcode-Breite (Std. 80), Barcode-Höhe (Std. 25),
     QR-Kante (Std. 40). Werte werden in `%APPDATA%\RFID-Leser\einstellungen.txt` gespeichert
     und beim Start geladen.
   - Programmstart = automatisch einmal auslesen.
   - „Neu auslesen“: Anzeige leeren, Port suchen, öffnen, im Hintergrund (BackgroundWorker
     oder Thread, Oberfläche darf nicht einfrieren) Daten sammeln bis `ChipParser` eine
     Nummer liefert oder 30 s Timeout; Port **danach schließen**. Status: „Bitte Chip
     auflegen…“, „Gelesen“, „Kein Reader gefunden“, „Zeitüberschreitung“, Port belegt → Meldung.
     Während des Lesens beide Knöpfe gesperrt.
   - „Drucken“: gesperrt ohne Nummer oder wenn Layout nicht passt (Fehler anzeigen).
     Standard-Windows-PrintDialog, A4 erzwingen, `Color=false`, Seitenränder 0 und
     Koordinaten mit `HardMarginX/Y` korrigieren, Einheit mm (`GraphicsUnit.Millimeter`).
     Zeichnen: Barcode-Striche und QR-Module als schwarze Rechtecke (Vektor, keine
     Bitmap-Skalierung) – Barcode mit Ruhezone innerhalb der angegebenen Breite
     (10 Module je Seite einrechnen), QR mit 4 Modulen Ruhezone innerhalb der Kante.
     Klartext (Nummer) zentriert unter jedem Code. Schnittbox als dünne gestrichelte Linie.
   - Optional Vorschau-Knopf ist NICHT gefordert – nicht einbauen.

7. **build.ps1 / test.ps1** fertig, exe bauen, `README.md` (Kurz-Bedienung, Bauen, Lizenzhinweis QRCoder MIT).

## Git
- Repo existiert, Branch `feature/erste-version`. Nach jedem grünen Baustein Commit,
  Nachricht Deutsch, beschreibt *was* (z. B. "Code-128-Erzeugung ergänzt"), endet mit
  Leerzeile + `Co-Authored-By: Claude Haiku 5.5 <noreply@anthropic.com>`.
- Nicht nach `main` mergen, nicht pushen.


---

# Erweiterung v1.1: Hilfe in der exe (verbindlich)

Branch `feature/hilfe`. README.md (Inhalt fertig, NICHT inhaltlich ändern) wird angezeigt.

8. **HilfeText** (`src\HilfeText.cs`, Logik, unit-getestet):
   - `List<HilfeZeile> HilfeText.Zerlege(string markdown)`; `HilfeZeile` hat
     `string Text` und `bool IstUeberschrift`.
   - Zeilen mit `#`, `##`, `###` am Anfang → Überschrift, Rauten + folgendes Leerzeichen entfernt.
   - `**` und Backticks (`) werden aus allen Zeilen entfernt.
   - Zeile `---` → Leerzeile. Leerzeilen bleiben erhalten. Listenpunkte (`- `, `1. `) bleiben unverändert.
   - `string HilfeText.LadeAusRessource()` liest die eingebettete Ressource `README.md`
     (UTF-8) aus der ausführenden Assembly; fehlt sie → verständlicher deutscher Ersatztext.
   Tests: H1/H2/H3 erkannt, ** entfernt, Backticks entfernt, --- wird leer, normale Zeile
   bleibt, Listenpunkt bleibt, leere Eingabe → leere Liste, null → leere Liste.

9. **Build + Ressource**: build.ps1 bettet `README.md` mit `/resource:...,README.md` ein.
   test.ps1 bettet sie ebenfalls ins Testprogramm ein und es gibt einen Test, dass
   `LadeAusRessource()` Text liefert, der „Reader und andere Programme“ enthält.
   Zusätzlich prüft test.ps1 NACH dem Build per Reflexion, dass `bin\RFID-Leser.exe`
   die Ressource `README.md` enthält (sonst Exit-Code != 0).

10. **Hilfefenster** (`src\Hilfefenster.cs`, nicht unit-getestet):
   - Eigenes Fenster „RFID-Leser – Hilfe“, ca. 700 x 600, größenveränderbar,
     RichTextBox nur lesen, Segoe UI 10pt, Überschriften fett 12pt, scrollbar, Knopf „Schließen“.
   - Hauptfenster: neuer Knopf „Hilfe“ (gut sichtbar, z. B. rechts neben/unter den
     anderen Knöpfen, Fenster ggf. vergrößern, nichts überlappen) und Taste F1
     (`KeyPreview = true`). Öffnet das Hilfefenster (nicht modal blockierend ist egal; ShowDialog ok).

Commits: (a) Tests+HilfeText, (b) Ressource in build/test, (c) Hilfefenster + Knopf.
