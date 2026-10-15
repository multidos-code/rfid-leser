using System;
using System.Drawing;
using System.Windows.Forms;

namespace RfidLeser
{
    /// <summary>
    /// Hilfefenster: zeigt die Anwender-Hilfe (eingebettete README.md) nur zum Lesen.
    /// Nicht durch Unit-Tests abgedeckt, wird manuell geprüft.
    /// </summary>
    public class Hilfefenster : Form
    {
        private readonly RichTextBox textfeld;
        private readonly Button btnSchliessen;

        public Hilfefenster()
        {
            Text = "RFID-Leser – Hilfe";
            // Programmsymbol der exe auch in Titelleiste und Taskleiste zeigen
            try
            {
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch (Exception)
            {
                // Symbol nicht lesbar: Windows-Standardsymbol bleibt
            }
            ClientSize = new Size(700, 600);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            MinimumSize = new Size(400, 300);
            Font = new Font("Segoe UI", 10F);

            textfeld = new RichTextBox();
            textfeld.Location = new Point(12, 12);
            textfeld.Size = new Size(ClientSize.Width - 24, ClientSize.Height - 64);
            textfeld.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textfeld.ReadOnly = true;
            textfeld.BackColor = Color.White;
            textfeld.BorderStyle = BorderStyle.FixedSingle;
            textfeld.ScrollBars = RichTextBoxScrollBars.Vertical;
            textfeld.Font = new Font("Segoe UI", 10F);
            textfeld.TabStop = false;

            btnSchliessen = new Button();
            btnSchliessen.Text = "Schließen";
            btnSchliessen.Size = new Size(140, 36);
            btnSchliessen.Location = new Point(ClientSize.Width - 12 - 140, ClientSize.Height - 48);
            btnSchliessen.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSchliessen.Font = new Font("Segoe UI", 11F);
            btnSchliessen.Click += delegate { Close(); };

            Controls.Add(textfeld);
            Controls.Add(btnSchliessen);
            CancelButton = btnSchliessen;

            FuelleText(HilfeText.Zerlege(HilfeText.LadeAusRessource()));
        }

        /// <summary>Schreibt die Zeilen ins Textfeld; Überschriften fett und größer.</summary>
        private void FuelleText(System.Collections.Generic.List<HilfeZeile> zeilen)
        {
            Font normal = new Font("Segoe UI", 10F);
            Font ueberschrift = new Font("Segoe UI", 12F, FontStyle.Bold);

            textfeld.SuspendLayout();
            foreach (HilfeZeile zeile in zeilen)
            {
                textfeld.SelectionStart = textfeld.TextLength;
                textfeld.SelectionLength = 0;
                textfeld.SelectionFont = zeile.IstUeberschrift ? ueberschrift : normal;
                textfeld.SelectedText = zeile.Text + "\n";
            }
            textfeld.SelectionStart = 0;
            textfeld.SelectionLength = 0;
            textfeld.ResumeLayout();
        }
    }
}
