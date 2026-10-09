using System;
using System.Collections.Generic;

namespace RfidLeser
{
    /// <summary>
    /// Erzeugt die Balkenmuster für einen Code-128-Barcode (nur Code Set B, ASCII 32 bis 127).
    /// Jedes Zeichen ist ein Muster aus 6 Elementen (3 Striche, 3 Lücken), das Stopp-Zeichen
    /// hat 7 Elemente. Die Tabelle unten sind die Bitfolgen der 107 Code-128-Werte
    /// (1 = schwarz, 0 = weiß); Index = Code-Wert.
    /// </summary>
    public static class Code128
    {
        private const int StartB = 104;
        private const int Stopp = 106;
        private const int Pruefmodulo = 103;

        // Muster aller 107 Code-128-Werte (Index = Wert). Startzeichen B = 104, Stopp = 106.
        private static readonly string[] Tabelle =
        {
            "11011001100", "11001101100", "11001100110", "10010011000", "10010001100", "10001001100", "10011001000", "10011000100", // 0-7
            "10001100100", "11001001000", "11001000100", "11000100100", "10110011100", "10011011100", "10011001110", "10111001100", // 8-15
            "10011101100", "10011100110", "11001110010", "11001011100", "11001001110", "11011100100", "11001110100", "11101101110", // 16-23
            "11101001100", "11100101100", "11100100110", "11101100100", "11100110100", "11100110010", "11011011000", "11011000110", // 24-31
            "11000110110", "10100011000", "10001011000", "10001000110", "10110001000", "10001101000", "10001100010", "11010001000", // 32-39
            "11000101000", "11000100010", "10110111000", "10110001110", "10001101110", "10111011000", "10111000110", "10001110110", // 40-47
            "11101110110", "11010001110", "11000101110", "11011101000", "11011100010", "11011101110", "11101011000", "11101000110", // 48-55
            "11100010110", "11101101000", "11101100010", "11100011010", "11101111010", "11001000010", "11110001010", "10100110000", // 56-63
            "10100001100", "10010110000", "10010000110", "10000101100", "10000100110", "10110010000", "10110000100", "10011010000", // 64-71
            "10011000010", "10000110100", "10000110010", "11000010010", "11001010000", "11110111010", "11000010100", "10001111010", // 72-79
            "10100111100", "10010111100", "10010011110", "10111100100", "10011110100", "10011110010", "11110100100", "11110010100", // 80-87
            "11110010010", "11011011110", "11011110110", "11110110110", "10101111000", "10100011110", "10001011110", "10111101000", // 88-95
            "10111100010", "11110101000", "11110100010", "10111011110", "10111101110", "11101011110", "11110101110", "11010000100", // 96-103
            "11010010000", "11010011100", "1100011101011"                                                                         // 104-106
        };

        /// <summary>
        /// Liefert das Prüfzeichen (Wert 0 bis 102) für einen Text.
        /// Beispiel: "PJJ123C" ergibt 55.
        /// </summary>
        public static int Pruefwert(string text)
        {
            PruefeText(text);
            return Berechne(text);
        }

        /// <summary>
        /// Liefert die Modulbreiten (abwechselnd Strich, Lücke, beginnend mit Strich)
        /// inklusive Startzeichen, Prüfzeichen und Stopp-Zeichen.
        /// Wirft ArgumentException bei null oder bei Zeichen außerhalb von ASCII 32 bis 127.
        /// </summary>
        public static int[] Muster(string text)
        {
            PruefeText(text);

            List<int> werte = new List<int>();
            werte.Add(StartB);
            foreach (char c in text)
            {
                werte.Add(c - 32);
            }
            werte.Add(Berechne(text));
            werte.Add(Stopp);

            List<int> breiten = new List<int>();
            foreach (int wert in werte)
            {
                breiten.AddRange(Lauflaengen(Tabelle[wert]));
            }
            return breiten.ToArray();
        }

        /// <summary>Prüft Eingabe: darf nicht null sein und nur Zeichen 32 bis 127 enthalten.</summary>
        private static void PruefeText(string text)
        {
            if (text == null)
            {
                throw new ArgumentException("Der Barcode-Text darf nicht leer (null) sein.", "text");
            }
            foreach (char c in text)
            {
                if (c < 32 || c > 127)
                {
                    throw new ArgumentException("Das Zeichen '" + c + "' kann nicht als Code 128 gedruckt werden.", "text");
                }
            }
        }

        /// <summary>Gewichtete Summe nach Code-128-Regel, Modulo 103.</summary>
        private static int Berechne(string text)
        {
            int summe = StartB;
            for (int i = 0; i < text.Length; i++)
            {
                summe += (text[i] - 32) * (i + 1);
            }
            return summe % Pruefmodulo;
        }

        /// <summary>Wandelt eine Bitfolge (z. B. "11011001100") in Lauflängen um (z. B. 2,1,2,2,2,2).</summary>
        private static List<int> Lauflaengen(string bits)
        {
            List<int> laengen = new List<int>();
            int laenge = 1;
            for (int i = 1; i < bits.Length; i++)
            {
                if (bits[i] == bits[i - 1])
                {
                    laenge++;
                }
                else
                {
                    laengen.Add(laenge);
                    laenge = 1;
                }
            }
            laengen.Add(laenge);
            return laengen;
        }
    }
}
