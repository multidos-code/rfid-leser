using System;

namespace RfidLeser
{
    /// <summary>
    /// Erzeugt die Module (schwarz/weiß) eines QR-Codes über die Bibliothek QRCoder.
    /// (Baustein 4 – vorläufiger Stub, damit die Tests rot laufen.)
    /// </summary>
    public static class QrErzeuger
    {
        // Ruhezone, die QRCoder in seiner Matrix mitliefert (in Modulen, je Seite).
        private const int QrRuhezoneQrCoder = 4;

        /// <summary>
        /// Liefert die Module als bool[Zeile, Spalte] (true = schwarz), ECC-Stufe M,
        /// OHNE Ruhezone. Wirft ArgumentException bei leerem Text.
        /// </summary>
        public static bool[,] Matrix(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Der QR-Code-Text darf nicht leer sein.", "text");
            }

            using (QRCoder.QRCodeGenerator erzeuger = new QRCoder.QRCodeGenerator())
            using (var daten = erzeuger.CreateQrCode(text, QRCoder.QRCodeGenerator.ECCLevel.M))
            {
                // QRCoder liefert die Matrix MIT eigener Ruhezone (4 Module rundherum).
                // Die wird hier abgeschnitten, weil das Layout die Ruhezone selbst festlegt.
                var zeilen = daten.ModuleMatrix;
                int rand = QrRuhezoneQrCoder;
                int groesse = zeilen.Count - 2 * rand;
                bool[,] ergebnis = new bool[groesse, groesse];
                for (int z = 0; z < groesse; z++)
                {
                    for (int s = 0; s < groesse; s++)
                    {
                        ergebnis[z, s] = zeilen[z + rand][s + rand];
                    }
                }
                return ergebnis;
            }
        }
    }
}
