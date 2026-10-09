using System;
using System.Collections.Generic;

namespace RfidLeser.Tests
{
    /// <summary>
    /// Kleine eigene Assert-Hilfe. Jeder Test ist eine Aktion; schlägt eine
    /// Prüfung fehl, wird eine PruefFehler-Ausnahme geworfen und als Fehler gezählt.
    /// </summary>
    public static class Pruefer
    {
        private static int bestanden = 0;
        private static int fehlgeschlagen = 0;

        public static int Bestanden { get { return bestanden; } }
        public static int Fehlgeschlagen { get { return fehlgeschlagen; } }

        /// <summary>Führt einen einzelnen Test aus und zählt Erfolg bzw. Fehlschlag.</summary>
        public static void Test(string name, Action aktion)
        {
            try
            {
                aktion();
                bestanden++;
                Console.WriteLine("  OK      " + name);
            }
            catch (Exception ex)
            {
                fehlgeschlagen++;
                Console.WriteLine("  FEHLER  " + name + " -> " + ex.Message);
            }
        }

        /// <summary>Prüft, ob zwei Werte gleich sind.</summary>
        public static void IstGleich(object erwartet, object tatsaechlich)
        {
            if (!Equals(erwartet, tatsaechlich))
            {
                throw new PruefFehler("erwartet <" + erwartet + ">, erhalten <" + tatsaechlich + ">");
            }
        }

        /// <summary>Prüft, ob zwei Kommazahlen bis auf 0,001 gleich sind (Rundungsfehler erlaubt).</summary>
        public static void IstNahe(double erwartet, double tatsaechlich)
        {
            if (Math.Abs(erwartet - tatsaechlich) > 0.001)
            {
                throw new PruefFehler("erwartet <" + erwartet + ">, erhalten <" + tatsaechlich + ">");
            }
        }

        /// <summary>Prüft, ob eine Bedingung wahr ist.</summary>
        public static void IstWahr(bool bedingung, string meldung)
        {
            if (!bedingung)
            {
                throw new PruefFehler(meldung);
            }
        }

        /// <summary>Prüft, ob ein Wert null ist.</summary>
        public static void IstNull(object wert)
        {
            if (wert != null)
            {
                throw new PruefFehler("erwartet null, erhalten <" + wert + ">");
            }
        }

        /// <summary>Prüft, ob ein Wert nicht null ist.</summary>
        public static void IstNichtNull(object wert)
        {
            if (wert == null)
            {
                throw new PruefFehler("erwartet einen Wert, erhalten null");
            }
        }

        /// <summary>Prüft, ob die Aktion eine Ausnahme des Typs T wirft.</summary>
        public static void Wirft<T>(Action aktion) where T : Exception
        {
            try
            {
                aktion();
            }
            catch (Exception ex)
            {
                if (ex is T)
                {
                    return;
                }
                throw new PruefFehler("erwartet " + typeof(T).Name + ", erhalten " + ex.GetType().Name);
            }
            throw new PruefFehler("erwartet " + typeof(T).Name + ", es wurde aber keine Ausnahme geworfen");
        }

        /// <summary>Prüft, ob zwei Zahlenfolgen (z. B. Arrays) elementweise gleich sind.</summary>
        public static void IstGleicheFolge<T>(IEnumerable<T> erwartet, IEnumerable<T> tatsaechlich)
        {
            List<T> e = new List<T>(erwartet);
            List<T> t = new List<T>(tatsaechlich);
            if (e.Count != t.Count)
            {
                throw new PruefFehler("Länge: erwartet " + e.Count + ", erhalten " + t.Count);
            }
            for (int i = 0; i < e.Count; i++)
            {
                if (!Equals(e[i], t[i]))
                {
                    throw new PruefFehler("Position " + i + ": erwartet <" + e[i] + ">, erhalten <" + t[i] + ">");
                }
            }
        }
    }

    /// <summary>Ausnahme, die eine fehlgeschlagene Prüfung kennzeichnet.</summary>
    public class PruefFehler : Exception
    {
        public PruefFehler(string meldung) : base(meldung) { }
    }
}
