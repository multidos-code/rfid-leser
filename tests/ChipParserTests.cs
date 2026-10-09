using System;

namespace RfidLeser.Tests
{
    /// <summary>Tests für ChipParser.ErsteNummer (Baustein 1).</summary>
    public static class ChipParserTests
    {
        public static void Alle()
        {
            Console.WriteLine("ChipParser:");

            Pruefer.Test("normale Zeile mit CR LF wird erkannt", () =>
                Pruefer.IstGleich("0117ED48DB8", ChipParser.ErsteNummer("0117ED48DB8\r\n")));

            Pruefer.Test("nur LF als Zeilenende reicht", () =>
                Pruefer.IstGleich("0117ED48DB8", ChipParser.ErsteNummer("0117ED48DB8\n")));

            Pruefer.Test("bei mehreren Zeilen wird die erste genommen", () =>
                Pruefer.IstGleich("0117ED48DB8", ChipParser.ErsteNummer("0117ED48DB8\r\n0117ED48DB9\r\n")));

            Pruefer.Test("angeschnittene erste Zeile wird übersprungen", () =>
                Pruefer.IstGleich("0117ED48DB8", ChipParser.ErsteNummer("ED48DB8\r\n0117ED48DB8\r\n")));

            Pruefer.Test("fehlendes Zeilenende ergibt null", () =>
                Pruefer.IstNull(ChipParser.ErsteNummer("0117ED48DB8")));

            Pruefer.Test("nur CR ohne LF ergibt null", () =>
                Pruefer.IstNull(ChipParser.ErsteNummer("0117ED48DB8\r")));

            Pruefer.Test("Müllzeichen in der Zeile ergeben null", () =>
                Pruefer.IstNull(ChipParser.ErsteNummer("0117ZZ48DB8\r\n")));

            Pruefer.Test("Müllzeile wird übersprungen, gültige danach genommen", () =>
                Pruefer.IstGleich("0117ED48DB8", ChipParser.ErsteNummer("@#!?\r\n0117ED48DB8\r\n")));

            Pruefer.Test("leerer Puffer ergibt null", () =>
                Pruefer.IstNull(ChipParser.ErsteNummer("")));

            Pruefer.Test("Puffer null ergibt null", () =>
                Pruefer.IstNull(ChipParser.ErsteNummer(null)));

            Pruefer.Test("Kleinbuchstaben werden in Großbuchstaben umgewandelt", () =>
                Pruefer.IstGleich("0117ED48DB8", ChipParser.ErsteNummer("0117ed48db8\r\n")));

            Pruefer.Test("Leerzeichen am Rand werden entfernt", () =>
                Pruefer.IstGleich("0117ED48DB8", ChipParser.ErsteNummer("  0117ED48DB8 \r\n")));

            Pruefer.Test("7 Zeichen (zu kurz) ergeben null", () =>
                Pruefer.IstNull(ChipParser.ErsteNummer("0117ED4\r\n")));

            Pruefer.Test("8 Zeichen (kleinste gültige Länge) werden akzeptiert", () =>
                Pruefer.IstGleich("0117ED48", ChipParser.ErsteNummer("0117ED48\r\n")));

            Pruefer.Test("16 Zeichen (größte gültige Länge) werden akzeptiert", () =>
                Pruefer.IstGleich("0117ED48DB800FF1", ChipParser.ErsteNummer("0117ED48DB800FF1\r\n")));

            Pruefer.Test("17 Zeichen (zu lang) ergeben null", () =>
                Pruefer.IstNull(ChipParser.ErsteNummer("0117ED48DB800FF12\r\n")));
        }
    }
}
