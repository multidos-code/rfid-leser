using System;
using System.Collections.Generic;

namespace RfidLeser.Tests
{
    /// <summary>Tests für HilfeText.Zerlege (Baustein 8, Hilfe in der exe).</summary>
    public static class HilfeTextTests
    {
        public static void Alle()
        {
            Console.WriteLine("HilfeText:");

            Pruefer.Test("H1, H2 und H3 werden als Überschrift erkannt", () =>
            {
                List<HilfeZeile> zeilen = HilfeText.Zerlege("# Titel\n## Unter\n### Klein");
                Pruefer.IstGleich(3, zeilen.Count);
                Pruefer.IstGleich("Titel", zeilen[0].Text);
                Pruefer.IstWahr(zeilen[0].IstUeberschrift, "H1 sollte Überschrift sein");
                Pruefer.IstGleich("Unter", zeilen[1].Text);
                Pruefer.IstWahr(zeilen[1].IstUeberschrift, "H2 sollte Überschrift sein");
                Pruefer.IstGleich("Klein", zeilen[2].Text);
                Pruefer.IstWahr(zeilen[2].IstUeberschrift, "H3 sollte Überschrift sein");
            });

            Pruefer.Test("normale Zeile ist keine Überschrift", () =>
            {
                List<HilfeZeile> zeilen = HilfeText.Zerlege("Normale Zeile.");
                Pruefer.IstGleich("Normale Zeile.", zeilen[0].Text);
                Pruefer.IstWahr(!zeilen[0].IstUeberschrift, "normale Zeile sollte keine Überschrift sein");
            });

            Pruefer.Test("** wird aus dem Text entfernt", () =>
            {
                List<HilfeZeile> zeilen = HilfeText.Zerlege("Das **wichtig** hier");
                Pruefer.IstGleich("Das wichtig hier", zeilen[0].Text);
            });

            Pruefer.Test("Backticks werden aus dem Text entfernt", () =>
            {
                List<HilfeZeile> zeilen = HilfeText.Zerlege("Nutze `build.ps1` jetzt");
                Pruefer.IstGleich("Nutze build.ps1 jetzt", zeilen[0].Text);
            });

            Pruefer.Test("--- wird zur Leerzeile", () =>
            {
                List<HilfeZeile> zeilen = HilfeText.Zerlege("Vorher\n---\nNachher");
                Pruefer.IstGleich(3, zeilen.Count);
                Pruefer.IstGleich("", zeilen[1].Text);
                Pruefer.IstWahr(!zeilen[1].IstUeberschrift, "--- sollte keine Überschrift sein");
            });

            Pruefer.Test("Leerzeilen bleiben erhalten", () =>
            {
                List<HilfeZeile> zeilen = HilfeText.Zerlege("a\n\nb");
                Pruefer.IstGleich(3, zeilen.Count);
                Pruefer.IstGleich("", zeilen[1].Text);
            });

            Pruefer.Test("Listenpunkte mit - und 1. bleiben unverändert", () =>
            {
                List<HilfeZeile> zeilen = HilfeText.Zerlege("- Eintrag\n1. Schritt");
                Pruefer.IstGleich("- Eintrag", zeilen[0].Text);
                Pruefer.IstWahr(!zeilen[0].IstUeberschrift, "Listenpunkt sollte keine Überschrift sein");
                Pruefer.IstGleich("1. Schritt", zeilen[1].Text);
                Pruefer.IstWahr(!zeilen[1].IstUeberschrift, "nummerierter Punkt sollte keine Überschrift sein");
            });

            Pruefer.Test("LadeAusRessource liefert den Hilfetext aus der eingebetteten README.md", () =>
                Pruefer.IstWahr(HilfeText.LadeAusRessource().Contains("Reader und andere Programme"),
                    "in der Ressource fehlt der erwartete Text (README.md nicht eingebettet?)"));

            Pruefer.Test("leere Eingabe ergibt leere Liste", () =>
                Pruefer.IstGleich(0, HilfeText.Zerlege("").Count));

            Pruefer.Test("null ergibt leere Liste", () =>
                Pruefer.IstGleich(0, HilfeText.Zerlege(null).Count));
        }
    }
}
