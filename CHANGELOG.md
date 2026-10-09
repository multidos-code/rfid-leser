# Änderungsprotokoll RFID-Leser

Jede freigegebene Version braucht hier einen Abschnitt `## x.y.z`. Sein Text erscheint
auf der GitHub-Release-Seite. Bitte in einfacher Sprache beschreiben, was sich für die
Anwender ändert.

## 1.2.0

- Neues Freigabeverfahren: Eine Version wird nur noch veröffentlicht, wenn sie bewusst
  freigegeben wurde. Zwischenstände stehen als Testversion bereit und zeigen im
  Fenstertitel „(Testversion)“.

## 1.1.1

- Die Versionsnummer steht im Fenstertitel und in den Dateieigenschaften der exe.
- Die exe wird auf GitHub automatisch gebaut und unter „Releases“ zum Download bereitgestellt.

## 1.1

- Hilfe im Programm: Knopf „Hilfe“ oder Taste F1 öffnet eine Anleitung, inklusive
  Erklärung, warum immer nur ein Programm gleichzeitig auf den Reader zugreifen kann.

## 1.0

- Erste Version: Chip-Nummer vom RFID-Reader auslesen und anzeigen.
- Drucken der Nummer als Barcode (Code 128) und QR-Code auf A4, schwarz/weiß, mit
  gestrichelter Schnittlinie im Abstand von 2 cm.
- Größe der Codes einstellbar; die Werte werden gespeichert.
