using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace RfidLeser
{
    /// <summary>Programmstart. Bindet die eingebettete QRCoder.dll ein.</summary>
    public static class Programm
    {
        [STAThread]
        public static void Main()
        {
            // MUSS die erste Anweisung sein: Lädt QRCoder.dll aus der eingebetteten Ressource,
            // bevor irgendeine Klasse aus QRCoder benutzt wird.
            AppDomain.CurrentDomain.AssemblyResolve += LadeEingebettetesQrCoder;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Hauptfenster());
        }

        /// <summary>Liefert die QRCoder-Bibliothek aus der Ressource der exe, wenn sie angefordert wird.</summary>
        private static Assembly LadeEingebettetesQrCoder(object sender, ResolveEventArgs args)
        {
            AssemblyName name = new AssemblyName(args.Name);
            if (name.Name != "QRCoder")
            {
                return null;
            }

            using (Stream strom = Assembly.GetExecutingAssembly().GetManifestResourceStream("QRCoder.dll"))
            {
                if (strom == null)
                {
                    return null;
                }

                using (MemoryStream speicher = new MemoryStream())
                {
                    strom.CopyTo(speicher);
                    return Assembly.Load(speicher.ToArray());
                }
            }
        }
    }
}
