using System;

namespace RfidLeser.Tests
{
    /// <summary>Tests für die Standardmaße.</summary>
    public static class EinstellungenTests
    {
        public static void Alle()
        {
            Console.WriteLine();
            Console.WriteLine("Einstellungen");

            Pruefer.Test("Standardmaße: Barcode 32 x 20 mm, QR-Code 30 mm", () =>
            {
                Einstellungen e = new Einstellungen();
                Pruefer.IstGleich(32m, e.BarcodeBreite);
                Pruefer.IstGleich(20m, e.BarcodeHoehe);
                Pruefer.IstGleich(30m, e.QrKante);
            });

            Pruefer.Test("Standardmaße passen auf das A4-Blatt", () =>
            {
                Einstellungen e = new Einstellungen();
                LayoutErgebnis l = SeitenLayout.Berechne((double)e.BarcodeBreite, (double)e.BarcodeHoehe, (double)e.QrKante);
                Pruefer.IstWahr(l.Passt, "passt nicht: " + l.Fehler);
            });
        }
    }
}
