using System;
using System.Drawing;
using System.Windows.Forms;

namespace ForzaHorizon6AutoshowUnlocker
{
    public sealed partial class MainForm
    {
        private sealed class TutorialGuideSpec
        {
            public string Title;
            public string Body;
            public string Icon;
            public Color Accent;
            public Action Action;
        }

        private void BuildTutorialPage()
        {
            _tutorialPage.AutoScroll = true;
            AddPageHeader(_tutorialPage, "Tutorial", "Learn Luna's main workflows with direct, task-focused guides.");

            var startPanel = new ModernPanel();
            startPanel.SetBounds(0, 72, ContentWidth, 122);
            startPanel.FillColor = Surface;
            startPanel.BorderColor = Blend(Border, AccentBlue, 0.28F);
            startPanel.CornerRadius = 12;
            startPanel.BorderWidth = 1.2F;
            _tutorialPage.Controls.Add(startPanel);

            var startTitle = new Label();
            startTitle.Text = "Start with the safe workflow";
            startTitle.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            startTitle.ForeColor = TextPrimary;
            startTitle.BackColor = Color.Transparent;
            startTitle.SetBounds(20, 16, 360, 24);
            startPanel.Controls.Add(startTitle);

            var startDetail = new Label();
            startDetail.Text = "The same three steps apply to most Luna tools.";
            startDetail.Font = new Font("Segoe UI", 9F);
            startDetail.ForeColor = TextMuted;
            startDetail.BackColor = Color.Transparent;
            startDetail.SetBounds(20, 40, 400, 20);
            startPanel.Controls.Add(startDetail);

            AddTutorialStep(startPanel, 20, 72, "1", "Launch FH6", "Reach the menu or world before attaching.", Color.FromArgb(59, 130, 246));
            AddTutorialStep(startPanel, 314, 72, "2", "Connect Luna", "Wait for the green Connected badge.", Color.FromArgb(16, 185, 129));
            AddTutorialStep(startPanel, 608, 72, "3", "Apply carefully", "Use small values and verify in game.", Color.FromArgb(245, 158, 11));

            AddTutorialSectionLabel(_tutorialPage, 218, "Tool guides", "Open a guide card to go directly to that tool.");

            var guides = new[]
            {
                new TutorialGuideSpec { Title = "Connect", Body = "Start FH6 first. Luna normally attaches automatically; Settings contains the manual path options.", Icon = "Connection.png", Accent = Color.FromArgb(6, 182, 212), Action = ShowConnectPage },
                new TutorialGuideSpec { Title = "Features", Body = "Set a value, switch the row on, then press Apply. Switching it off sends the stop command immediately.", Icon = "features (1).png", Accent = Color.FromArgb(59, 130, 246), Action = ShowFeaturesPage },
                new TutorialGuideSpec { Title = "Autoshow", Body = "Load cars, select the entries you want, then use Add Selected. Review removals before confirming.", Icon = "Autoshow.png", Accent = Color.FromArgb(16, 185, 129), Action = ShowMainPage },
                new TutorialGuideSpec { Title = "Database Tools", Body = "Choose one database action, let it finish, then reload the matching FH6 menu to refresh cached data.", Icon = "database (1).png", Accent = Color.FromArgb(249, 115, 22), Action = ShowProfilePageWithDatabaseWarning },
                new TutorialGuideSpec { Title = "Driving Editor", Body = "Use small live handling values first. Enable only the rows you need, then apply the live hooks.", Icon = "car-maintenance.png", Accent = Color.FromArgb(45, 212, 191), Action = ShowDrivingPage },
                new TutorialGuideSpec { Title = "Driving Tuning", Body = "Start the live scan, load current values, edit selected rows, then apply or restore the snapshot.", Icon = "service_846355.png", Accent = Color.FromArgb(239, 68, 68), Action = ShowTuningPage },
                new TutorialGuideSpec { Title = "Teleport", Body = "Save a location, select it, enable the hotkey, or replay a saved list using the configured play key.", Icon = "teleport (1).png", Accent = Color.FromArgb(34, 197, 94), Action = ShowTeleportPage },
                new TutorialGuideSpec { Title = "Settings", Body = "Change language, appearance, attach behavior, and review the installed Luna version.", Icon = "setting.png", Accent = Color.FromArgb(148, 163, 184), Action = ShowSettingsPage },
                new TutorialGuideSpec { Title = "Console", Body = "Review the live action history, copy useful diagnostics, or open the persistent log folder.", Icon = "consoel.png", Accent = Color.FromArgb(161, 161, 170), Action = ShowConsolePage },
                new TutorialGuideSpec { Title = "Feedback", Body = "Report a problem or suggest an improvement through Luna's embedded feedback page.", Icon = "bug.png", Accent = Color.FromArgb(244, 63, 94), Action = ShowFeedbackBugPage }
            };

            const int guideTop = 258;
            const int cardGap = 14;
            const int cardHeight = 104;
            var cardWidth = (ContentWidth - cardGap) / 2;
            for (var i = 0; i < guides.Length; i++)
            {
                var column = i % 2;
                var row = i / 2;
                AddTutorialGuideCard(
                    _tutorialPage,
                    column * (cardWidth + cardGap),
                    guideTop + (row * (cardHeight + cardGap)),
                    cardWidth,
                    cardHeight,
                    guides[i]);
            }

            var workflowTop = guideTop + (5 * (cardHeight + cardGap)) + 18;
            AddTutorialSectionLabel(_tutorialPage, workflowTop, "Live feature workflow", "A consistent pattern used throughout Features and Driving Editor.");

            var workflowPanel = new ModernPanel();
            workflowPanel.SetBounds(0, workflowTop + 40, ContentWidth, 112);
            workflowPanel.FillColor = Surface;
            workflowPanel.BorderColor = Border;
            workflowPanel.CornerRadius = 12;
            _tutorialPage.Controls.Add(workflowPanel);

            const int workflowInset = 18;
            const int workflowGap = 8;
            var workflowSlotWidth = (ContentWidth - (workflowInset * 2) - (workflowGap * 3)) / 4;
            AddTutorialWorkflowItem(workflowPanel, workflowInset, workflowSlotWidth, "01", "Choose a value", "Type a number or open the row's configuration.", AccentBlue);
            AddTutorialWorkflowItem(workflowPanel, workflowInset + workflowSlotWidth + workflowGap, workflowSlotWidth, "02", "Switch it on", "Green means active. Red means stopped.", AccentGreen);
            AddTutorialWorkflowItem(workflowPanel, workflowInset + ((workflowSlotWidth + workflowGap) * 2), workflowSlotWidth, "03", "Press Apply", "Wait for the status message before continuing.", Color.FromArgb(245, 158, 11));
            AddTutorialWorkflowItem(workflowPanel, workflowInset + ((workflowSlotWidth + workflowGap) * 3), workflowSlotWidth, "04", "Verify in FH6", "Reload the relevant menu if the game cached it.", AccentPurple);

            var tipsTop = workflowPanel.Bottom + 24;
            AddTutorialSectionLabel(_tutorialPage, tipsTop, "Useful refresh triggers", "Some live values update only when FH6 reads that system again.");
            AddTutorialTip(_tutorialPage, 0, tipsTop + 40, 286, "Profile values", "Enter and leave Autoshow for credits, wheelspins, and related profile values.", Color.FromArgb(16, 185, 129));
            AddTutorialTip(_tutorialPage, 302, tipsTop + 40, 286, "Driving values", "Switch cars or reload the world when handling or database values appear cached.", Color.FromArgb(45, 212, 191));
            AddTutorialTip(_tutorialPage, 604, tipsTop + 40, 292, "Logs and support", "Use Console and Open Folder when sharing a reproducible failure.", Color.FromArgb(99, 102, 241));

            _tutorialPage.AutoScrollMinSize = new Size(ContentWidth, tipsTop + 144);
        }

        private static void AddTutorialSectionLabel(Control parent, int top, string title, string subtitle)
        {
            var titleLabel = new Label();
            titleLabel.Text = title;
            titleLabel.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
            titleLabel.ForeColor = TextPrimary;
            titleLabel.BackColor = AppBackground;
            titleLabel.SetBounds(4, top, 300, 22);
            parent.Controls.Add(titleLabel);

            var subtitleLabel = new Label();
            subtitleLabel.Text = subtitle;
            subtitleLabel.Font = new Font("Segoe UI", 8.75F);
            subtitleLabel.ForeColor = TextMuted;
            subtitleLabel.BackColor = AppBackground;
            subtitleLabel.SetBounds(310, top + 1, 580, 20);
            subtitleLabel.TextAlign = ContentAlignment.MiddleRight;
            parent.Controls.Add(subtitleLabel);
        }

        private static void AddTutorialStep(Control parent, int left, int top, string number, string title, string body, Color accent)
        {
            var badge = new NoteBadge();
            badge.Text = number;
            badge.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            badge.FillColor = Blend(Surface, accent, 0.22F);
            badge.GlyphColor = accent;
            badge.Tag = accent;
            badge.SetBounds(left, top, 34, 34);
            parent.Controls.Add(badge);

            var titleLabel = new Label();
            titleLabel.Text = title;
            titleLabel.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
            titleLabel.ForeColor = TextPrimary;
            titleLabel.BackColor = Color.Transparent;
            titleLabel.SetBounds(left + 44, top - 1, 220, 18);
            parent.Controls.Add(titleLabel);

            var bodyLabel = new Label();
            bodyLabel.Text = body;
            bodyLabel.Font = new Font("Segoe UI", 8.25F);
            bodyLabel.ForeColor = TextMuted;
            bodyLabel.BackColor = Color.Transparent;
            bodyLabel.SetBounds(left + 44, top + 17, 232, 34);
            parent.Controls.Add(bodyLabel);
        }

        private void AddTutorialGuideCard(Control parent, int left, int top, int width, int height, TutorialGuideSpec spec)
        {
            var card = new ModernPanel();
            card.SetBounds(left, top, width, height);
            card.FillColor = Surface;
            card.BorderColor = Blend(Border, spec.Accent, 0.20F);
            card.CornerRadius = 10;
            card.BorderWidth = 1F;
            card.Cursor = Cursors.Hand;
            parent.Controls.Add(card);

            var icon = new PictureBox();
            icon.Image = LoadLocalAssetIconImage(spec.Icon, spec.Accent);
            icon.SizeMode = PictureBoxSizeMode.Zoom;
            icon.BackColor = Color.Transparent;
            icon.Tag = "HubIcon";
            icon.SetBounds(18, 22, 48, 48);
            card.Controls.Add(icon);

            var title = new Label();
            title.Text = spec.Title;
            title.Font = new Font("Segoe UI Semibold", 10.25F, FontStyle.Bold);
            title.ForeColor = TextPrimary;
            title.BackColor = Color.Transparent;
            title.SetBounds(82, 16, width - 102, 22);
            card.Controls.Add(title);

            var body = new Label();
            body.Text = spec.Body;
            body.Font = new Font("Segoe UI", 8.5F);
            body.ForeColor = TextMuted;
            body.BackColor = Color.Transparent;
            body.SetBounds(82, 40, width - 102, 50);
            card.Controls.Add(body);

            EventHandler enter = delegate
            {
                card.FillColor = Blend(Surface, spec.Accent, 0.07F);
                card.BorderColor = spec.Accent;
                card.Invalidate();
            };
            EventHandler leave = delegate
            {
                card.FillColor = Surface;
                card.BorderColor = Blend(Border, spec.Accent, 0.20F);
                card.Invalidate();
            };
            EventHandler click = delegate { spec.Action(); };
            foreach (var control in new Control[] { card, icon, title, body })
            {
                control.Cursor = Cursors.Hand;
                control.MouseEnter += enter;
                control.MouseLeave += leave;
                control.Click += click;
            }
        }

        private static void AddTutorialWorkflowItem(Control parent, int slotLeft, int slotWidth, string number, string title, string body, Color accent)
        {
            var contentWidth = Math.Min(194, Math.Max(150, slotWidth - 10));
            var left = slotLeft + Math.Max(0, (slotWidth - contentWidth) / 2);
            var top = Math.Max(12, (parent.Height - 70) / 2);

            var badge = new NoteBadge();
            badge.Text = number;
            badge.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
            badge.FillColor = Blend(Surface, accent, 0.22F);
            badge.GlyphColor = accent;
            badge.Tag = accent;
            badge.SetBounds(left, top + 2, 34, 34);
            parent.Controls.Add(badge);

            var titleLabel = new Label();
            titleLabel.Text = title;
            titleLabel.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            titleLabel.ForeColor = TextPrimary;
            titleLabel.BackColor = Color.Transparent;
            titleLabel.SetBounds(left + 44, top, contentWidth - 44, 20);
            parent.Controls.Add(titleLabel);

            var bodyLabel = new Label();
            bodyLabel.Text = body;
            bodyLabel.Font = new Font("Segoe UI", 8.25F);
            bodyLabel.ForeColor = TextMuted;
            bodyLabel.BackColor = Color.Transparent;
            bodyLabel.SetBounds(left + 44, top + 23, contentWidth - 44, 46);
            parent.Controls.Add(bodyLabel);
        }

        private static void AddTutorialTip(Control parent, int left, int top, int width, string title, string body, Color accent)
        {
            var panel = new ModernPanel();
            panel.SetBounds(left, top, width, 78);
            panel.FillColor = Surface;
            panel.BorderColor = Blend(Border, accent, 0.24F);
            panel.CornerRadius = 10;
            parent.Controls.Add(panel);

            var marker = new Panel();
            marker.BackColor = accent;
            marker.SetBounds(0, 16, 3, 46);
            panel.Controls.Add(marker);

            var titleLabel = new Label();
            titleLabel.Text = title;
            titleLabel.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
            titleLabel.ForeColor = TextPrimary;
            titleLabel.BackColor = Color.Transparent;
            titleLabel.SetBounds(16, 12, width - 30, 20);
            panel.Controls.Add(titleLabel);

            var bodyLabel = new Label();
            bodyLabel.Text = body;
            bodyLabel.Font = new Font("Segoe UI", 8.25F);
            bodyLabel.ForeColor = TextMuted;
            bodyLabel.BackColor = Color.Transparent;
            bodyLabel.SetBounds(16, 34, width - 30, 36);
            panel.Controls.Add(bodyLabel);
        }

        private void ShowTutorialPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowTutorialPage);
                return;
            }
            HidePages();
            _tutorialPage.Visible = true;
            _tutorialPage.BringToFront();
            SetStatus("Tutorial");
            UpdateNavigationState(_navTutorial);
        }
    }
}
