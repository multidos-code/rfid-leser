using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace RfidLeser
{
    /// <summary>
    /// Die drei Größen (mm), die beim Programmstart geladen und bei Änderung gespeichert werden.
    /// Datei: %APPDATA%\RFID-Leser\einstellungen.txt (Zeilen im Format Schlüssel=Wert).
    /// </summary>
    public class Einstellungen
    {
        /// <summary>Kleinster und größter erlaubter Wert in mm.</summary>
        public const decimal MinMm = 1;
        public const decimal MaxMm = 210;

        // Bis v1.1.1 hieß der Schlüssel "BarcodeHoehe" und meinte die Höhe OHNE Schrift.
        // Seit v1.2.1 gilt die Höhe inklusive Schrift; alte Werte werden deshalb nicht übernommen.
        private const string SchluesselBarcodeHoehe = "BarcodeHoeheMitSchrift";

        public Einstellungen()
        {
            BarcodeBreite = 32;
            BarcodeHoehe = 20;
            QrKante = 30;
        }

        /// <summary>Breite des Barcodes ohne Rand (Standard 32 mm).</summary>
        public decimal BarcodeBreite { get; set; }

        /// <summary>Höhe des Barcodes inklusive Schrift darunter (Standard 20 mm).</summary>
        public decimal BarcodeHoehe { get; set; }

        /// <summary>Kantenlänge des QR-Codes (Standard 30 mm).</summary>
        public decimal QrKante { get; set; }

        /// <summary>Vollständiger Pfad der Einstellungsdatei.</summary>
        public static string Datei
        {
            get
            {
                string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return Path.Combine(appdata, "RFID-Leser", "einstellungen.txt");
            }
        }

        /// <summary>Lädt die Einstellungen. Fehlt die Datei oder ist ein Wert ungültig, gilt der Standard.</summary>
        public static Einstellungen Laden()
        {
            Einstellungen ergebnis = new Einstellungen();
            try
            {
                string pfad = Datei;
                if (!File.Exists(pfad))
                {
                    return ergebnis;
                }

                foreach (string zeile in File.ReadAllLines(pfad, Encoding.UTF8))
                {
                    int gleich = zeile.IndexOf('=');
                    if (gleich <= 0)
                    {
                        continue;
                    }

                    string schluessel = zeile.Substring(0, gleich).Trim();
                    string wert = zeile.Substring(gleich + 1).Trim();

                    decimal zahl;
                    if (!decimal.TryParse(wert, NumberStyles.Number, CultureInfo.InvariantCulture, out zahl))
                    {
                        continue;
                    }
                    if (zahl < MinMm || zahl > MaxMm)
                    {
                        continue;
                    }

                    if (schluessel == "BarcodeBreite")
                    {
                        ergebnis.BarcodeBreite = zahl;
                    }
                    else if (schluessel == SchluesselBarcodeHoehe)
                    {
                        ergebnis.BarcodeHoehe = zahl;
                    }
                    else if (schluessel == "QrKante")
                    {
                        ergebnis.QrKante = zahl;
                    }
                }
            }
            catch (Exception)
            {
                // Datei defekt oder nicht lesbar: Standardwerte verwenden.
            }
            return ergebnis;
        }

        /// <summary>Speichert die Einstellungen. Legt den Ordner bei Bedarf an.</summary>
        public void Speichern()
        {
            string pfad = Datei;
            Directory.CreateDirectory(Path.GetDirectoryName(pfad));

            StringBuilder text = new StringBuilder();
            text.AppendLine("BarcodeBreite=" + BarcodeBreite.ToString(CultureInfo.InvariantCulture));
            text.AppendLine(SchluesselBarcodeHoehe + "=" + BarcodeHoehe.ToString(CultureInfo.InvariantCulture));
            text.AppendLine("QrKante=" + QrKante.ToString(CultureInfo.InvariantCulture));
            File.WriteAllText(pfad, text.ToString(), Encoding.UTF8);
        }
    }
}
