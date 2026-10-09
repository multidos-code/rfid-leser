# RFID-Leser

Windows-Programm, das die Nummer eines RFID-Chips vom seriellen Reader liest,
anzeigt und auf Knopfdruck als Code-128-Barcode und QR-Code auf A4 druckt.
Die Ausgabe ist eine einzelne Datei `bin\RFID-Leser.exe`, die ohne Installation läuft
(Windows 10/11, .NET Framework 4 ist vorhanden).

## Bedienung

1. Reader per USB anschließen (FTDI-Gerät, am Testrechner COM3). Das Programm sucht den Port selbst.
2. Programm starten: Es liest automatisch einmal aus. Den Chip auf den Reader legen.
3. Sobald die Nummer erscheint, zeigt die Statuszeile „Gelesen“ an.
4. Mit **Drucken** öffnet sich der Windows-Druckdialog (Papier A4, ohne Ränder).
5. Mit **Neu auslesen** wird ein neuer Chip gelesen.

Die Größen (Barcode-Breite, Barcode-Höhe, QR-Kantenlänge in mm) lassen sich im Fenster ändern.
Sie werden in `%APPDATA%\RFID-Leser\einstellungen.txt` gespeichert.
Standard: Barcode 80 x 25 mm, QR-Code 40 mm. Passt das Layout nicht auf A4,
ist der Druckknopf gesperrt und der Grund steht in roter Schrift.

## Bauen

Voraussetzung: Windows mit .NET Framework 4 (der Compiler `csc.exe` liegt unter
`%WINDIR%\Microsoft.NET\Framework64\v4.0.30319`). Es werden keine weiteren Programme geladen.

- Programm bauen: `pwsh -File .\build.ps1` (erzeugt `bin\RFID-Leser.exe`)
- Tests ausführen: `pwsh -File .\test.ps1` (Exit-Code 0 = alles grün)

## Aufbau

- `src\` Programmcode (Logik getrennt von Oberfläche)
- `tests\` eigenes kleines Testprogramm
- `lib\QRCoder.dll` QR-Bibliothek, wird in die exe eingebettet

## Lizenzhinweis

Der QR-Code wird mit der Bibliothek **QRCoder** (Version 1.4.3, Lizenz MIT) erzeugt.
