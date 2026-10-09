using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace RfidLeser
{
    /// <summary>Eine Zeile des Hilfetextes.</summary>
    public class HilfeZeile
    {
        public string Text;
        public bool IstUeberschrift;

        public HilfeZeile(string text, bool istUeberschrift)
        {
            Text = text;
            IstUeberschrift = istUeberschrift;
        }
    }

    /// <summary>Wandelt den Hilfetext (Markdown) in einfache Zeilen für das Hilfefenster um.</summary>
    public static class HilfeText
    {
        /// <summary>Text, der angezeigt wird, wenn die eingebettete Hilfedatei fehlt.</summary>
        private const string ErsatzText =
            "Die Hilfe konnte nicht geladen werden, weil die Hilfedatei in diesem Programm fehlt.";

        /// <summary>
        /// Zerlegt Markdown in Zeilen. Überschriften (#, ##, ###) werden markiert,
        /// Rauten, ** und Backticks entfernt, "---" wird zur Leerzeile.
        /// </summary>
        public static List<HilfeZeile> Zerlege(string markdown)
        {
            List<HilfeZeile> ergebnis = new List<HilfeZeile>();
            if (string.IsNullOrEmpty(markdown))
            {
                return ergebnis;
            }

            string[] zeilen = markdown.Split('\n');
            // Ein abschließender Zeilenumbruch erzeugt keine zusätzliche Leerzeile.
            int anzahl = markdown.EndsWith("\n") ? zeilen.Length - 1 : zeilen.Length;

            for (int i = 0; i < anzahl; i++)
            {
                string zeile = zeilen[i].TrimEnd('\r');

                if (zeile.Trim() == "---")
                {
                    ergebnis.Add(new HilfeZeile("", false));
                    continue;
                }

                bool ueberschrift = false;
                if (zeile.StartsWith("### "))
                {
                    zeile = zeile.Substring(4);
                    ueberschrift = true;
                }
                else if (zeile.StartsWith("## "))
                {
                    zeile = zeile.Substring(3);
                    ueberschrift = true;
                }
                else if (zeile.StartsWith("# "))
                {
                    zeile = zeile.Substring(2);
                    ueberschrift = true;
                }

                zeile = zeile.Replace("**", "").Replace("`", "");
                ergebnis.Add(new HilfeZeile(zeile, ueberschrift));
            }

            return ergebnis;
        }

        /// <summary>
        /// Liest die eingebettete Ressource README.md (UTF-8). Fehlt sie, kommt der Ersatztext.
        /// </summary>
        public static string LadeAusRessource()
        {
            try
            {
                Assembly ausfuehrend = Assembly.GetExecutingAssembly();
                using (Stream strom = ausfuehrend.GetManifestResourceStream("README.md"))
                {
                    if (strom == null)
                    {
                        return ErsatzText;
                    }

                    using (StreamReader leser = new StreamReader(strom, Encoding.UTF8))
                    {
                        return leser.ReadToEnd();
                    }
                }
            }
            catch (Exception)
            {
                return ErsatzText;
            }
        }
    }
}
