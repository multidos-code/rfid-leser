using System;

namespace RfidLeser.Tests
{
    /// <summary>Tests für SeitenLayout.Berechne (Baustein 5).</summary>
    public static class SeitenLayoutTests
    {
        public static void Alle()
        {
            Console.WriteLine("SeitenLayout:");

            Pruefer.Test("Standardwerte (Barcode 32x20, QR 30) passen aufs Blatt", () =>
            {
                LayoutErgebnis e = SeitenLayout.Berechne(32, 20, 30);
                Pruefer.IstWahr(e.Passt, "passt nicht: " + e.Fehler);
            });

            Pruefer.Test("bei Passen ist die Fehlermeldung leer", () =>
            {
                LayoutErgebnis e = SeitenLayout.Berechne(32, 20, 30);
                Pruefer.IstGleich(string.Empty, e.Fehler);
            });

            Pruefer.Test("Barcode: Abstand links Code zur Schnittlinie = 20 mm", () =>
            {
                CodeLayout b = SeitenLayout.Berechne(32, 20, 30).Barcode;
                Pruefer.IstNahe(20, b.Code.X - b.Schnittbox.X);
            });

            Pruefer.Test("Barcode: Abstand oben Code zur Schnittlinie = 20 mm", () =>
            {
                CodeLayout b = SeitenLayout.Berechne(32, 20, 30).Barcode;
                Pruefer.IstNahe(20, b.Code.Y - b.Schnittbox.Y);
            });

            Pruefer.Test("Barcode: Abstand rechts Code zur Schnittlinie = 20 mm", () =>
            {
                CodeLayout b = SeitenLayout.Berechne(32, 20, 30).Barcode;
                Pruefer.IstNahe(20, (b.Schnittbox.X + b.Schnittbox.Breite) - (b.Code.X + b.Code.Breite));
            });

            Pruefer.Test("Barcode: Abstand Klartext zur unteren Schnittlinie = 20 mm", () =>
            {
                CodeLayout b = SeitenLayout.Berechne(32, 20, 30).Barcode;
                Pruefer.IstNahe(20, (b.Schnittbox.Y + b.Schnittbox.Hoehe) - (b.Klartext.Y + b.Klartext.Hoehe));
            });

            Pruefer.Test("Klartextzeile sitzt direkt unter dem Code und ist 6 mm hoch", () =>
            {
                CodeLayout b = SeitenLayout.Berechne(32, 20, 30).Barcode;
                Pruefer.IstNahe(b.Code.Y + b.Code.Hoehe, b.Klartext.Y);
                Pruefer.IstNahe(6, b.Klartext.Hoehe);
            });

            Pruefer.Test("QR: Abstand links Code zur Schnittlinie = 20 mm", () =>
            {
                CodeLayout q = SeitenLayout.Berechne(32, 20, 30).QrCode;
                Pruefer.IstNahe(20, q.Code.X - q.Schnittbox.X);
            });

            Pruefer.Test("QR hat die angegebene Kantenlänge 30 mm", () =>
            {
                CodeLayout q = SeitenLayout.Berechne(32, 20, 30).QrCode;
                Pruefer.IstNahe(30, q.Code.Breite);
                Pruefer.IstNahe(30, q.Code.Hoehe);
            });

            Pruefer.Test("Gesamtblock beginnt 10 mm vom oberen Blattrand", () =>
            {
                LayoutErgebnis e = SeitenLayout.Berechne(32, 20, 30);
                Pruefer.IstNahe(10, e.Barcode.Schnittbox.Y);
            });

            Pruefer.Test("Barcode- und QR-Schnittbox sind horizontal zentriert", () =>
            {
                LayoutErgebnis e = SeitenLayout.Berechne(32, 20, 30);
                Pruefer.IstNahe(105, e.Barcode.Schnittbox.X + e.Barcode.Schnittbox.Breite / 2);
                Pruefer.IstNahe(105, e.QrCode.Schnittbox.X + e.QrCode.Schnittbox.Breite / 2);
            });

            Pruefer.Test("Abstand zwischen Barcode- und QR-Schnittbox = 10 mm", () =>
            {
                LayoutErgebnis e = SeitenLayout.Berechne(32, 20, 30);
                double untenBarcode = e.Barcode.Schnittbox.Y + e.Barcode.Schnittbox.Hoehe;
                Pruefer.IstNahe(10, e.QrCode.Schnittbox.Y - untenBarcode);
            });

            Pruefer.Test("Schnittboxen überlappen sich nicht", () =>
            {
                LayoutErgebnis e = SeitenLayout.Berechne(32, 20, 30);
                double untenBarcode = e.Barcode.Schnittbox.Y + e.Barcode.Schnittbox.Hoehe;
                Pruefer.IstWahr(untenBarcode <= e.QrCode.Schnittbox.Y, "Schnittboxen überlappen sich");
            });

            Pruefer.Test("Schnittboxen haben mindestens 5 mm Abstand zum Blattrand", () =>
            {
                LayoutErgebnis e = SeitenLayout.Berechne(32, 20, 30);
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

            Pruefer.Test("Barcode-Höhe 20 mm umfasst Striche UND Schrift", () =>
            {
                CodeLayout b = SeitenLayout.Berechne(32, 20, 30).Barcode;
                Pruefer.IstNahe(20, b.Code.Hoehe + b.Klartext.Hoehe);
                Pruefer.IstNahe(14, b.Code.Hoehe);
            });

            Pruefer.Test("Barcode-Schnittbox: Höhe = 20 mm Code mit Schrift + 2 x 20 mm Rand", () =>
            {
                CodeLayout b = SeitenLayout.Berechne(32, 20, 30).Barcode;
                Pruefer.IstNahe(60, b.Schnittbox.Hoehe);
            });

            Pruefer.Test("Barcode-Höhe ohne Platz für Striche passt nicht (6 mm = nur Schrift)", () =>
            {
                LayoutErgebnis e = SeitenLayout.Berechne(32, 6, 30);
                Pruefer.IstWahr(!e.Passt, "sollte nicht passen");
                Pruefer.IstWahr(e.Fehler.Contains("Barcode-Höhe"), "Fehlermeldung soll die Barcode-Höhe nennen: " + e.Fehler);
            });

            Pruefer.Test("QR: Schrift sitzt 4 mm näher am Code (ragt 4 mm in den weißen Rand)", () =>
            {
                CodeLayout q = SeitenLayout.Berechne(32, 20, 30).QrCode;
                Pruefer.IstNahe(q.Code.Y + q.Code.Hoehe - 4, q.Klartext.Y);
            });

            Pruefer.Test("QR: Abstand Schrift zur unteren Schnittlinie bleibt 20 mm", () =>
            {
                CodeLayout q = SeitenLayout.Berechne(32, 20, 30).QrCode;
                Pruefer.IstNahe(20, (q.Schnittbox.Y + q.Schnittbox.Hoehe) - (q.Klartext.Y + q.Klartext.Hoehe));
            });
        }
    }
}
