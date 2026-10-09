using System;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace RfidLeser
{
    /// <summary>Ausgang eines Leseversuchs.</summary>
    public enum LeseStatus
    {
        Gelesen,
        KeinReader,
        PortBelegt,
        Zeitueberschreitung,
        Fehler
    }

    /// <summary>Ergebnis eines Leseversuchs mit verständlicher Meldung.</summary>
    public class LeseErgebnis
    {
        public LeseErgebnis(LeseStatus status, string nummer, string meldung)
        {
            Status = status;
            Nummer = nummer;
            Meldung = meldung;
        }

        public LeseStatus Status { get; private set; }

        /// <summary>Die gelesene Chip-Nummer, nur bei Status Gelesen.</summary>
        public string Nummer { get; private set; }

        /// <summary>Meldung für die Statuszeile.</summary>
        public string Meldung { get; private set; }
    }

    /// <summary>
    /// Liest einmal eine Chip-Nummer vom RFID-Reader (FTDI, 9600 Baud, 8N1).
    /// Der Port wird am Ende immer wieder geschlossen.
    /// </summary>
    public static class ChipLeser
    {
        private const int Baudrate = 9600;
        private const int MaxPufferZeichen = 2000;

        /// <summary>Sucht den Reader, liest bis eine gültige Nummer kommt oder die Zeit abläuft.</summary>
        public static LeseErgebnis LeseChip(int timeoutSekunden)
        {
            string portName = ReaderSuche.FindePort(ReaderSuche.AktuellePorts());
            if (portName == null)
            {
                return new LeseErgebnis(LeseStatus.KeinReader, null,
                    "Kein Reader gefunden. Bitte das USB-Kabel des RFID-Lesers prüfen.");
            }

            SerialPort port = null;
            try
            {
                port = new SerialPort(portName, Baudrate, Parity.None, 8, StopBits.One);
                port.Handshake = Handshake.None;
                port.Open();
            }
            catch (UnauthorizedAccessException)
            {
                return new LeseErgebnis(LeseStatus.PortBelegt, null,
                    "Der Reader an " + portName + " ist belegt. Bitte andere Programme schließen, die darauf zugreifen.");
            }
            catch (Exception ex)
            {
                return new LeseErgebnis(LeseStatus.Fehler, null,
                    "Der Reader an " + portName + " konnte nicht geöffnet werden: " + ex.Message);
            }

            try
            {
                return LeseAusPort(port, timeoutSekunden);
            }
            catch (Exception ex)
            {
                return new LeseErgebnis(LeseStatus.Fehler, null,
                    "Die Verbindung zum Reader wurde unterbrochen: " + ex.Message);
            }
            finally
            {
                try
                {
                    if (port.IsOpen)
                    {
                        port.Close();
                    }
                }
                catch (Exception)
                {
                    // Schließen fehlgeschlagen: Port wird beim Beenden des Programms freigegeben.
                }
                port.Dispose();
            }
        }

        /// <summary>Sammelt Daten, bis ChipParser eine gültige Nummer liefert oder die Zeit abgelaufen ist.</summary>
        private static LeseErgebnis LeseAusPort(SerialPort port, int timeoutSekunden)
        {
            StringBuilder puffer = new StringBuilder();

            // Das Programm kann mitten in einer Übertragung einsteigen. Deshalb werden die Daten
            // erst ab dem ersten Zeilenende ausgewertet, damit keine angeschnittene Zeile entsteht.
            bool anfangGefunden = false;

            DateTime ende = DateTime.UtcNow.AddSeconds(timeoutSekunden);
            while (DateTime.UtcNow < ende)
            {
                if (port.BytesToRead <= 0)
                {
                    Thread.Sleep(50);
                    continue;
                }

                string neu = port.ReadExisting();
                if (!anfangGefunden)
                {
                    int zeilenende = neu.IndexOf('\n');
                    if (zeilenende < 0)
                    {
                        continue;
                    }
                    neu = neu.Substring(zeilenende + 1);
                    anfangGefunden = true;
                }

                puffer.Append(neu);
                if (puffer.Length > MaxPufferZeichen)
                {
                    puffer.Remove(0, puffer.Length - MaxPufferZeichen);
                }

                string nummer = ChipParser.ErsteNummer(puffer.ToString());
                if (nummer != null)
                {
                    return new LeseErgebnis(LeseStatus.Gelesen, nummer, "Gelesen.");
                }
            }

            return new LeseErgebnis(LeseStatus.Zeitueberschreitung, null,
                "Zeitüberschreitung: In " + timeoutSekunden + " Sekunden wurde kein Chip erkannt. " +
                "Bitte den Chip auflegen und erneut auslesen.");
        }
    }
}
