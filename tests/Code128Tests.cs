using System;
using System.Linq;

namespace RfidLeser.Tests
{
    /// <summary>Tests für Code128.Muster und Code128.Pruefwert (Baustein 3).</summary>
    public static class Code128Tests
    {
        public static void Alle()
        {
            Console.WriteLine("Code128:");

            Pruefer.Test("Prüfzeichen für PJJ123C ist 55", () =>
                Pruefer.IstGleich(55, Code128.Pruefwert("PJJ123C")));

            Pruefer.Test("Startzeichen B hat die Breiten 2,1,1,2,1,4", () =>
            {
                int[] muster = Code128.Muster("A");
                Pruefer.IstGleicheFolge(new int[] { 2, 1, 1, 2, 1, 4 }, muster.Take(6));
            });

            Pruefer.Test("Stopp-Zeichen hat die Breiten 2,3,3,1,1,1,2", () =>
            {
                int[] muster = Code128.Muster("A");
                Pruefer.IstGleicheFolge(new int[] { 2, 3, 3, 1, 1, 1, 2 }, muster.Skip(muster.Length - 7));
            });

            Pruefer.Test("Prüfzeichen-Muster für PJJ123C entspricht Wert 55 (Breiten 3,1,1,3,2,1)", () =>
            {
                int[] muster = Code128.Muster("PJJ123C");
                // Start (6) + 7 Zeichen zu je 6 Breiten = 48 Werte, danach das Prüfzeichen
                Pruefer.IstGleicheFolge(new int[] { 3, 1, 1, 3, 2, 1 }, muster.Skip(48).Take(6));
            });

            Pruefer.Test("Gesamtmodulzahl = 11*(n+2)+13 für PJJ123C (n=7) ergibt 112", () =>
            {
                int[] muster = Code128.Muster("PJJ123C");
                Pruefer.IstGleich(112, muster.Sum());
            });

            Pruefer.Test("Gesamtmodulzahl stimmt auch für leeren Text (35)", () =>
            {
                int[] muster = Code128.Muster("");
                Pruefer.IstGleich(35, muster.Sum());
            });

            Pruefer.Test("jedes druckbare Zeichen 32 bis 127 ergibt 46 Module (1 Zeichen)", () =>
            {
                for (int c = 32; c <= 127; c++)
                {
                    int[] muster = Code128.Muster(((char)c).ToString());
                    Pruefer.IstGleich(46, muster.Sum());
                    Pruefer.IstGleich(25, muster.Length);
                }
            });

            Pruefer.Test("Breiten sind alle größer als 0", () =>
            {
                int[] muster = Code128.Muster("0117ED48DB8");
                Pruefer.IstWahr(muster.All(b => b > 0), "es gibt eine Breite <= 0");
            });

            Pruefer.Test("Zeichen unter 32 werden abgelehnt", () =>
                Pruefer.Wirft<ArgumentException>(() => Code128.Muster("\u0001")));

            Pruefer.Test("Umlaut wird abgelehnt", () =>
                Pruefer.Wirft<ArgumentException>(() => Code128.Muster("Ä")));

            Pruefer.Test("Zeichen über 127 wird abgelehnt", () =>
                Pruefer.Wirft<ArgumentException>(() => Code128.Muster("\u0080")));

            Pruefer.Test("null wird abgelehnt", () =>
                Pruefer.Wirft<ArgumentException>(() => Code128.Muster(null)));
        }
    }
}
