using OfficeFileGuard.Whitelist;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace OfficeFileGuard.App
{
    public partial class EditWhitelistForm : Form
    {
        private readonly WhitelistStore _store = new WhitelistStore();
        private string _originalText = string.Empty;    // salvo qui il testo originale per capire se Utente ha modificato il testo

        public EditWhitelistForm()
        {
            InitializeComponent();

            whitelistTextBox.WordWrap = false;
            whitelistTextBox.AcceptsReturn = true;
            whitelistTextBox.ScrollBars = ScrollBars.Both;
            whitelistTextBox.Font = new Font("Consolas", 10F);

            saveButton.Click += saveButton_Click;
            cancelButton.Click += cancelButton_Click;
        }


        // al caricamento della finestra...
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                // all'apertura salvo il testo in _originalText
                _originalText = _store.LoadText()
                    .Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Cannot read the whitelist file:\n" + ex.Message,
                    "OfficeFileGuard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            whitelistTextBox.Text = _originalText;
            whitelistTextBox.Select(0, 0);
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            var result = WhitelistParser.Parse(whitelistTextBox.Text);

            if (!result.IsValid)
            {
                string msg = string.Join(Environment.NewLine,
                    result.Errors.Take(10).Select(err =>
                        "Line " + err.Line + ": " + err.Text + "  (" + err.Reason + ")"));
                if (result.Errors.Count > 10)
                    msg += Environment.NewLine + "... and " + (result.Errors.Count - 10) + " more.";

                MessageBox.Show(this, msg, "Whitelist not saved",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                SelectLine(result.Errors[0].Line);
                return;
            }

            try
            {
                _store.SaveText(whitelistTextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Cannot save the whitelist:\n" + ex.Message,
                    "OfficeFileGuard", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _originalText = whitelistTextBox.Text;
            DialogResult = DialogResult.OK;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (whitelistTextBox.Text != _originalText)
            {
                var answer = MessageBox.Show(this, "Discard your changes?", "OfficeFileGuard",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (answer != DialogResult.Yes) return;
            }
            DialogResult = DialogResult.Cancel;
        }


        private void SelectLine(int line)
        {
            int start = whitelistTextBox.GetFirstCharIndexFromLine(line - 1);
            if (start < 0) return;
            whitelistTextBox.Focus();
            whitelistTextBox.Select(start, whitelistTextBox.Lines[line - 1].Length);
            whitelistTextBox.ScrollToCaret();
        }

        private void whitelistTextBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
