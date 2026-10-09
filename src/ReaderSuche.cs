using System;
using System.Collections.Generic;

namespace RfidLeser
{
    /// <summary>Ein serieller Anschluss mit seiner Geräte-ID (PnP-ID).</summary>
    public class PortInfo
    {
        public PortInfo(string name, string geraeteId)
        {
            Name = name;
            GeraeteId = geraeteId;
        }

        /// <summary>Name des Anschlusses, z. B. "COM3".</summary>
        public string Name { get; private set; }

        /// <summary>Geräte-ID, z. B. "USB\VID_0403&amp;PID_6001\...".</summary>
        public string GeraeteId { get; private set; }
    }

    /// <summary>Findet unter den seriellen Anschlüssen den Port des RFID-Readers (FTDI-Chip).</summary>
    public static class ReaderSuche
    {
        // Herstellerkennung (VID) und Produktkennung (PID) des FTDI-USB-Seriell-Wandlers.
        private const string FtdiVid = "VID_0403";
        private const string FtdiPid = "PID_6001";

        /// <summary>Liefert den Namen des FTDI-Ports (z. B. "COM3") oder null, wenn keiner da ist.</summary>
        public static string FindePort(IEnumerable<PortInfo> ports)
        {
            if (ports == null)
            {
                return null;
            }

            foreach (PortInfo port in ports)
            {
                if (port == null || port.GeraeteId == null)
                {
                    continue;
                }

                string id = port.GeraeteId.ToUpperInvariant();
                if (id.Contains(FtdiVid) && id.Contains(FtdiPid))
                {
                    return port.Name;
                }
            }

            return null;
        }

        /// <summary>
        /// Liest alle echten seriellen Anschlüsse von Windows (über WMI, Klasse Win32_PnPEntity).
        /// Nur Einträge mit "(COMx)" im Namen zählen. Bei WMI-Fehler wird eine leere Liste geliefert.
        /// Hinweis: Dieser Teil wird nicht durch Tests geprüft, sondern manuell am Gerät.
        /// </summary>
        public static List<PortInfo> AktuellePorts()
        {
            List<PortInfo> ergebnis = new List<PortInfo>();
            try
            {
                using (System.Management.ManagementObjectSearcher suche =
                    new System.Management.ManagementObjectSearcher(
                        "SELECT Name, PNPDeviceID FROM Win32_PnPEntity WHERE Name LIKE '%(COM%'"))
                {
                    foreach (System.Management.ManagementBaseObject eintrag in suche.Get())
                    {
                        string name = eintrag["Name"] as string;
                        string geraeteId = eintrag["PNPDeviceID"] as string;
                        if (name == null)
                        {
                            continue;
                        }

                        System.Text.RegularExpressions.Match treffer =
                            System.Text.RegularExpressions.Regex.Match(name, @"\((COM\d+)\)");
                        if (treffer.Success)
                        {
                            ergebnis.Add(new PortInfo(treffer.Groups[1].Value, geraeteId));
                        }
                    }
                }
            }
            catch (Exception)
            {
                // WMI nicht verfügbar: keine Ports gefunden.
            }
            return ergebnis;
        }
    }
}
