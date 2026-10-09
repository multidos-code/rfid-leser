using System;
using System.Collections.Generic;

namespace RfidLeser.Tests
{
    /// <summary>Tests für die Berechnung der nächsten Versionsnummer und den Fenstertitel.</summary>
    public static class VersionsnummerTests
    {
        public static void Alle()
        {
            Console.WriteLine();
            Console.WriteLine("Versionsnummer");

            Pruefer.Test("v1.1 wird zu 1.1.1", delegate
            {
                Pruefer.IstGleich("1.1.1", Versionsnummer.Naechste(new[] { "v1.0", "v1.1" }));
            });

            Pruefer.Test("dreistellige Version: letzte Stelle +1", delegate
            {
                Pruefer.IstGleich("1.1.3", Versionsnummer.Naechste(new[] { "v1.1", "v1.1.1", "v1.1.2" }));
            });

            Pruefer.Test("höchste Version zählt, nicht die Reihenfolge", delegate
            {
                Pruefer.IstGleich("2.0.1", Versionsnummer.Naechste(new[] { "v2.0", "v1.9.9", "v1.1" }));
            });

            Pruefer.Test("Zahlen werden als Zahlen verglichen (1.10 > 1.9)", delegate
            {
                Pruefer.IstGleich("1.10.1", Versionsnummer.Naechste(new[] { "v1.9", "v1.10" }));
            });

            Pruefer.Test("bewusst gesetztes v1.2 setzt Zählung dort fort", delegate
            {
                Pruefer.IstGleich("1.2.1", Versionsnummer.Naechste(new[] { "v1.1.5", "v1.2" }));
            });

            Pruefer.Test("fremde Markierungen werden ignoriert", delegate
            {
                Pruefer.IstGleich("1.1.1", Versionsnummer.Naechste(new[] { "test", "v1.1", "v2.0-beta", "1.5" }));
            });

            Pruefer.Test("ohne Versionsmarken beginnt die Zählung bei 1.0.0", delegate
            {
                Pruefer.IstGleich("1.0.0", Versionsnummer.Naechste(new string[0]));
            });

            Pruefer.Test("null wird wie keine Versionsmarken behandelt", delegate
            {
                Pruefer.IstGleich("1.0.0", Versionsnummer.Naechste(null));
            });

            Pruefer.Test("Fenstertitel mit Version", delegate
            {
                Pruefer.IstGleich("RFID-Leser v1.1.1", Versionsnummer.Fenstertitel("1.1.1"));
            });

            Pruefer.Test("Fenstertitel ohne Version (lokaler Build)", delegate
            {
                Pruefer.IstGleich("RFID-Leser (Entwicklerversion)", Versionsnummer.Fenstertitel(null));
                Pruefer.IstGleich("RFID-Leser (Entwicklerversion)", Versionsnummer.Fenstertitel(""));
            });

            Pruefer.Test("Version gültig: nur Zahl.Zahl.Zahl", delegate
            {
                Pruefer.IstWahr(Versionsnummer.IstGueltig("1.1.1"), "1.1.1 muss gültig sein");
                Pruefer.IstWahr(!Versionsnummer.IstGueltig("v1.1.1"), "v1.1.1 darf nicht gültig sein");
                Pruefer.IstWahr(!Versionsnummer.IstGueltig("1.1"), "1.1 darf nicht gültig sein");
                Pruefer.IstWahr(!Versionsnummer.IstGueltig("1.1.x"), "1.1.x darf nicht gültig sein");
                Pruefer.IstWahr(!Versionsnummer.IstGueltig(null), "null darf nicht gültig sein");
            });
        }
    }
}
