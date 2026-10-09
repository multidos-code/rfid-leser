using System;
using System.Text;

namespace RfidLeser.Tests
{
    /// <summary>
    /// Einstiegspunkt des Testprogramms. Ruft alle Testgruppen auf.
    /// Rückgabewert 0 = alles grün, 1 = mindestens ein Test fehlgeschlagen.
    /// </summary>
    public static class Testlauf
    {
        public static int Main(string[] args)
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
            }
            catch (Exception)
            {
                // Konsole lässt die Kodierung nicht ändern – Umlaute ggf. verzerrt, Tests laufen trotzdem.
            }

            Console.WriteLine("RFID-Leser Tests");

            ChipParserTests.Alle();
            ReaderSucheTests.Alle();

            // Weitere Testgruppen hier eintragen (Bausteine werden nacheinander ergänzt).

            Console.WriteLine();
            Console.WriteLine("Ergebnis: " + Pruefer.Bestanden + " bestanden, " + Pruefer.Fehlgeschlagen + " fehlgeschlagen");
            return Pruefer.Fehlgeschlagen == 0 ? 0 : 1;
        }
    }
}
