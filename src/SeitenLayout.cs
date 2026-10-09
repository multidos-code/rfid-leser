using System;

namespace RfidLeser
{
    /// <summary>Rechteck in Millimetern, Ursprung oben links auf dem A4-Blatt.</summary>
    public class Rechteck
    {
        public Rechteck(double x, double y, double breite, double hoehe)
        {
            X = x;
            Y = y;
            Breite = breite;
            Hoehe = hoehe;
        }

        public double X { get; private set; }
        public double Y { get; private set; }
        public double Breite { get; private set; }
        public double Hoehe { get; private set; }
    }

    /// <summary>Position eines einzelnen Codes auf dem Blatt.</summary>
    public class CodeLayout
    {
        public CodeLayout(Rechteck code, Rechteck klartext, Rechteck schnittbox)
        {
            Code = code;
            Klartext = klartext;
            Schnittbox = schnittbox;
        }

        /// <summary>Fläche des eigentlichen Codes (Barcode oder QR).</summary>
        public Rechteck Code { get; private set; }

        /// <summary>Zeile mit der Nummer im Klartext, direkt unter dem Code.</summary>
        public Rechteck Klartext { get; private set; }

        /// <summary>Schnittlinie: Code plus Klartext plus 20 mm Rand.</summary>
        public Rechteck Schnittbox { get; private set; }
    }

    /// <summary>Ergebnis der Seitenberechnung.</summary>
    public class LayoutErgebnis
    {
        public LayoutErgebnis(CodeLayout barcode, CodeLayout qrCode, bool passt, string fehler)
        {
            Barcode = barcode;
            QrCode = qrCode;
            Passt = passt;
            Fehler = fehler;
        }

        public CodeLayout Barcode { get; private set; }
        public CodeLayout QrCode { get; private set; }

        /// <summary>true, wenn beide Schnittboxen vollständig und ohne Überlappung auf das Blatt passen.</summary>
        public bool Passt { get; private set; }

        /// <summary>Verständliche Fehlermeldung auf Deutsch; leer, wenn alles passt.</summary>
        public string Fehler { get; private set; }
    }

    /// <summary>Verteilt Barcode und QR-Code untereinander auf einem A4-Blatt (210 x 297 mm).</summary>
    public static class SeitenLayout
    {
        private const double BlattBreite = 210;
        private const double BlattHoehe = 297;
        private const double Blattrand = 5;      // Mindestabstand Schnittbox zum Blattrand (Druckerrand)
        private const double Schnittrand = 20;   // Abstand Code zur Schnittlinie, auf allen Seiten
        private const double Klartexthoehe = 6;  // Höhe der Klartextzeile unter dem Code
        private const double Abstand = 10;       // Abstand zwischen Barcode-Box und QR-Box
        private const double Oben = 10;          // Abstand des Gesamtblocks zum oberen Blattrand

        /// <summary>
        /// Berechnet die Positionen in mm. Maße sind die Code-Flächen ohne Klartext und Rand.
        /// Ist etwas ungültig oder passt nicht, liefert Passt = false und eine Fehlermeldung.
        /// </summary>
        public static LayoutErgebnis Berechne(double barcodeBreiteMm, double barcodeHoeheMm, double qrKanteMm)
        {
            if (!(barcodeBreiteMm > 0) || !(barcodeHoeheMm > 0) || !(qrKanteMm > 0))
            {
                return new LayoutErgebnis(null, null, false,
                    "Alle Größen müssen mehr als 0 mm sein.");
            }

            // Schnittboxen: Code plus Klartext plus 20 mm Rand auf allen Seiten
            double barcodeBoxBreite = barcodeBreiteMm + 2 * Schnittrand;
            double barcodeBoxHoehe = barcodeHoeheMm + Klartexthoehe + 2 * Schnittrand;
            double qrBoxBreite = qrKanteMm + 2 * Schnittrand;
            double qrBoxHoehe = qrKanteMm + Klartexthoehe + 2 * Schnittrand;

            // Horizontal zentriert, vertikal untereinander mit 10 mm Abstand
            double barcodeBoxX = (BlattBreite - barcodeBoxBreite) / 2;
            double barcodeBoxY = Oben;
            double qrBoxX = (BlattBreite - qrBoxBreite) / 2;
            double qrBoxY = barcodeBoxY + barcodeBoxHoehe + Abstand;

            CodeLayout barcode = BauePosition(barcodeBoxX, barcodeBoxY, barcodeBreiteMm, barcodeHoeheMm, barcodeBoxBreite, barcodeBoxHoehe);
            CodeLayout qr = BauePosition(qrBoxX, qrBoxY, qrKanteMm, qrKanteMm, qrBoxBreite, qrBoxHoehe);

            // Prüfen: Schnittboxen müssen innerhalb des Blatts mit 5 mm Rand liegen
            string fehler = PruefeInBlatt(barcode.Schnittbox, "Der Barcode");
            if (fehler == null)
            {
                fehler = PruefeInBlatt(qr.Schnittbox, "Der QR-Code");
            }
            if (fehler == null && Ueberlappt(barcode.Schnittbox, qr.Schnittbox))
            {
                fehler = "Barcode und QR-Code überlappen sich auf dem Blatt.";
            }

            if (fehler != null)
            {
                return new LayoutErgebnis(barcode, qr, false, fehler);
            }
            return new LayoutErgebnis(barcode, qr, true, string.Empty);
        }

        /// <summary>Erzeugt Code-, Klartext- und Schnittbox-Rechteck für eine Position.</summary>
        private static CodeLayout BauePosition(double boxX, double boxY, double codeBreite, double codeHoehe,
            double boxBreite, double boxHoehe)
        {
            Rechteck code = new Rechteck(boxX + Schnittrand, boxY + Schnittrand, codeBreite, codeHoehe);
            Rechteck klartext = new Rechteck(boxX + Schnittrand, boxY + Schnittrand + codeHoehe, codeBreite, Klartexthoehe);
            Rechteck schnittbox = new Rechteck(boxX, boxY, boxBreite, boxHoehe);
            return new CodeLayout(code, klartext, schnittbox);
        }

        /// <summary>Gibt null zurück, wenn die Box mit Mindestabstand auf dem Blatt liegt, sonst eine Meldung.</summary>
        private static string PruefeInBlatt(Rechteck box, string name)
        {
            if (box.X < Blattrand || box.Y < Blattrand ||
                box.X + box.Breite > BlattBreite - Blattrand ||
                box.Y + box.Hoehe > BlattHoehe - Blattrand)
            {
                return name + " passt nicht auf das A4-Blatt (Schnittlinie wäre zu nah am Rand oder außerhalb). " +
                    "Bitte die Größe verkleinern.";
            }
            return null;
        }

        /// <summary>true, wenn sich zwei Rechtecke überschneiden.</summary>
        private static bool Ueberlappt(Rechteck a, Rechteck b)
        {
            return a.X < b.X + b.Breite && b.X < a.X + a.Breite &&
                   a.Y < b.Y + b.Hoehe && b.Y < a.Y + a.Hoehe;
        }
    }
}
