using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RfidLeser
{
    /// <summary>
    /// Versionsnummer: nächste Version aus vorhandenen Versionsmarken (Git-Tags) berechnen
    /// und den Fenstertitel bilden. Wird auch vom Release-Build auf GitHub benutzt.
    /// </summary>
    public static class Versionsnummer
    {
        private const string Programmname = "RFID-Leser";
        private const string ErsteVersion = "1.0.0";

        // Versionsmarken wie "v1.1" oder "v1.1.2" (dritte Stelle optional)
        private static readonly Regex MarkenMuster = new Regex(@"^v(\d+)\.(\d+)(?:\.(\d+))?$");
        private static readonly Regex VersionMuster = new Regex(@"^\d+\.\d+\.\d+$");

        /// <summary>
        /// Liefert die nächste Version: höchste vorhandene Versionsmarke, letzte Stelle + 1
        /// (z. B. v1.1 → 1.1.1, v1.1.2 → 1.1.3). Ohne Versionsmarken: 1.0.0.
        /// Andere Markierungen (z. B. "test", "v2.0-beta") werden ignoriert.
        /// </summary>
        public static string Naechste(IEnumerable<string> tags)
        {
            int[] hoechste = null;
            if (tags != null)
            {
                foreach (string tag in tags)
                {
                    int[] teile = Zerlege(tag);
                    if (teile != null && (hoechste == null || IstGroesser(teile, hoechste)))
                    {
                        hoechste = teile;
                    }
                }
            }

            if (hoechste == null)
            {
                return ErsteVersion;
            }
            return hoechste[0] + "." + hoechste[1] + "." + (hoechste[2] + 1);
        }

        /// <summary>Fenstertitel mit Version, z. B. "RFID-Leser v1.1.1". Ohne Version: Entwicklerversion.</summary>
        public static string Fenstertitel(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                return Programmname + " (Entwicklerversion)";
            }
            return Programmname + " v" + version;
        }

        /// <summary>true, wenn die Version die Form Zahl.Zahl.Zahl hat (z. B. "1.1.1").</summary>
        public static bool IstGueltig(string version)
        {
            return version != null && VersionMuster.IsMatch(version);
        }

        /// <summary>
        /// Liest die beim Bauen eingetragene Version aus der laufenden exe.
        /// Lokale Builds ohne Version liefern null.
        /// </summary>
        public static string AusProgramm()
        {
            Assembly programm = Assembly.GetEntryAssembly();
            if (programm == null)
            {
                return null;
            }
            object[] attribute = programm.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
            if (attribute.Length == 0)
            {
                return null;
            }
            return ((AssemblyInformationalVersionAttribute)attribute[0]).InformationalVersion;
        }

        /// <summary>Zerlegt "v1.2" bzw. "v1.2.3" in Zahlen; fehlende dritte Stelle = 0. Ungültig → null.</summary>
        private static int[] Zerlege(string tag)
        {
            if (tag == null)
            {
                return null;
            }
            Match treffer = MarkenMuster.Match(tag.Trim());
            if (!treffer.Success)
            {
                return null;
            }
            int dritte = treffer.Groups[3].Success ? int.Parse(treffer.Groups[3].Value) : 0;
            return new int[] { int.Parse(treffer.Groups[1].Value), int.Parse(treffer.Groups[2].Value), dritte };
        }

        /// <summary>Vergleicht zwei Versionen Stelle für Stelle als Zahlen.</summary>
        private static bool IstGroesser(int[] a, int[] b)
        {
            for (int i = 0; i < 3; i++)
            {
                if (a[i] != b[i])
                {
                    return a[i] > b[i];
                }
            }
            return false;
        }
    }
}
