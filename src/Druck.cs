using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace RfidLeser
{
    /// <summary>
    /// Zeichnet Barcode, QR-Code, Klartext und Schnittlinien als schwarze Vektor-Rechtecke.
    /// Erwartet ein Graphics-Objekt, dessen PageUnit auf Millimeter steht.
    /// </summary>
    public static class Druck
    {
        private const int BarcodeRuhezoneModule = 10;  // Ruhezone je Seite beim Barcode
        private const int QrRuhezoneModule = 4;        // Ruhezone je Seite beim QR-Code
        private const float KlartextSchriftMm = 4.2f;  // Schriftgröße des Klartexts in mm
        private const float SchnittlinieMm = 0.2f;     // Strichstärke der Schnittlinie in mm

        /// <summary>Zeichnet beide Codes mit Klartext und Schnittlinien.</summary>
        public static void ZeichneSeite(Graphics g, LayoutErgebnis layout, string nummer)
        {
            ZeichneBarcode(g, layout.Barcode, nummer);
            ZeichneQrCode(g, layout.QrCode, nummer);
        }

        /// <summary>Barcode: jeder Strich ist ein schwarzes Rechteck, Ruhezone innerhalb der Breite.</summary>
        private static void ZeichneBarcode(Graphics g, CodeLayout code, string nummer)
        {
            int[] muster = Code128.Muster(nummer);
            double gesamtModule = muster.Sum() + 2 * BarcodeRuhezoneModule;
            double modulBreite = code.Code.Breite / gesamtModule;

            double x = code.Code.X + BarcodeRuhezoneModule * modulBreite;
            bool strich = true;
            foreach (int breite in muster)
            {
                double w = breite * modulBreite;
                if (strich)
                {
                    g.FillRectangle(Brushes.Black, (float)x, (float)code.Code.Y, (float)w, (float)code.Code.Hoehe);
                }
                x += w;
                strich = !strich;
            }

            ZeichneKlartext(g, code.Klartext, nummer);
            ZeichneSchnittlinie(g, code.Schnittbox);
        }

        /// <summary>QR-Code: jedes schwarze Modul ist ein Rechteck, Ruhezone innerhalb der Kante.</summary>
        private static void ZeichneQrCode(Graphics g, CodeLayout code, string nummer)
        {
            bool[,] module = QrErzeuger.Matrix(nummer);
            int anzahl = module.GetLength(0);
            double modulGroesse = code.Code.Breite / (anzahl + 2 * QrRuhezoneModule);
            double x0 = code.Code.X + QrRuhezoneModule * modulGroesse;
            double y0 = code.Code.Y + QrRuhezoneModule * modulGroesse;

            for (int zeile = 0; zeile < anzahl; zeile++)
            {
                for (int spalte = 0; spalte < anzahl; spalte++)
                {
                    if (module[zeile, spalte])
                    {
                        g.FillRectangle(Brushes.Black,
                            (float)(x0 + spalte * modulGroesse),
                            (float)(y0 + zeile * modulGroesse),
                            (float)modulGroesse,
                            (float)modulGroesse);
                    }
                }
            }

            ZeichneKlartext(g, code.Klartext, nummer);
            ZeichneSchnittlinie(g, code.Schnittbox);
        }

        /// <summary>Schreibt die Nummer zentriert in das Klartext-Feld.</summary>
        private static void ZeichneKlartext(Graphics g, Rechteck feld, string text)
        {
            using (Font schrift = new Font("Consolas", KlartextSchriftMm, FontStyle.Regular, GraphicsUnit.Millimeter))
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                RectangleF bereich = new RectangleF((float)feld.X, (float)feld.Y, (float)feld.Breite, (float)feld.Hoehe);
                g.DrawString(text, schrift, Brushes.Black, bereich, format);
            }
        }

        /// <summary>Dünne gestrichelte Linie als Schnittmarke.</summary>
        private static void ZeichneSchnittlinie(Graphics g, Rechteck box)
        {
            using (Pen stift = new Pen(Color.Black, SchnittlinieMm))
            {
                stift.DashStyle = DashStyle.Dash;
                g.DrawRectangle(stift, (float)box.X, (float)box.Y, (float)box.Breite, (float)box.Hoehe);
            }
        }
    }
}
