using System;
using System.Collections.Generic;

namespace RfidLeser.Tests
{
    /// <summary>Tests für Versionsdatei, Testversion/Freigabe, Fenstertitel und Changelog-Auszug.</summary>
    public static class VersionsnummerTests
    {
        private const string BeispielChangelog =
            "# Änderungsprotokoll\r\n" +
            "\r\n" +
            "## 1.2.0\r\n" +
            "\r\n" +
            "- Neue Funktion A.\r\n" +
            "- Neue Funktion B.\r\n" +
            "\r\n" +
            "## 1.1.1\r\n" +
            "\r\n" +
            "- Fehler behoben.\r\n" +
            "\r\n" +
            "## 1.1\r\n" +
            "\r\n" +
            "- Hilfe ergänzt.\r\n";

        public static void Alle()
        {
            Console.WriteLine();
            Console.WriteLine("Versionsnummer");

            Pruefer.Test("Version gültig: Zahl.Zahl.Zahl", delegate
            {
                Pruefer.IstWahr(Versionsnummer.IstGueltig("1.1.1"), "1.1.1 muss gültig sein");
                Pruefer.IstWahr(Versionsnummer.IstGueltig("10.20.30"), "10.20.30 muss gültig sein");
            });

            Pruefer.Test("Version gültig: Zahl.Zahl.Zahl-SNAPSHOT", delegate
            {
                Pruefer.IstWahr(Versionsnummer.IstGueltig("1.2.0-SNAPSHOT"), "1.2.0-SNAPSHOT muss gültig sein");
            });

            Pruefer.Test("Version ungültig: andere Formen", delegate
            {
                Pruefer.IstWahr(!Versionsnummer.IstGueltig("v1.1.1"), "v1.1.1 darf nicht gültig sein");
                Pruefer.IstWahr(!Versionsnummer.IstGueltig("1.1"), "1.1 darf nicht gültig sein");
                Pruefer.IstWahr(!Versionsnummer.IstGueltig("1.1.x"), "1.1.x darf nicht gültig sein");
                Pruefer.IstWahr(!Versionsnummer.IstGueltig("1.2.0-snapshot"), "Kleinschreibung darf nicht gültig sein");
                Pruefer.IstWahr(!Versionsnummer.IstGueltig("1.2.0-BETA"), "1.2.0-BETA darf nicht gültig sein");
                Pruefer.IstWahr(!Versionsnummer.IstGueltig(""), "leer darf nicht gültig sein");
                Pruefer.IstWahr(!Versionsnummer.IstGueltig(null), "null darf nicht gültig sein");
            });

            Pruefer.Test("Testversion wird erkannt", delegate
            {
                Pruefer.IstWahr(Versionsnummer.IstTestversion("1.2.0-SNAPSHOT"), "1.2.0-SNAPSHOT ist Testversion");
                Pruefer.IstWahr(!Versionsnummer.IstTestversion("1.2.0"), "1.2.0 ist keine Testversion");
            });

            Pruefer.Test("Zahlenteil für Dateieigenschaften", delegate
            {
                Pruefer.IstGleich("1.2.0", Versionsnummer.Zahlenteil("1.2.0-SNAPSHOT"));
                Pruefer.IstGleich("1.2.0", Versionsnummer.Zahlenteil("1.2.0"));
            });

            Pruefer.Test("Versionsdatei: Leerzeichen und Zeilenende werden entfernt", delegate
            {
                Pruefer.IstGleich("1.2.0-SNAPSHOT", Versionsnummer.AusVersionsdatei("﻿ 1.2.0-SNAPSHOT \r\n"));
            });

            Pruefer.Test("Versionsdatei: ungültiger Inhalt wird abgelehnt", delegate
            {
                Pruefer.Wirft<ArgumentException>(delegate { Versionsnummer.AusVersionsdatei("1.2"); });
                Pruefer.Wirft<ArgumentException>(delegate { Versionsnummer.AusVersionsdatei(""); });
                Pruefer.Wirft<ArgumentException>(delegate { Versionsnummer.AusVersionsdatei(null); });
            });

            Pruefer.Test("Fenstertitel freigegebene Version", delegate
            {
                Pruefer.IstGleich("RFID-Leser v1.2.0", Versionsnummer.Fenstertitel("1.2.0"));
            });

            Pruefer.Test("Fenstertitel Testversion", delegate
            {
                Pruefer.IstGleich("RFID-Leser v1.2.0-SNAPSHOT (Testversion)", Versionsnummer.Fenstertitel("1.2.0-SNAPSHOT"));
            });

            Pruefer.Test("Fenstertitel ohne Version (lokaler Build)", delegate
            {
                Pruefer.IstGleich("RFID-Leser (Entwicklerversion)", Versionsnummer.Fenstertitel(null));
                Pruefer.IstGleich("RFID-Leser (Entwicklerversion)", Versionsnummer.Fenstertitel(""));
            });

            Pruefer.Test("Changelog: Abschnitt der Version wird herausgelöst", delegate
            {
                Pruefer.IstGleich("- Neue Funktion A.\r\n- Neue Funktion B.",
                    Versionsnummer.ChangelogAbschnitt(BeispielChangelog, "1.2.0"));
            });

            Pruefer.Test("Changelog: letzter Abschnitt bis Dateiende", delegate
            {
                Pruefer.IstGleich("- Hilfe ergänzt.", Versionsnummer.ChangelogAbschnitt(BeispielChangelog, "1.1"));
            });

            Pruefer.Test("Changelog: 1.1 trifft nicht 1.1.1 (exakte Überschrift)", delegate
            {
                Pruefer.IstGleich("- Fehler behoben.", Versionsnummer.ChangelogAbschnitt(BeispielChangelog, "1.1.1"));
            });

            Pruefer.Test("Changelog: Überschrift mit Zusatz (z. B. Datum) wird erkannt", delegate
            {
                Pruefer.IstGleich("- Text.", Versionsnummer.ChangelogAbschnitt("## 2.0.0 (09.10.2026)\n\n- Text.\n", "2.0.0"));
            });

            Pruefer.Test("Changelog: fehlender oder leerer Abschnitt ergibt null", delegate
            {
                Pruefer.IstNull(Versionsnummer.ChangelogAbschnitt(BeispielChangelog, "9.9.9"));
                Pruefer.IstNull(Versionsnummer.ChangelogAbschnitt("## 3.0.0\n\n## 2.0.0\n- x\n", "3.0.0"));
                Pruefer.IstNull(Versionsnummer.ChangelogAbschnitt(null, "1.2.0"));
            });
        }
    }
}
