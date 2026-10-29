using OfficeFileGuard.Whitelist;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OfficeFileGuard.App
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            // Inizializza i radio button dal valore corrente.
            ClassicOnlyRadioButton.Checked = !AppSettings.IsOwaEnabled;
            ClassicAndOwaRadioButton.Checked = AppSettings.IsOwaEnabled;
            // Inizializza i bottoni editwhitelist e deletewhitelist
            //editwhitelistButton.Click += editwhitelistButton_Click;  //già inizializzato in MainForm.Designer.cs
            //deletewhitelistButton.Click += deletewhitelistButton_Click;  //già inizializzato in MainForm.Designer.cs

            // alla modifica del valore checked aggiunge (ed esegue) l'evento OutlookMode_CheckedChanged
            ClassicAndOwaRadioButton.CheckedChanged += OutlookMode_CheckedChanged;
        }

        // evento da eseguire quando uno dei due radiobutton viene modificato
        private void OutlookMode_CheckedChanged(object sender, EventArgs e)
        {
            AppSettings.IsOwaEnabled = ClassicAndOwaRadioButton.Checked;
        }


        private void editWhitelistMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new EditWhitelistForm())
            {
                dlg.ShowDialog(this);
            }
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void tableFooterPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void panelFooter1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click_1(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://bluecrocodile.it");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://officefileguard.com");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://officefileguard.com/howtoinstall.html");
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.officefileguard.com/privacy.html");
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.officefileguard.com/#pricing");
        }

        private void HeaderPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void editwhitelistButton_Click(object sender, EventArgs e)
        {
            using (var form = new EditWhitelistForm())
            {
                form.ShowDialog(this);
            }
        }

        private void deletewhitelistButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete the entire whitelist?",
                "Delete Whitelist",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
                return;

            try
            {
                var store = new WhitelistStore();
                if (File.Exists(store.FilePath))
                {
                    File.SetAttributes(store.FilePath, FileAttributes.Normal);
                    File.Delete(store.FilePath);
                }
                MessageBox.Show(
                    "Whitelist deleted.",
                    "OfficeFileGuard",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to delete the whitelist.\n\n" + ex.Message,
                    "OfficeFileGuard",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
