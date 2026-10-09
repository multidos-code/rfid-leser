using System;

namespace RfidLeser.Tests
{
    /// <summary>Tests für QrErzeuger.Matrix (Baustein 4).</summary>
    public static class QrErzeugerTests
    {
        public static void Alle()
        {
            Console.WriteLine("QrErzeuger:");

            Pruefer.Test("Matrix ist quadratisch", () =>
            {
                bool[,] m = QrErzeuger.Matrix("0117ED48DB8");
                Pruefer.IstNichtNull(m);
                Pruefer.IstGleich(m.GetLength(0), m.GetLength(1));
            });

            Pruefer.Test("Größe ist 21 für 0117ED48DB8 (Version 1)", () =>
            {
                bool[,] m = QrErzeuger.Matrix("0117ED48DB8");
                Pruefer.IstGleich(21, m.GetLength(0));
            });

            Pruefer.Test("Positionsmuster oben links vorhanden", () =>
                PruefeRahmen(QrErzeuger.Matrix("0117ED48DB8"), 0, 0));

            Pruefer.Test("Positionsmuster oben rechts vorhanden", () =>
                PruefeRahmen(QrErzeuger.Matrix("0117ED48DB8"), 0, 14));

            Pruefer.Test("Positionsmuster unten links vorhanden", () =>
                PruefeRahmen(QrErzeuger.Matrix("0117ED48DB8"), 14, 0));

            Pruefer.Test("leerer Text wird abgelehnt", () =>
                Pruefer.Wirft<ArgumentException>(() => QrErzeuger.Matrix("")));

            Pruefer.Test("null wird abgelehnt", () =>
                Pruefer.Wirft<ArgumentException>(() => QrErzeuger.Matrix(null)));
        }

        /// <summary>
        /// Prüft ein 7x7-Positionsmuster, das bei (zeile0, spalte0) beginnt:
        /// äußerer Rahmen schwarz, zweiter Rahmen weiß, 3x3-Mitte schwarz.
        /// </summary>
        private static void PruefeRahmen(bool[,] m, int zeile0, int spalte0)
        {
            for (int dz = 0; dz < 7; dz++)
            {
                for (int ds = 0; ds < 7; ds++)
                {
                    bool erwartet;
                    if (dz == 0 || dz == 6 || ds == 0 || ds == 6)
                    {
                        erwartet = true;
                    }
                    else if (dz == 1 || dz == 5 || ds == 1 || ds == 5)
                    {
                        erwartet = false;
                    }
                    else
                    {
                        erwartet = true;
                    }

                    bool tatsaechlich = m[zeile0 + dz, spalte0 + ds];
                    if (tatsaechlich != erwartet)
                    {
                        throw new PruefFehler("Positionsmuster bei Zeile " + (zeile0 + dz) +
                            ", Spalte " + (spalte0 + ds) + " stimmt nicht");
                    }
                }
            }
        }
    }
}
