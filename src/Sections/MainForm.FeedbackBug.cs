using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace ForzaHorizon6AutoshowUnlocker
{
    public sealed partial class MainForm
    {
        private const string FeedbackBugUrl = "https://forms.gle/ozsrnxHUf46DWb7E7";
        private WebView2 _feedbackBugWebView;
        private Label _feedbackBugStatusLabel;
        private bool _feedbackBugWebViewInitializing;

        private void BuildFeedbackBugPage()
        {
            _feedbackBugPage.AutoScroll = true;
            AddPageHeader(_feedbackBugPage, "Feedback", "Send feedback, report bugs, or share what needs a closer look.");

            var viewer = new ModernPanel();
            viewer.Location = new Point(0, 72);
            viewer.Size = new Size(ContentWidth, 760);
            viewer.FillColor = Surface;
            viewer.BorderColor = Border;
            viewer.CornerRadius = 8;
            viewer.BorderWidth = 1F;
            viewer.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            _feedbackBugPage.Controls.Add(viewer);

            var viewerTitle = new Label();
            viewerTitle.Text = "Feedback";
            viewerTitle.Location = new Point(18, 10);
            viewerTitle.Size = new Size(420, 34);
            viewerTitle.Font = new Font("Segoe UI Semibold", 11F);
            viewerTitle.ForeColor = TextPrimary;
            viewerTitle.BackColor = Color.Transparent;
            viewerTitle.TextAlign = ContentAlignment.MiddleLeft;
            viewer.Controls.Add(viewerTitle);

            var open = MakeButton("Open In Browser", ContentWidth - 188, 10, 170, 34);
            MakeAccentButton(open, Color.FromArgb(255, 111, 145));
            open.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            open.Click += delegate
            {
                try
                {
                    Process.Start(new ProcessStartInfo(FeedbackBugUrl) { UseShellExecute = true });
                    SetStatus("Feedback opened");
                }
                catch (Exception ex)
                {
                    ShowTranslatedMessageBox(this, "Could not open the feedback form: " + ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
            viewer.Controls.Add(open);
            SetTranslatedToolTip(open, "Open the feedback and bug report form in your default browser.");

            var browserFrame = new Panel();
            browserFrame.Location = new Point(12, 54);
            browserFrame.Size = new Size(ContentWidth - 24, 694);
            browserFrame.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            browserFrame.BackColor = SurfaceAlt;
            browserFrame.Padding = new Padding(1);
            viewer.Controls.Add(browserFrame);

            _feedbackBugStatusLabel = new Label();
            _feedbackBugStatusLabel.Dock = DockStyle.Fill;
            _feedbackBugStatusLabel.Padding = new Padding(28);
            _feedbackBugStatusLabel.TextAlign = ContentAlignment.MiddleCenter;
            _feedbackBugStatusLabel.Font = new Font("Segoe UI Semibold", 10F);
            _feedbackBugStatusLabel.ForeColor = TextMuted;
            _feedbackBugStatusLabel.BackColor = SurfaceAlt;
            _feedbackBugStatusLabel.Text = "Loading feedback form...";
            browserFrame.Controls.Add(_feedbackBugStatusLabel);

            _feedbackBugWebView = new WebView2();
            _feedbackBugWebView.Dock = DockStyle.Fill;
            _feedbackBugWebView.AllowExternalDrop = false;
            _feedbackBugWebView.DefaultBackgroundColor = SurfaceAlt;
            browserFrame.Controls.Add(_feedbackBugWebView);
            _feedbackBugWebView.BringToFront();

            _feedbackBugPage.AutoScrollMinSize = new Size(ContentWidth, 852);
        }

        private async void InitializeFeedbackBugWebView()
        {
            if (_feedbackBugWebView == null || _feedbackBugWebView.IsDisposed || _feedbackBugWebViewInitializing)
                return;

            if (_feedbackBugWebView.CoreWebView2 != null)
            {
                NavigateFeedbackBugWebView();
                return;
            }

            _feedbackBugWebViewInitializing = true;
            try
            {
                var userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Forza Horizon 6 Luna",
                    "WebView2UserData");
                Directory.CreateDirectory(userDataFolder);

                var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, null);
                await _feedbackBugWebView.EnsureCoreWebView2Async(environment);

                var settings = _feedbackBugWebView.CoreWebView2.Settings;
                settings.AreDevToolsEnabled = false;
                settings.AreDefaultScriptDialogsEnabled = true;
                settings.AreDefaultContextMenusEnabled = true;

                NavigateFeedbackBugWebView();
                if (_feedbackBugStatusLabel != null && !_feedbackBugStatusLabel.IsDisposed)
                    _feedbackBugStatusLabel.Visible = false;
            }
            catch
            {
                if (_feedbackBugWebView != null && !_feedbackBugWebView.IsDisposed)
                    _feedbackBugWebView.Visible = false;
                ShowFeedbackBugStatus("WebView2 could not start. Keep the WebView2 DLLs beside Luna, or use Open In Browser.");
            }
            finally
            {
                _feedbackBugWebViewInitializing = false;
            }
        }

        private void NavigateFeedbackBugWebView()
        {
            if (_feedbackBugWebView == null || _feedbackBugWebView.IsDisposed || _feedbackBugWebView.CoreWebView2 == null)
                return;

            _feedbackBugWebView.CoreWebView2.Navigate(FeedbackBugUrl);
        }

        private void ShowFeedbackBugStatus(string message)
        {
            if (_feedbackBugStatusLabel == null || _feedbackBugStatusLabel.IsDisposed)
                return;

            _feedbackBugStatusLabel.Text = message;
            _feedbackBugStatusLabel.Visible = true;
            _feedbackBugStatusLabel.BringToFront();
        }

        private void ShowFeedbackBugPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowFeedbackBugPage);
                return;
            }

            HidePages();
            _feedbackBugPage.Visible = true;
            _feedbackBugPage.BringToFront();
            SetStatus("Feedback");
            UpdateNavigationState(_navFeedbackBug);
            InitializeFeedbackBugWebView();
        }
    }
}
