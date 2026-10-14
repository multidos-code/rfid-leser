using System;

namespace RfidLeser
{
    /// <summary>Wählt die Schriftgröße so, dass der Klartext in die verfügbare Breite passt.</summary>
    public static class Schriftgroesse
    {
        private const double KleinsteGroesseMm = 1;

        /// <summary>
        /// Liefert die Startgröße, wenn der Text damit in maxBreiteMm passt. Sonst wird die Schrift
        /// so weit verkleinert, dass der Text gerade hineinpasst (nie kleiner als 1 mm).
        /// messe(text, größeMm) liefert die Breite des Texts in mm bei dieser Schriftgröße.
        /// </summary>
        public static double Passend(string text, double maxBreiteMm, double startGroesseMm, Func<string, double, double> messe)
        {
            if (string.IsNullOrEmpty(text))
            {
                return startGroesseMm;
            }

            double breite = messe(text, startGroesseMm);
            if (breite <= maxBreiteMm)
            {
                return startGroesseMm;
            }

            // Textbreite wächst mit der Schriftgröße: im Verhältnis verkleinern,
            // danach in kleinen Schritten nachregeln, falls die Messung nicht genau linear ist.
            double groesse = startGroesseMm * maxBreiteMm / breite;
            for (int i = 0; i < 50 && groesse > KleinsteGroesseMm && messe(text, groesse) > maxBreiteMm; i++)
            {
                groesse *= 0.98;
            }
            return Math.Max(groesse, KleinsteGroesseMm);
        }
    }
}
