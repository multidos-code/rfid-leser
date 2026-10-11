# RFID-Leser – Hilfe

Dieses Programm liest die Nummer eines RFID-Chips vom angeschlossenen Reader,
zeigt sie an und druckt sie auf Wunsch als Barcode (Code 128) und als QR-Code
auf ein A4-Blatt (schwarz/weiß).

Das Programm besteht aus einer einzigen Datei (RFID-Leser.exe). Es muss nicht
installiert werden: Datei auf den PC kopieren, Reader anschließen, starten.

## Bedienung

1. Den Reader per USB an den PC anschließen.
2. RFID-Leser.exe starten. Das Programm sucht den Reader selbst und wartet
   auf einen Chip. In der Statuszeile steht „Bitte Chip auflegen…“.
3. Den Chip auf den Reader legen. Nach ein bis zwei Sekunden erscheint die
   Nummer groß im Fenster, die Statuszeile zeigt „Gelesen“.
4. **Drucken** druckt die Nummer als Barcode und QR-Code (siehe unten).
5. **Neu auslesen** löscht die angezeigte Nummer und wartet auf den nächsten Chip.

Das Programm liest die Nummer nur einmal. Wird danach ein anderer Chip
aufgelegt, ändert sich die Anzeige nicht – dafür „Neu auslesen“ drücken.

## Drucken

- Nach Klick auf **Drucken** öffnet sich der normale Windows-Druckdialog.
  Dort den gewünschten Drucker auswählen und mit „Drucken“ bzw. „OK“ bestätigen.
- **Achtung:** Vorausgewählt ist der Windows-Standarddrucker. Das ist nicht immer
  ein Papierdrucker (z. B. ein Faxprogramm). Bitte den Drucker im Dialog prüfen.
- Mit „Abbrechen“ im Dialog wird nichts gedruckt.
- Gedruckt wird auf A4 ein Barcode und darunter ein QR-Code, jeweils mit der
  Nummer als Klartext darunter.
- Um jeden Code ist eine gestrichelte Schnittlinie mit 2 cm Abstand zum Code.

### Größe der Codes

Im Bereich „Größen in mm“ lassen sich Barcode-Breite, Barcode-Höhe und die
Kantenlänge des QR-Codes einstellen. Die Werte gelten für den Code selbst,
ohne den 2-cm-Rand. Standard: Barcode 80 × 25 mm, QR-Code 40 mm.

Die Werte werden gespeichert und beim nächsten Start wieder verwendet.
Sind die Codes zu groß für ein A4-Blatt, ist der Knopf „Drucken“ gesperrt und
der Grund steht in roter Schrift im Fenster.

## Reader und andere Programme

Windows erlaubt immer nur **einem Programm gleichzeitig** den Zugriff auf den
Reader. Ein reines „Mithören“, während ein anderes Programm den Reader nutzt,
ist nicht möglich – das gilt für jedes Programm, nicht nur für dieses.

So verhält sich RFID-Leser:

- Der Reader wird nur während des Auslesens belegt: beim Start und nach
  „Neu auslesen“.
- Sobald eine Nummer gelesen ist (meist nach 1–2 Sekunden), gibt das Programm
  den Reader sofort wieder frei. Liegt kein Chip auf, spätestens nach 30 Sekunden.
- Während die Nummer nur angezeigt oder gedruckt wird, ist der Reader frei.
  Andere Programme können ihn dann nutzen.

Was das im Alltag bedeutet:

- Nicht in zwei Programmen gleichzeitig auslesen.
- Hält ein anderes Programm den Reader dauerhaft fest (solange es geöffnet ist),
  kann RFID-Leser erst lesen, wenn das andere Programm geschlossen wurde.
- Umgekehrt kann ein anderes Programm in den wenigen Sekunden, in denen
  RFID-Leser gerade liest, nicht auf den Reader zugreifen.

## Meldungen und was zu tun ist

- **„Kein Reader gefunden“**: USB-Kabel des Readers prüfen und „Neu auslesen“
  drücken. Auf einem neuen PC kann Windows beim ersten Anschließen einige
  Sekunden brauchen, um den Treiber einzurichten. Klappt es dauerhaft nicht,
  fehlt eventuell der Treiber – bitte an die IT wenden.
- **„Der Reader … ist belegt“**: Ein anderes Programm nutzt den Reader gerade
  (siehe „Reader und andere Programme“). Das andere Programm schließen, dann
  „Neu auslesen“ drücken.
- **„Zeitüberschreitung“**: Innerhalb von 30 Sekunden wurde kein Chip erkannt.
  Chip auflegen und „Neu auslesen“ drücken.
- **„Drucken“ ist grau**: Es wurde noch keine Nummer gelesen, oder die
  eingestellten Größen passen nicht auf A4 (Grund steht in roter Schrift).

---

## Für Entwickler

### Bauen

Voraussetzung: Windows mit .NET Framework 4 (der Compiler `csc.exe` liegt unter
`%WINDIR%\Microsoft.NET\Framework64\v4.0.30319`). Es werden keine weiteren Programme geladen.

- Programm bauen: `pwsh -File .\build.ps1` (erzeugt `bin\RFID-Leser.exe`)
- Tests ausführen: `pwsh -File .\test.ps1` (Exit-Code 0 = alles grün)
- Lokal gebaute exe zeigt im Titel „(Entwicklerversion)“.

### Versionen, Testversionen und Releases

Die Version steht in `version.txt`. Bei jedem Push auf `main` (und bei Pull Requests)
läuft auf GitHub `.github/workflows/release.yml`: Tests → Build → exe als Download.
Schlägt ein Test fehl, wird nichts veröffentlicht.

- **Testversion** (`version.txt` = z. B. `1.2.0-SNAPSHOT`): Die exe liegt 7 Tage im
  GitHub-Lauf unter „Artifacts“ zum Download, zum Prüfen in einer Testumgebung.
  Kein Release. Im Fenstertitel steht „(Testversion)“.
- **Freigabe** (`version.txt` = z. B. `1.2.0`): Zusätzlich entsteht das GitHub-Release
  `v1.2.0` mit der exe. Der Text der Release-Seite kommt aus dem Abschnitt `## 1.2.0`
  in `CHANGELOG.md` – fehlt er, bricht der Lauf ab.
- Ablauf einer Freigabe:
  1. `CHANGELOG.md`: Abschnitt `## 1.2.0` in einfacher Sprache schreiben.
  2. `version.txt` auf `1.2.0` setzen, committen, nach `main` pushen → Release entsteht.
  3. `version.txt` auf die nächste geplante Version mit `-SNAPSHOT` setzen
     (z. B. `1.2.1-SNAPSHOT`), committen, pushen.
- Eine schon veröffentlichte Version wird nie überschrieben (Lauf gibt eine Warnung aus).
- Versionsnummern: letzte Stelle für Fehlerbehebungen (1.2.0 → 1.2.1),
  mittlere Stelle für neue Funktionen (1.2.0 → 1.3.0).
- Die Version steht im Fenstertitel und in den Dateieigenschaften der exe.
  Lokal gebaute exe: „(Entwicklerversion)“. Werkzeug: `pwsh -File .\release-info.ps1`.

### Technik

- Reader: FTDI USB-Seriell (VID 0403, PID 6001), 9600 Baud, 8N1. Sendet die
  Nummer als Hex-Text mit Zeilenende, wiederholt, solange der Chip aufliegt.
- Einstellungen: `%APPDATA%\RFID-Leser\einstellungen.txt`
- Diese README wird in die exe eingebettet und über „Hilfe“ (F1) angezeigt.

### Aufbau

- `src\` Programmcode (Logik getrennt von Oberfläche)
- `tests\` eigenes kleines Testprogramm
- `lib\QRCoder.dll` QR-Bibliothek, wird in die exe eingebettet

### Lizenzhinweis

Der QR-Code wird mit der Bibliothek **QRCoder** (Version 1.4.3, Lizenz MIT) erzeugt.
