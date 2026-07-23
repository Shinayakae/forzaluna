using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ForzaHorizon6AutoshowUnlocker
{
    public sealed partial class MainForm
    {
        private RichTextBox BuildConsolePage()
        {
            AddPageHeader(_consolePage, "Console", "Review Luna's live activity, copy diagnostics, and open the persistent log.");

            var commandBar = new ModernPanel();
            commandBar.SetBounds(0, 72, ContentWidth, 86);
            commandBar.FillColor = Surface;
            commandBar.BorderColor = Border;
            commandBar.CornerRadius = 12;
            commandBar.BorderWidth = 1F;
            _consolePage.Controls.Add(commandBar);

            var badge = new NoteBadge();
            badge.Text = "LOG";
            badge.Font = new Font("Segoe UI Semibold", 7.5F, FontStyle.Bold);
            badge.FillColor = Blend(Surface, Color.FromArgb(161, 161, 170), 0.20F);
            badge.GlyphColor = Color.FromArgb(161, 161, 170);
            badge.Tag = Color.FromArgb(161, 161, 170);
            badge.SetBounds(18, 21, 44, 44);
            commandBar.Controls.Add(badge);

            var commandTitle = new Label();
            commandTitle.Text = "Session activity";
            commandTitle.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
            commandTitle.ForeColor = TextPrimary;
            commandTitle.BackColor = Color.Transparent;
            commandTitle.SetBounds(76, 17, 250, 22);
            commandBar.Controls.Add(commandTitle);

            _consoleStatusLabel = new Label();
            _consoleStatusLabel.Text = "0 entries  |  " + Path.GetFileName(_logPath);
            _consoleStatusLabel.Font = new Font("Segoe UI", 8.75F);
            _consoleStatusLabel.ForeColor = TextMuted;
            _consoleStatusLabel.BackColor = Color.Transparent;
            _consoleStatusLabel.SetBounds(76, 41, 390, 20);
            commandBar.Controls.Add(_consoleStatusLabel);

            var openFolder = MakeButton("Open Folder", 748, 25, 128, 36);
            MakeAccentButton(openFolder, AccentBlue);
            openFolder.Click += delegate { OpenResultsFolder(); };
            commandBar.Controls.Add(openFolder);

            _consoleOverlayButton = MakeButton("Overlay Off", 604, 25, 128, 36);
            _consoleOverlayButton.Click += delegate
            {
                SetConsoleOverlayEnabled(!_consoleOverlayEnabled, true);
            };
            commandBar.Controls.Add(_consoleOverlayButton);
            UpdateConsoleOverlayButton();

            var logShell = new ModernPanel();
            logShell.SetBounds(0, 176, ContentWidth, 566);
            logShell.FillColor = Surface;
            logShell.BorderColor = Border;
            logShell.CornerRadius = 12;
            logShell.BorderWidth = 1F;
            _consolePage.Controls.Add(logShell);

            var logTitle = new Label();
            logTitle.Text = "Live log";
            logTitle.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
            logTitle.ForeColor = TextPrimary;
            logTitle.BackColor = Color.Transparent;
            logTitle.SetBounds(18, 15, 160, 24);
            logShell.Controls.Add(logTitle);

            var liveDot = new Panel();
            liveDot.BackColor = AccentGreen;
            liveDot.SetBounds(88, 23, 7, 7);
            logShell.Controls.Add(liveDot);

            var log = new RichTextBox();
            log.SetBounds(18, 52, logShell.Width - 36, 448);
            log.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            StyleLogBox(log);
            logShell.Controls.Add(log);

            var footerLine = new Panel();
            footerLine.BackColor = Border;
            footerLine.SetBounds(18, 514, logShell.Width - 36, 1);
            footerLine.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            logShell.Controls.Add(footerLine);

            var copy = MakeButton("Copy Log", 18, 525, 104, 30);
            copy.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            copy.Click += delegate
            {
                var text = string.IsNullOrEmpty(log.SelectedText) ? log.Text : log.SelectedText;
                if (!string.IsNullOrEmpty(text))
                    Clipboard.SetText(text);
                SetStatus("Console copied");
            };
            logShell.Controls.Add(copy);

            var clear = MakeButton("Clear View", 132, 525, 104, 30);
            clear.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            clear.Click += delegate
            {
                log.Clear();
                _consoleStatusLabel.Text = "0 entries  |  " + Path.GetFileName(_logPath);
                RefreshConsoleOverlay();
                SetStatus("Console view cleared");
            };
            logShell.Controls.Add(clear);

            _consoleAutoScrollButton = MakeButton("Auto-scroll On", 246, 525, 124, 30);
            _consoleAutoScrollButton.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            _consoleAutoScrollButton.Click += delegate
            {
                _consoleAutoScroll = !_consoleAutoScroll;
                _consoleAutoScrollButton.Text = _consoleAutoScroll ? "Auto-scroll On" : "Auto-scroll Off";
                if (_consoleAutoScroll)
                {
                    log.SelectionStart = log.TextLength;
                    log.ScrollToCaret();
                }
                SetStatus(_consoleAutoScroll ? "Console auto-scroll on" : "Console auto-scroll off");
            };
            logShell.Controls.Add(_consoleAutoScrollButton);

            var persistence = new Label();
            persistence.Text = "Clear View does not delete the persistent log file.";
            persistence.Font = new Font("Segoe UI", 8.25F);
            persistence.ForeColor = TextMuted;
            persistence.BackColor = Color.Transparent;
            persistence.TextAlign = ContentAlignment.MiddleRight;
            persistence.SetBounds(logShell.Width - 364, 526, 346, 28);
            persistence.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            logShell.Controls.Add(persistence);

            _consolePage.AutoScrollMinSize = new Size(ContentWidth, logShell.Bottom + 28);
            return log;
        }

        private void ShowConsolePage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowConsolePage);
                return;
            }
            HidePages();
            _consolePage.Visible = true;
            _consolePage.BringToFront();
            if (_log != null && _consoleStatusLabel != null)
            {
                var lineCount = _log.Lines.Count(item => !string.IsNullOrWhiteSpace(item));
                _consoleStatusLabel.Text = lineCount.ToString(CultureInfo.InvariantCulture) + " entries  |  " + Path.GetFileName(_logPath);
            }
            UpdateConsoleOverlayButton();
            SetStatus("Console");
            UpdateNavigationState(_navConsole);
        }
    }
}
