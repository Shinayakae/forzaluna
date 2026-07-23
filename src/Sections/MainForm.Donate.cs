using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ForzaHorizon6AutoshowUnlocker
{
    public sealed partial class MainForm
    {
        private void BuildDonatePage()
        {
            _donatePage.AutoScroll = true;
            AddPageHeader(_donatePage, "Donate", "Thank you for being here and supporting Luna.");

            var donateCard = MakeCard(_donatePage, 0, 82, ContentWidth, 540, string.Empty, string.Empty);
            donateCard.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            var logoPanel = new ModernPanel();
            logoPanel.Location = new Point(28, 30);
            logoPanel.Size = new Size(286, 480);
            logoPanel.FillColor = Blend(SurfaceAlt, Color.FromArgb(255, 95, 31), 0.05F);
            logoPanel.BorderColor = Blend(Border, Color.FromArgb(255, 95, 31), 0.24F);
            logoPanel.CornerRadius = 14;
            logoPanel.BorderWidth = 1F;
            logoPanel.Tag = "DonatePanel";
            donateCard.Controls.Add(logoPanel);

            var logo = new PictureBox();
            logo.Location = new Point(70, 44);
            logo.Size = new Size(146, 146);
            logo.BackColor = logoPanel.FillColor;
            logo.SizeMode = PictureBoxSizeMode.Zoom;
            logo.Tag = "DonateLogo";
            logo.Image = LoadEmbeddedImage("kofi-logo.png");
            logoPanel.Controls.Add(logo);

            if (logo.Image == null)
            {
                var fallback = new Label();
                fallback.Text = "Ko-fi";
                fallback.Location = logo.Location;
                fallback.Size = logo.Size;
                fallback.Font = new Font("Segoe UI Semibold", 18F);
                fallback.ForeColor = TextPrimary;
                fallback.BackColor = Color.Transparent;
                fallback.TextAlign = ContentAlignment.MiddleCenter;
                logoPanel.Controls.Add(fallback);
                fallback.BringToFront();
            }

            var supportTitle = new Label();
            supportTitle.Text = "Support!";
            supportTitle.Location = new Point(24, 222);
            supportTitle.Size = new Size(238, 30);
            supportTitle.Font = new Font("Segoe UI Semibold", 14F);
            supportTitle.ForeColor = TextPrimary;
            supportTitle.BackColor = Color.Transparent;
            supportTitle.TextAlign = ContentAlignment.MiddleCenter;
            logoPanel.Controls.Add(supportTitle);

            var supportSub = MakeBodyLabel("Donations are never required. They just help keep Luna moving forward.", 28, 258, 230, 62);
            supportSub.BackColor = Color.Transparent;
            supportSub.TextAlign = ContentAlignment.MiddleCenter;
            supportSub.Font = new Font("Segoe UI", 9.25F);
            logoPanel.Controls.Add(supportSub);

            var donate = MakeButton("Donate on Ko-fi", 42, 350, 202, 42);
            MakeAccentButton(donate, Color.FromArgb(255, 95, 31));
            donate.Click += delegate { OpenDonationPage(); };
            logoPanel.Controls.Add(donate);
            SetTranslatedToolTip(donate, "Open " + DonationUrl + " in your default browser.");

            var crypto = MakeButton("Donate with Crypto", 42, 406, 202, 42);
            MakeAccentButton(crypto, Color.FromArgb(70, 168, 131));
            crypto.Click += delegate { ShowCryptoDonateDialog(); };
            logoPanel.Controls.Add(crypto);
            SetTranslatedToolTip(crypto, "Show the USDT Ethereum wallet QR and copy button.");

            var messagePanel = new ModernPanel();
            messagePanel.Location = new Point(338, 30);
            messagePanel.Size = new Size(534, 480);
            messagePanel.FillColor = SurfaceAlt;
            messagePanel.BorderColor = Border;
            messagePanel.CornerRadius = 14;
            messagePanel.BorderWidth = 1F;
            messagePanel.Tag = "DonatePanel";
            donateCard.Controls.Add(messagePanel);

            var message = new Label();
            message.Text =
                "First of all, thank you all for the support, feedback, and excitement around this project. Seeing people enjoy something I did for fun honestly means a lot to me. This project would not be possible without the help of others." + Environment.NewLine + Environment.NewLine +
                "Donations are completely optional. Seriously. Nothing is expected from anyone. I'm still going to keep working on this because I genuinely enjoy doing it and I love sharing it with the community." + Environment.NewLine + Environment.NewLine +
                "If you do choose to support, it helps with a lot of things. More than anything, it motivates me to keep improving things for you guys." + Environment.NewLine + Environment.NewLine +
                "But whether you donate or not, thank you for being here and supporting the project. That alone already means a lot.";
            message.Location = new Point(44, 58);
            message.Size = new Size(446, 360);
            message.Font = new Font("Segoe UI", 10.25F);
            message.ForeColor = TextPrimary;
            message.BackColor = Color.Transparent;
            message.TextAlign = ContentAlignment.MiddleCenter;
            messagePanel.Controls.Add(message);

            _donatePage.AutoScrollMinSize = new Size(ContentWidth, donateCard.Bottom + 30);
        }

        private void OpenDonationPage()
        {
            try
            {
                var psi = new ProcessStartInfo(DonationUrl);
                psi.UseShellExecute = true;
                Process.Start(psi);
                Log("Opening donation page: " + DonationUrl);
            }
            catch (Exception ex)
            {
                ShowTranslatedMessageBox("Could not open the donation page: " + ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Log("Could not open donation page: " + ex.Message);
            }
        }

        private void ShowCryptoDonateDialog()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowCryptoDonateDialog);
                return;
            }

            using (var dialog = new Form())
            {
                dialog.Text = "Donate with Crypto";
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.ClientSize = new Size(560, 498);
                dialog.BackColor = AppBackground;
                dialog.ForeColor = TextPrimary;
                dialog.Font = new Font("Segoe UI", 9F);

                var card = new ModernPanel();
                card.Location = new Point(14, 14);
                card.Size = new Size(532, 470);
                card.FillColor = Surface;
                card.BorderColor = Border;
                card.CornerRadius = 14;
                dialog.Controls.Add(card);

                var title = new Label();
                title.Text = "Donate with Crypto";
                title.Font = new Font("Segoe UI Semibold", 14F);
                title.ForeColor = TextPrimary;
                title.BackColor = Surface;
                title.Location = new Point(24, 20);
                title.Size = new Size(484, 30);
                title.TextAlign = ContentAlignment.MiddleCenter;
                card.Controls.Add(title);

                var qr = new PictureBox();
                qr.Location = new Point(50, 66);
                qr.Size = new Size(432, 180);
                qr.BackColor = SurfaceAlt;
                qr.SizeMode = PictureBoxSizeMode.Zoom;
                qr.Image = LoadEmbeddedImage("crypto-usdt.png");
                card.Controls.Add(qr);

                var network = new Label();
                network.Text = "USDT on Ethereum network";
                network.Font = new Font("Segoe UI Semibold", 10F);
                network.ForeColor = AccentGreen;
                network.BackColor = Surface;
                network.Location = new Point(24, 268);
                network.Size = new Size(484, 24);
                network.TextAlign = ContentAlignment.MiddleCenter;
                card.Controls.Add(network);

                var walletLabel = new Label();
                walletLabel.Text = "Wallet";
                walletLabel.Font = new Font("Segoe UI Semibold", 9F);
                walletLabel.ForeColor = TextPrimary;
                walletLabel.BackColor = Surface;
                walletLabel.Location = new Point(24, 314);
                walletLabel.Size = new Size(484, 20);
                card.Controls.Add(walletLabel);

                var wallet = new TextBox();
                wallet.Text = CryptoWalletAddress;
                wallet.Location = new Point(24, 338);
                wallet.Size = new Size(484, 28);
                wallet.ReadOnly = true;
                wallet.TextAlign = HorizontalAlignment.Center;
                StyleTextBox(wallet);
                card.Controls.Add(wallet);

                var copy = MakeButton("Copy Wallet", 286, 410, 108, 32);
                MakeAccentButton(copy, AccentGreen);
                copy.Click += delegate
                {
                    Clipboard.SetText(CryptoWalletAddress);
                    SetStatus("Wallet copied");
                    Log("Crypto wallet copied.");
                };
                card.Controls.Add(copy);

                var close = MakeButton("Close", 408, 410, 100, 32);
                close.Click += delegate
                {
                    dialog.DialogResult = DialogResult.OK;
                    dialog.Close();
                };
                card.Controls.Add(close);
                dialog.AcceptButton = close;

                PrepareDialogForLanguage(dialog);
                dialog.ShowDialog(this);
            }
        }

        private void ShowDonatePage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowDonatePage);
                return;
            }
            HidePages();
            _donatePage.Visible = true;
            _donatePage.BringToFront();
            SetStatus("Donate");
            UpdateNavigationState(_navDonate);
        }
    }
}
