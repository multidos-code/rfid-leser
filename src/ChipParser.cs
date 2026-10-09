using System;

namespace RfidLeser
{
    /// <summary>
    /// Liest aus dem Datenpuffer des RFID-Readers die erste vollständige, gültige Chip-Nummer.
    /// (Baustein 1 – vorläufiger Stub, damit die Tests rot laufen.)
    /// </summary>
    public static class ChipParser
    {
        /// <summary>Liefert die erste gültige Nummer im Puffer oder null.</summary>
        public static string ErsteNummer(string puffer)
        {
            if (string.IsNullOrEmpty(puffer))
            {
                return null;
            }

            string[] zeilen = puffer.Split('\n');

            // Der letzte Abschnitt hinter dem letzten Zeilenende ist noch unvollständig
            // und wird deshalb nicht betrachtet.
            int vollstaendig = zeilen.Length - 1;
            for (int i = 0; i < vollstaendig; i++)
            {
                string kandidat = zeilen[i].Trim().ToUpperInvariant();
                if (IstGueltig(kandidat))
                {
                    return kandidat;
                }
            }

            return null;
        }

        /// <summary>Prüft, ob der Text 8 bis 16 Hex-Zeichen (0-9, A-F) lang ist.</summary>
        private static bool IstGueltig(string text)
        {
            if (text.Length < 8 || text.Length > 16)
            {
                return false;
            }

            foreach (char c in text)
            {
                bool ziffer = c >= '0' && c <= '9';
                bool buchstabe = c >= 'A' && c <= 'F';
                if (!ziffer && !buchstabe)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
