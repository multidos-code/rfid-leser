using System;

namespace RfidLeser.Tests
{
    /// <summary>Tests für die automatische Verkleinerung der Klartext-Schrift.</summary>
    public static class SchriftgroesseTests
    {
        // Ersatz für die echte Textmessung: jedes Zeichen ist 0,6 x Schriftgröße breit
        private static double Messe(string text, double groesseMm)
        {
            return text.Length * 0.6 * groesseMm;
        }

        public static void Alle()
        {
            Console.WriteLine();
            Console.WriteLine("Schriftgroesse");

            Pruefer.Test("passt der Text, bleibt die Startgröße", () =>
            {
                // 11 Zeichen x 0,6 x 4,2 mm = 27,72 mm < 32 mm
                Pruefer.IstNahe(4.2, Schriftgroesse.Passend("0117ED48DB8", 32, 4.2, Messe));
            });

            Pruefer.Test("zu breiter Text wird verkleinert, bis er passt", () =>
            {
                double groesse = Schriftgroesse.Passend("0117ED48DB8", 20, 4.2, Messe);
                Pruefer.IstWahr(groesse < 4.2, "Schrift muss kleiner werden, ist " + groesse);
                Pruefer.IstWahr(Messe("0117ED48DB8", groesse) <= 20, "Text ist noch zu breit: " + Messe("0117ED48DB8", groesse));
            });

            Pruefer.Test("verkleinerte Schrift nutzt die Breite fast vollständig aus", () =>
            {
                double groesse = Schriftgroesse.Passend("0117ED48DB8", 20, 4.2, Messe);
                Pruefer.IstWahr(Messe("0117ED48DB8", groesse) >= 19, "Schrift unnötig klein: " + groesse);
            });

            Pruefer.Test("Schrift wird nie größer als die Startgröße", () =>
            {
                Pruefer.IstNahe(4.2, Schriftgroesse.Passend("AB", 200, 4.2, Messe));
            });

            Pruefer.Test("Schrift wird nie kleiner als 1 mm", () =>
            {
                Pruefer.IstNahe(1, Schriftgroesse.Passend("0117ED48DB8", 2, 4.2, Messe));
            });

            Pruefer.Test("leerer Text behält die Startgröße", () =>
            {
                Pruefer.IstNahe(4.2, Schriftgroesse.Passend("", 10, 4.2, Messe));
            });
        }
    }
}
