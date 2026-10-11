using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RfidLeser
{
    /// <summary>
    /// Versionsnummer aus der Datei version.txt: prüfen, Testversion (-SNAPSHOT) erkennen,
    /// Fenstertitel bilden und den passenden Abschnitt aus CHANGELOG.md herauslösen.
    /// Wird auch vom Release-Build auf GitHub benutzt.
    /// </summary>
    public static class Versionsnummer
    {
        private const string Programmname = "RFID-Leser";
        private const string TestKennung = "-SNAPSHOT";

        // Erlaubt: 1.2.0 (freigegeben) oder 1.2.0-SNAPSHOT (Testversion)
        private static readonly Regex VersionMuster = new Regex(@"^(\d+\.\d+\.\d+)(-SNAPSHOT)?$");

        /// <summary>true, wenn die Version die Form Zahl.Zahl.Zahl oder Zahl.Zahl.Zahl-SNAPSHOT hat.</summary>
        public static bool IstGueltig(string version)
        {
            return version != null && VersionMuster.IsMatch(version);
        }

        /// <summary>true bei einer Testversion (endet auf -SNAPSHOT) – dafür gibt es kein Release.</summary>
        public static bool IstTestversion(string version)
        {
            return version != null && version.EndsWith(TestKennung, StringComparison.Ordinal);
        }

        /// <summary>Nur die Zahlen, z. B. "1.2.0" aus "1.2.0-SNAPSHOT" (für die Dateieigenschaften der exe).</summary>
        public static string Zahlenteil(string version)
        {
            Match treffer = VersionMuster.Match(version ?? "");
            return treffer.Success ? treffer.Groups[1].Value : null;
        }

        /// <summary>
        /// Liest die Version aus dem Inhalt von version.txt (BOM, Leerzeichen, Zeilenende werden entfernt).
        /// Wirft ArgumentException, wenn der Inhalt keine gültige Version ist.
        /// </summary>
        public static string AusVersionsdatei(string inhalt)
        {
            string version = (inhalt ?? "").Trim().TrimStart('﻿').Trim();
            if (!IstGueltig(version))
            {
                throw new ArgumentException(
                    "version.txt enthält keine gültige Version: '" + version + "'. Erwartet z. B. 1.2.0 oder 1.2.0-SNAPSHOT.");
            }
            return version;
        }

        /// <summary>
        /// Fenstertitel, z. B. "RFID-Leser v1.2.0" oder "RFID-Leser v1.2.0-SNAPSHOT (Testversion)".
        /// Ohne Version (lokaler Build): "RFID-Leser (Entwicklerversion)".
        /// </summary>
        public static string Fenstertitel(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                return Programmname + " (Entwicklerversion)";
            }
            if (IstTestversion(version))
            {
                return Programmname + " v" + version + " (Testversion)";
            }
            return Programmname + " v" + version;
        }

        /// <summary>
        /// Liefert den Text unter der Überschrift "## {version}" bis zur nächsten "## "-Überschrift,
        /// ohne Leerzeilen am Anfang und Ende. Hinter der Versionsnummer darf ein Zusatz stehen
        /// (z. B. ein Datum). Fehlt der Abschnitt oder ist er leer: null.
        /// </summary>
        public static string ChangelogAbschnitt(string changelog, string version)
        {
            if (changelog == null || string.IsNullOrEmpty(version))
            {
                return null;
            }

            Regex ueberschrift = new Regex("^## " + Regex.Escape(version) + "( |$)");
            List<string> abschnitt = new List<string>();
            bool drin = false;

            foreach (string roh in changelog.Split('\n'))
            {
                string zeile = roh.TrimEnd('\r');
                if (zeile.StartsWith("## "))
                {
                    if (drin)
                    {
                        break;
                    }
                    drin = ueberschrift.IsMatch(zeile);
                    continue;
                }
                if (drin)
                {
                    abschnitt.Add(zeile);
                }
            }

            // Leerzeilen am Anfang und Ende entfernen
            while (abschnitt.Count > 0 && abschnitt[0].Trim() == "")
            {
                abschnitt.RemoveAt(0);
            }
            while (abschnitt.Count > 0 && abschnitt[abschnitt.Count - 1].Trim() == "")
            {
                abschnitt.RemoveAt(abschnitt.Count - 1);
            }

            return abschnitt.Count == 0 ? null : string.Join("\r\n", abschnitt);
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
    }
}
