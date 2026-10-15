# Änderungsprotokoll RFID-Leser

Jede freigegebene Version braucht hier einen Abschnitt `## x.y.z`. Sein Text erscheint
auf der GitHub-Release-Seite. Bitte in einfacher Sprache beschreiben, was sich für die
Anwender ändert.

## 1.3.0

- Eigenes Programmsymbol: RFID-Leser ist auf dem Desktop, im Explorer und in der
  Taskleiste an einer blauen Chipkarte mit Funkwellen zu erkennen.

## 1.2.1

- Neue Standardgrößen: Barcode 32 mm breit und 20 mm hoch, QR-Code 30 mm.
- Die Barcode-Höhe enthält jetzt die Schrift darunter: 20 mm sind Striche und
  Nummer zusammen.
- Beim QR-Code steht die Nummer 4 mm näher am Code.
- Ist die Nummer breiter als der Barcode oder der QR-Code, wird die Schrift
  automatisch kleiner, statt über den Code hinauszuragen.
- Hinweis: Wer schon eigene Größen gespeichert hatte, behält Breite und QR-Größe;
  die Barcode-Höhe springt einmalig auf den neuen Standard 20 mm, weil sie jetzt
  die Schrift mit einschließt.
- Neues Freigabeverfahren: Eine Version wird nur noch veröffentlicht, wenn sie bewusst
  freigegeben wurde. Zwischenstände stehen als Testversion bereit und zeigen im
  Fenstertitel „(Testversion)“. (Version 1.2.0 wurde dafür nicht einzeln veröffentlicht.)

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
