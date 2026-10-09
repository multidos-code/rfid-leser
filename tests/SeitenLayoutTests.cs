using System;

namespace RfidLeser.Tests
{
    /// <summary>Tests für SeitenLayout.Berechne (Baustein 5).</summary>
    public static class SeitenLayoutTests
    {
        public static void Alle()
        {
            Console.WriteLine("SeitenLayout:");

            Pruefer.Test("Standardwerte (Barcode 80x25, QR 40) passen aufs Blatt", () =>
            {
                LayoutErgebnis e = SeitenLayout.Berechne(80, 25, 40);
                Pruefer.IstWahr(e.Passt, "passt nicht: " + e.Fehler);
            });

            Pruefer.Test("bei Passen ist die Fehlermeldung leer", () =>
            {
                LayoutErgebnis e = SeitenLayout.Berechne(80, 25, 40);
                Pruefer.IstGleich(string.Empty, e.Fehler);
            });

            Pruefer.Test("Barcode: Abstand links Code zur Schnittlinie = 20 mm", () =>
            {
                CodeLayout b = SeitenLayout.Berechne(80, 25, 40).Barcode;
                Pruefer.IstNahe(20, b.Code.X - b.Schnittbox.X);
            });

            Pruefer.Test("Barcode: Abstand oben Code zur Schnittlinie = 20 mm", () =>
            {
                CodeLayout b = SeitenLayout.Berechne(80, 25, 40).Barcode;
                Pruefer.IstNahe(20, b.Code.Y - b.Schnittbox.Y);
            });

            Pruefer.Test("Barcode: Abstand rechts Code zur Schnittlinie = 20 mm", () =>
            {
                CodeLayout b = SeitenLayout.Berechne(80, 25, 40).Barcode;
                Pruefer.IstNahe(20, (b.Schnittbox.X + b.Schnittbox.Breite) - (b.Code.X + b.Code.Breite));
            });

            Pruefer.Test("Barcode: Abstand Klartext zur unteren Schnittlinie = 20 mm", () =>
            {
                CodeLayout b = SeitenLayout.Berechne(80, 25, 40).Barcode;
                Pruefer.IstNahe(20, (b.Schnittbox.Y + b.Schnittbox.Hoehe) - (b.Klartext.Y + b.Klartext.Hoehe));
            });

            Pruefer.Test("Klartextzeile sitzt direkt unter dem Code und ist 6 mm hoch", () =>
            {
                CodeLayout b = SeitenLayout.Berechne(80, 25, 40).Barcode;
                Pruefer.IstNahe(b.Code.Y + b.Code.Hoehe, b.Klartext.Y);
                Pruefer.IstNahe(6, b.Klartext.Hoehe);
            });

            Pruefer.Test("QR: Abstand links Code zur Schnittlinie = 20 mm", () =>
            {
                CodeLayout q = SeitenLayout.Berechne(80, 25, 40).QrCode;
                Pruefer.IstNahe(20, q.Code.X - q.Schnittbox.X);
            });

            Pruefer.Test("QR hat die angegebene Kantenlänge 40 mm", () =>
            {
                CodeLayout q = SeitenLayout.Berechne(80, 25, 40).QrCode;
                Pruefer.IstNahe(40, q.Code.Breite);
                Pruefer.IstNahe(40, q.Code.Hoehe);
            });

            Pruefer.Test("Gesamtblock beginnt 10 mm vom oberen Blattrand", () =>
            {
                LayoutErgebnis e = SeitenLayout.Berechne(80, 25, 40);
                Pruefer.IstNahe(10, e.Barcode.Schnittbox.Y);
            });

            Pruefer.Test("Barcode- und QR-Schnittbox sind horizontal zentriert", () =>
            {
                LayoutErgebnis e = SeitenLayout.Berechne(80, 25, 40);
                Pruefer.IstNahe(105, e.Barcode.Schnittbox.X + e.Barcode.Schnittbox.Breite / 2);
                Pruefer.IstNahe(105, e.QrCode.Schnittbox.X + e.QrCode.Schnittbox.Breite / 2);
            });

            Pruefer.Test("Abstand zwischen Barcode- und QR-Schnittbox = 10 mm", () =>
            {
                LayoutErgebnis e = SeitenLayout.Berechne(80, 25, 40);
                double untenBarcode = e.Barcode.Schnittbox.Y + e.Barcode.Schnittbox.Hoehe;
                Pruefer.IstNahe(10, e.QrCode.Schnittbox.Y - untenBarcode);
            });

            Pruefer.Test("Schnittboxen überlappen sich nicht", () =>
            {
                LayoutErgebnis e = SeitenLayout.Berechne(80, 25, 40);
                double untenBarcode = e.Barcode.Schnittbox.Y + e.Barcode.Schnittbox.Hoehe;
                Pruefer.IstWahr(untenBarcode <= e.QrCode.Schnittbox.Y, "Schnittboxen überlappen sich");
            });

            Pruefer.Test("Schnittboxen haben mindestens 5 mm Abstand zum Blattrand", () =>
            {
                LayoutErgebnis e = SeitenLayout.Berechne(80, 25, 40);
                Rechteck[] boxen = { e.Barcode.Schnittbox, e.QrCode.Schnittbox };
                foreach (Rechteck r in boxen)
                {
                    Pruefer.IstWahr(r.X >= 5, "zu weit links");
                    Pruefer.IstWahr(r.Y >= 5, "zu weit oben");
                    Pruefer.IstWahr(r.X + r.Breite <= 210 - 5, "zu weit rechts");
                    Pruefer.IstWahr(r.Y + r.Hoehe <= 297 - 5, "zu weit unten");
                }
            });

            Pruefer.Test("Barcode 200 mm breit passt nicht", () =>
            {
                LayoutErgebnis e = SeitenLayout.Berechne(200, 25, 40);
                Pruefer.IstWahr(!e.Passt, "sollte nicht passen");
                Pruefer.IstWahr(e.Fehler.Length > 0, "Fehlermeldung fehlt");
            });

            Pruefer.Test("QR mit 250 mm Kante passt nicht", () =>
            {
                LayoutErgebnis e = SeitenLayout.Berechne(80, 25, 250);
                Pruefer.IstWahr(!e.Passt, "sollte nicht passen");
            });

            Pruefer.Test("Barcode-Breite 0 passt nicht", () =>
            {
                LayoutErgebnis e = SeitenLayout.Berechne(0, 25, 40);
                Pruefer.IstWahr(!e.Passt, "sollte nicht passen");
                Pruefer.IstWahr(e.Fehler.Length > 0, "Fehlermeldung fehlt");
            });

            Pruefer.Test("negative QR-Kante passt nicht", () =>
            {
                LayoutErgebnis e = SeitenLayout.Berechne(80, 25, -5);
                Pruefer.IstWahr(!e.Passt, "sollte nicht passen");
            });
        }
    }
}
