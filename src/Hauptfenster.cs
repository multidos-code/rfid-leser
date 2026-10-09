using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace RfidLeser
{
    /// <summary>Hauptfenster: zeigt die gelesene Chip-Nummer und druckt Barcode + QR-Code.</summary>
    public class Hauptfenster : Form
    {
        private const int LeseTimeoutSekunden = 30;

        private readonly TextBox txtNummer;
        private readonly Label lblStatus;
        private readonly Label lblLayoutFehler;
        private readonly Button btnNeu;
        private readonly Button btnDrucken;
        private readonly NumericUpDown nudBarcodeBreite;
        private readonly NumericUpDown nudBarcodeHoehe;
        private readonly NumericUpDown nudQrKante;

        private string aktuelleNummer;   // null, solange keine Nummer gelesen wurde
        private bool lesend;             // true, während im Hintergrund gelesen wird
        private BackgroundWorker leser;

        public Hauptfenster()
        {
            Text = "RFID-Leser";
            ClientSize = new Size(620, 400);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9F);

            Label ueberschrift = new Label();
            ueberschrift.Text = "Chip-Nummer";
            ueberschrift.Location = new Point(20, 15);
            ueberschrift.AutoSize = true;
            ueberschrift.Font = new Font("Segoe UI", 11F, FontStyle.Bold);

            txtNummer = new TextBox();
            txtNummer.Location = new Point(20, 40);
            txtNummer.Size = new Size(580, 70);
            txtNummer.Font = new Font("Consolas", 28F, FontStyle.Bold);
            txtNummer.TextAlign = HorizontalAlignment.Center;
            txtNummer.ReadOnly = true;
            txtNummer.BackColor = Color.White;
            txtNummer.TabStop = false;

            lblStatus = new Label();
            lblStatus.Location = new Point(20, 118);
            lblStatus.Size = new Size(580, 24);
            lblStatus.Font = new Font("Segoe UI", 11F);

            btnNeu = new Button();
            btnNeu.Text = "Neu auslesen";
            btnNeu.Location = new Point(20, 155);
            btnNeu.Size = new Size(280, 48);
            btnNeu.Font = new Font("Segoe UI", 11F);
            btnNeu.Click += delegate { StarteLesen(); };

            btnDrucken = new Button();
            btnDrucken.Text = "Drucken";
            btnDrucken.Location = new Point(320, 155);
            btnDrucken.Size = new Size(280, 48);
            btnDrucken.Font = new Font("Segoe UI", 11F);
            btnDrucken.Enabled = false;
            btnDrucken.Click += delegate { Drucken(); };

            GroupBox gruppe = new GroupBox();
            gruppe.Text = "Größen in mm";
            gruppe.Location = new Point(20, 220);
            gruppe.Size = new Size(580, 130);

            Label lblBreite = BaueBeschriftung("Barcode-Breite:", 15, 30);
            nudBarcodeBreite = BaueMassFeld(170, 27);
            Label lblHoehe = BaueBeschriftung("Barcode-Höhe:", 15, 60);
            nudBarcodeHoehe = BaueMassFeld(170, 57);
            Label lblQr = BaueBeschriftung("QR-Kantenlänge:", 15, 90);
            nudQrKante = BaueMassFeld(170, 87);

            Label hinweis = new Label();
            hinweis.Text = "Diese Maße gelten für den Code selbst, ohne Rand. " +
                "Vor dem Drucken wird geprüft, ob alles auf A4 passt.";
            hinweis.Location = new Point(300, 28);
            hinweis.Size = new Size(265, 90);

            gruppe.Controls.Add(lblBreite);
            gruppe.Controls.Add(nudBarcodeBreite);
            gruppe.Controls.Add(lblHoehe);
            gruppe.Controls.Add(nudBarcodeHoehe);
            gruppe.Controls.Add(lblQr);
            gruppe.Controls.Add(nudQrKante);
            gruppe.Controls.Add(hinweis);

            lblLayoutFehler = new Label();
            lblLayoutFehler.Location = new Point(20, 362);
            lblLayoutFehler.Size = new Size(580, 30);
            lblLayoutFehler.ForeColor = Color.DarkRed;
            lblLayoutFehler.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            Controls.Add(ueberschrift);
            Controls.Add(txtNummer);
            Controls.Add(lblStatus);
            Controls.Add(btnNeu);
            Controls.Add(btnDrucken);
            Controls.Add(gruppe);
            Controls.Add(lblLayoutFehler);

            // Gespeicherte Größen laden, erst danach Änderungen speichern
            Einstellungen einstellungen = Einstellungen.Laden();
            nudBarcodeBreite.Value = (decimal)einstellungen.BarcodeBreite;
            nudBarcodeHoehe.Value = (decimal)einstellungen.BarcodeHoehe;
            nudQrKante.Value = (decimal)einstellungen.QrKante;

            nudBarcodeBreite.ValueChanged += delegate { GroesseGeaendert(); };
            nudBarcodeHoehe.ValueChanged += delegate { GroesseGeaendert(); };
            nudQrKante.ValueChanged += delegate { GroesseGeaendert(); };

            Shown += delegate { StarteLesen(); };

            AktualisiereDruckStatus();
        }

        /// <summary>Erzeugt ein Beschriftungsfeld an fester Stelle.</summary>
        private static Label BaueBeschriftung(string text, int x, int y)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Location = new Point(x, y + 3);
            lbl.AutoSize = true;
            return lbl;
        }

        /// <summary>Erzeugt ein Zahlenfeld für Millimeter.</summary>
        private static NumericUpDown BaueMassFeld(int x, int y)
        {
            NumericUpDown nud = new NumericUpDown();
            nud.Location = new Point(x, y);
            nud.Size = new Size(100, 25);
            nud.Minimum = Einstellungen.MinMm;
            nud.Maximum = Einstellungen.MaxMm;
            nud.DecimalPlaces = 0;
            nud.Increment = 1;
            return nud;
        }

        /// <summary>Speichert die Größen und prüft das Layout neu.</summary>
        private void GroesseGeaendert()
        {
            Einstellungen einstellungen = new Einstellungen();
            einstellungen.BarcodeBreite = nudBarcodeBreite.Value;
            einstellungen.BarcodeHoehe = nudBarcodeHoehe.Value;
            einstellungen.QrKante = nudQrKante.Value;
            try
            {
                einstellungen.Speichern();
            }
            catch (Exception)
            {
                // Speichern nicht möglich (z. B. Rechte): Programm läuft trotzdem weiter.
            }
            AktualisiereDruckStatus();
        }

        /// <summary>Berechnet das Blattlayout aus den aktuellen Größen.</summary>
        private LayoutErgebnis AktuellesLayout()
        {
            return SeitenLayout.Berechne(
                (double)nudBarcodeBreite.Value,
                (double)nudBarcodeHoehe.Value,
                (double)nudQrKante.Value);
        }

        /// <summary>Schaltet den Druckknopf frei oder sperrt ihn und zeigt Layoutfehler an.</summary>
        private void AktualisiereDruckStatus()
        {
            LayoutErgebnis layout = AktuellesLayout();
            lblLayoutFehler.Text = layout.Passt ? string.Empty : layout.Fehler;
            btnDrucken.Enabled = !lesend && aktuelleNummer != null && layout.Passt;
        }

        /// <summary>Startet das Lesen im Hintergrund (Oberfläche bleibt bedienbar).</summary>
        private void StarteLesen()
        {
            if (leser != null && leser.IsBusy)
            {
                return;
            }

            aktuelleNummer = null;
            txtNummer.Text = string.Empty;
            lesend = true;
            btnNeu.Enabled = false;
            SetzeStatus("Bitte Chip auflegen…", Color.Black);
            AktualisiereDruckStatus();

            leser = new BackgroundWorker();
            leser.DoWork += LeseImHintergrund;
            leser.RunWorkerCompleted += LeseFertig;
            leser.RunWorkerAsync();
        }

        private static void LeseImHintergrund(object sender, DoWorkEventArgs e)
        {
            e.Result = ChipLeser.LeseChip(LeseTimeoutSekunden);
        }

        /// <summary>Verarbeitet das Leseergebnis auf der Oberfläche.</summary>
        private void LeseFertig(object sender, RunWorkerCompletedEventArgs e)
        {
            lesend = false;
            btnNeu.Enabled = true;

            if (e.Error != null)
            {
                SetzeStatus("Unerwarteter Fehler beim Lesen: " + e.Error.Message, Color.DarkRed);
            }
            else
            {
                LeseErgebnis ergebnis = (LeseErgebnis)e.Result;
                if (ergebnis.Status == LeseStatus.Gelesen)
                {
                    aktuelleNummer = ergebnis.Nummer;
                    txtNummer.Text = ergebnis.Nummer;
                    SetzeStatus("Gelesen. Jetzt kann gedruckt werden.", Color.DarkGreen);
                }
                else
                {
                    SetzeStatus(ergebnis.Meldung, Color.DarkRed);
                }
            }

            AktualisiereDruckStatus();
        }

        private void SetzeStatus(string text, Color farbe)
        {
            lblStatus.Text = text;
            lblStatus.ForeColor = farbe;
        }

        /// <summary>Öffnet den Druckdialog und druckt die Seite mit A4 und ohne Ränder.</summary>
        private void Drucken()
        {
            if (aktuelleNummer == null)
            {
                return;
            }

            LayoutErgebnis layout = AktuellesLayout();
            if (!layout.Passt)
            {
                MessageBox.Show(this, layout.Fehler, "Drucken nicht möglich", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string nummer = aktuelleNummer;
            string papierProblem = null;

            PrintDocument dokument = new PrintDocument();
            dokument.DefaultPageSettings.PaperSize = A4Papier(dokument);
            dokument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            dokument.DefaultPageSettings.Landscape = false;
            dokument.DefaultPageSettings.Color = false;
            dokument.PrintPage += delegate(object sender, PrintPageEventArgs ea)
            {
                if (!IstA4(ea.PageSettings.PaperSize))
                {
                    papierProblem = "Das Papierformat muss A4 sein. Bitte im Druckdialog A4 wählen.";
                    ea.Cancel = true;
                    return;
                }

                Graphics g = ea.Graphics;
                g.PageUnit = GraphicsUnit.Millimeter;

                // Der Drucker hat einen nicht bedruckbaren Randbereich (HardMargin, in 1/100 Zoll).
                // Der Nullpunkt wird um diesen Bereich verschoben, damit 0/0 die Blattecke ist.
                double hardX = ea.PageSettings.HardMarginX / 100.0 * 25.4;
                double hardY = ea.PageSettings.HardMarginY / 100.0 * 25.4;
                g.TranslateTransform((float)-hardX, (float)-hardY);

                Druck.ZeichneSeite(g, layout, nummer);
                ea.HasMorePages = false;
            };

            PrintDialog dialog = new PrintDialog();
            dialog.Document = dokument;
            dialog.AllowSomePages = false;

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                dokument.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Drucken fehlgeschlagen: " + ex.Message, "Fehler",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (papierProblem != null)
            {
                MessageBox.Show(this, papierProblem, "Drucken abgebrochen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>Sucht das Papierformat A4 des Druckers, sonst A4 mit Standardmaßen (8,27 x 11,69 Zoll).</summary>
        private static PaperSize A4Papier(PrintDocument dokument)
        {
            foreach (PaperSize papier in dokument.PrinterSettings.PaperSizes)
            {
                if (IstA4(papier))
                {
                    return papier;
                }
            }
            return new PaperSize("A4", 827, 1169);
        }

        /// <summary>true, wenn das Papier A4 ist (Kennung oder Maße 210 x 297 mm).</summary>
        private static bool IstA4(PaperSize papier)
        {
            if (papier.Kind == PaperKind.A4)
            {
                return true;
            }
            return Math.Abs(papier.Width - 827) <= 2 && Math.Abs(papier.Height - 1169) <= 2;
        }
    }
}
