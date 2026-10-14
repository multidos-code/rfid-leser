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

            // Die Schrift rückt laut Layout in den weißen Rand des QR-Codes, darf aber nie in die
            // schwarzen Module ragen (bei kleinen QR-Codes ist der weiße Rand schmaler als 4 mm).
            Rechteck klartext = code.Klartext;
            double unterkanteModule = y0 + anzahl * modulGroesse;
            if (klartext.Y < unterkanteModule)
            {
                klartext = new Rechteck(klartext.X, unterkanteModule, klartext.Breite, klartext.Hoehe);
            }

            ZeichneKlartext(g, klartext, nummer);
            ZeichneSchnittlinie(g, code.Schnittbox);
        }

        /// <summary>
        /// Schreibt die Nummer zentriert in das Klartext-Feld. Ist sie breiter als das Feld
        /// (= Barcode-Breite bzw. QR-Kante), wird die Schrift automatisch verkleinert.
        /// </summary>
        private static void ZeichneKlartext(Graphics g, Rechteck feld, string text)
        {
            double groesse = Schriftgroesse.Passend(text, feld.Breite, KlartextSchriftMm,
                delegate(string t, double mm) { return TextBreiteMm(g, t, mm); });

            using (Font schrift = new Font("Consolas", (float)groesse, FontStyle.Regular, GraphicsUnit.Millimeter))
            // Gleiche Textmessung wie in TextBreiteMm (ohne Innenabstand) und kein Zeilenumbruch –
            // sonst bricht ein knapp passender Text auf zwei Zeilen um.
            using (StringFormat format = (StringFormat)StringFormat.GenericTypographic.Clone())
            {
                format.FormatFlags |= StringFormatFlags.NoWrap;
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                RectangleF bereich = new RectangleF((float)feld.X, (float)feld.Y, (float)feld.Breite, (float)feld.Hoehe);
                g.DrawString(text, schrift, Brushes.Black, bereich, format);
            }
        }

        /// <summary>Misst die Breite eines Texts in mm (Graphics steht auf Millimeter).</summary>
        private static double TextBreiteMm(Graphics g, string text, double groesseMm)
        {
            using (Font schrift = new Font("Consolas", (float)groesseMm, FontStyle.Regular, GraphicsUnit.Millimeter))
            {
                return g.MeasureString(text, schrift, PointF.Empty, StringFormat.GenericTypographic).Width;
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
