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
        private void BuildConnectPage()
        {
            AddPageHeader(_connectPage, "Connect", "Luna auto-attaches when FH6 is already open. Manual controls live in Settings.");

            var welcome = MakeCard(_connectPage, 0, 72, ContentWidth, 126, string.Empty, string.Empty);
            welcome.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            _welcomeTitle = new AnimatedGradientLabel();
            _welcomeTitle.Text = "Welcome to Luna";
            _welcomeTitle.Location = new Point(18, 22);
            _welcomeTitle.Size = new Size(858, 44);
            _welcomeTitle.Font = new Font("Segoe UI Semibold", 24F, FontStyle.Bold);
            _welcomeTitle.ForeColor = AccentBlue;
            _welcomeTitle.BackColor = Surface;
            _welcomeTitle.TextAlign = ContentAlignment.MiddleCenter;
            welcome.Controls.Add(_welcomeTitle);

            var freeForever = MakeBodyLabel("Unleash the Horizon.", 18, 72, 858, 28);
            freeForever.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
            freeForever.TextAlign = ContentAlignment.MiddleCenter;
            freeForever.BackColor = Surface;
            welcome.Controls.Add(freeForever);
            UpdateWelcomeTitleColor();

            var connection = MakeCard(_connectPage, 0, 222, ContentWidth, 272, "Connection Status", "Green means connected, yellow means connecting, and red means disconnected.");
            connection.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var statusPanel = new Panel();
            statusPanel.Location = new Point(18, 62);
            statusPanel.Size = new Size(858, 178);
            statusPanel.BackColor = SurfaceAlt;
            connection.Controls.Add(statusPanel);

            var statusTitle = new Label();
            statusTitle.Text = "Connection Status";
            statusTitle.Location = new Point(18, 18);
            statusTitle.Size = new Size(822, 26);
            statusTitle.Font = new Font("Segoe UI Semibold", 12F);
            statusTitle.ForeColor = TextPrimary;
            statusTitle.BackColor = SurfaceAlt;
            statusTitle.TextAlign = ContentAlignment.MiddleCenter;
            statusPanel.Controls.Add(statusTitle);

            _connectionState = new Label();
            _connectionState.Text = "\u2715  Disconnected";
            _connectionState.Font = new Font("Segoe UI Semibold", 28F);
            _connectionState.ForeColor = AccentRed;
            _connectionState.Location = new Point(18, 50);
            _connectionState.Size = new Size(822, 60);
            _connectionState.BackColor = SurfaceAlt;
            _connectionState.TextAlign = ContentAlignment.MiddleCenter;
            statusPanel.Controls.Add(_connectionState);

            var statusHint = MakeBodyLabel("If FH6 is open, Luna will try to attach by itself. Use Settings only when you want to press Attach manually or pick a fallback folder.", 18, 126, 822, 34);
            statusHint.BackColor = SurfaceAlt;
            statusHint.TextAlign = ContentAlignment.TopCenter;
            statusPanel.Controls.Add(statusHint);
        }

        private void ShowConnectPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowConnectPage);
                return;
            }
            HidePages();
            _connectPage.Visible = true;
            _connectPage.BringToFront();
            var attached = _database != null && _database.IsAlive;
            SetConnectionState(attached);
            SetStatus(attached ? "Connected" : "Disconnected");
            UpdateNavigationState(_navConnect);
        }
    }
}
