using System;
using System.Collections.Generic;

namespace RfidLeser.Tests
{
    /// <summary>Tests für ReaderSuche.FindePort (Baustein 2).</summary>
    public static class ReaderSucheTests
    {
        public static void Alle()
        {
            Console.WriteLine("ReaderSuche:");

            Pruefer.Test("FTDI-Reader wird gefunden", () =>
            {
                List<PortInfo> ports = new List<PortInfo>
                {
                    new PortInfo("COM1", "ACPI\\PNP0501\\1"),
                    new PortInfo("COM3", "FTDIBUS\\VID_0403+PID_6001+A9ABCDEF\\0000")
                };
                Pruefer.IstGleich("COM3", ReaderSuche.FindePort(ports));
            });

            Pruefer.Test("USB-Schreibweise mit & wird erkannt", () =>
            {
                List<PortInfo> ports = new List<PortInfo>
                {
                    new PortInfo("COM3", "USB\\VID_0403&PID_6001\\A9ABCDEF")
                };
                Pruefer.IstGleich("COM3", ReaderSuche.FindePort(ports));
            });

            Pruefer.Test("Kleinschreibung in der Geräte-ID stört nicht", () =>
            {
                List<PortInfo> ports = new List<PortInfo>
                {
                    new PortInfo("COM7", "usb\\vid_0403&pid_6001\\a9abcdef")
                };
                Pruefer.IstGleich("COM7", ReaderSuche.FindePort(ports));
            });

            Pruefer.Test("bei mehreren Ports wird der FTDI-Port gewählt", () =>
            {
                List<PortInfo> ports = new List<PortInfo>
                {
                    new PortInfo("COM1", "BTHENUM\\{00001101}\\7&1"),
                    new PortInfo("COM4", "USB\\VID_0403&PID_6001\\A9ABCDEF"),
                    new PortInfo("COM5", "USB\\VID_1A86&PID_7523\\5")
                };
                Pruefer.IstGleich("COM4", ReaderSuche.FindePort(ports));
            });

            Pruefer.Test("kein passender Port ergibt null", () =>
            {
                List<PortInfo> ports = new List<PortInfo>
                {
                    new PortInfo("COM1", "ACPI\\PNP0501\\1"),
                    new PortInfo("COM2", "USB\\VID_0403&PID_6015\\B")
                };
                Pruefer.IstNull(ReaderSuche.FindePort(ports));
            });

            Pruefer.Test("leere Liste ergibt null", () =>
                Pruefer.IstNull(ReaderSuche.FindePort(new List<PortInfo>())));

            Pruefer.Test("Liste null ergibt null", () =>
                Pruefer.IstNull(ReaderSuche.FindePort(null)));
        }
    }
}
