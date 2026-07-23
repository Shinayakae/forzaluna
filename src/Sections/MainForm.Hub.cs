using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ForzaHorizon6AutoshowUnlocker
{
    public sealed partial class MainForm
    {
        private enum LunaConnectionState
        {
            Disconnected,
            Connecting,
            Connected
        }

        private sealed class HubTileSpec
        {
            public readonly string Title;
            public readonly string Subtitle;
            public readonly string IconResource;
            public readonly Color Accent;
            public readonly Action Action;

            public HubTileSpec(string title, string subtitle, string iconResource, Color accent, Action action)
            {
                Title = title;
                Subtitle = subtitle;
                IconResource = iconResource;
                Accent = accent;
                Action = action;
            }
        }

        private sealed class HubTileState
        {
            public ModernPanel Tile;
            public PictureBox Icon;
            public Label Title;
            public Label Subtitle;
            public Color Accent;
        }

        private List<Control> _hubTiles;
        private List<HubTileState> _hubTileStates;
        private Control _hubLogo;
        private ModernPanel _hubConnectionPill;
        private Label _hubConnectionDot;
        private Label _hubConnectionText;
        private HubUpdateCallout _hubUpdateCallout;
        private LunaConnectionState _currentConnectionState;
        private bool _hubLayoutRunning;
        private ModernPanel _hubHoveredTile;

        private void BuildHubPage()
        {
            _hubPage.AutoScroll = true;
            _hubPage.BackColor = AppBackground;
            _hubTiles = new List<Control>();
            _hubTileStates = new List<HubTileState>();

            _hubLogo = new HubHeroBanner();
            _hubLogo.Size = new Size(1180, 178);
            _hubPage.Controls.Add(_hubLogo);

            _hubWelcomeTitle = null;

            _hubConnectionPill = new ModernPanel();
            _hubConnectionPill.Size = new Size(184, 36);
            _hubConnectionPill.CornerRadius = 17;
            _hubConnectionPill.BorderWidth = 1.15F;
            _hubConnectionPill.UseGradientFill = false;
            _hubConnectionPill.Tag = "HubConnectionPill";
            _hubPage.Controls.Add(_hubConnectionPill);

            _hubConnectionDot = new Label();
            _hubConnectionDot.Text = "\u2715";
            _hubConnectionDot.Font = new Font("Segoe UI Symbol", 10.5F, FontStyle.Bold);
            _hubConnectionDot.Location = new Point(12, 6);
            _hubConnectionDot.Size = new Size(20, 24);
            _hubConnectionDot.BackColor = Color.Transparent;
            _hubConnectionDot.TextAlign = ContentAlignment.MiddleCenter;
            _hubConnectionDot.Tag = "HubConnectionDot";
            _hubConnectionPill.Controls.Add(_hubConnectionDot);

            _hubConnectionText = new Label();
            _hubConnectionText.Font = new Font("Segoe UI Semibold", 9.25F);
            _hubConnectionText.Location = new Point(38, 6);
            _hubConnectionText.Size = new Size(126, 24);
            _hubConnectionText.BackColor = Color.Transparent;
            _hubConnectionText.TextAlign = ContentAlignment.MiddleCenter;
            _hubConnectionText.Tag = "HubText";
            _hubConnectionPill.Controls.Add(_hubConnectionText);
            UpdateHubConnectionState(LunaConnectionState.Disconnected);

            var tiles = new[]
            {
                new HubTileSpec("Features", "Configure runtime tools, rewards, and gameplay options.", "features (1).png", Color.FromArgb(59, 130, 246), delegate { ShowFeaturesPage(); }),
                new HubTileSpec("Autoshow", "Manage garage entries and vehicle unlocks.", "Autoshow.png", Color.FromArgb(16, 185, 129), delegate { ShowMainPage(); }),
                new HubTileSpec("Teleport", "Save locations and replay custom routes.", "teleport (1).png", Color.FromArgb(34, 197, 94), delegate { ShowTeleportPage(); }),
                new HubTileSpec("Photo Mode", "Adjust camera and environment settings.", "photo-mode.png", Color.FromArgb(14, 165, 233), delegate { ShowPhotoModePage(); }),
                new HubTileSpec("Role Play", "Run role-play utilities and in-game overlays.", "role-play.png", Color.FromArgb(168, 85, 247), delegate { ShowRolePlayPage(); }),
                new HubTileSpec("Driving Editor", "Control live grip, motion, and handling.", "car-maintenance.png", Color.FromArgb(45, 212, 191), delegate { ShowDrivingPage(); }),
                new HubTileSpec("Driving Tuning", "Edit live tires, gearing, and suspension.", "service_846355.png", Color.FromArgb(239, 68, 68), delegate { ShowTuningPage(); }),
                new HubTileSpec("Database Tuning", "Edit vehicle parts and database values.", "self-driving.png", Color.FromArgb(245, 158, 11), delegate { ShowDatabaseTuningPage(); }),
                new HubTileSpec("Database Tools", "Repair saves and edit database records.", "database (1).png", Color.FromArgb(249, 115, 22), delegate { ShowProfilePageWithDatabaseWarning(); }),
                new HubTileSpec("Console", "Review logs and diagnostic output.", "consoel.png", Color.FromArgb(161, 161, 170), delegate { ShowConsolePage(); }),
                new HubTileSpec("Tutorial", "Read setup instructions and usage guides.", "Tutorial 2.png", Color.FromArgb(99, 102, 241), delegate { ShowTutorialPage(); }),
                new HubTileSpec("Feedback", "Report issues and suggest improvements.", "bug.png", Color.FromArgb(244, 63, 94), delegate { ShowFeedbackBugPage(); }),
                new HubTileSpec("Settings", "Configure Luna preferences and appearance.", "setting.png", Color.FromArgb(148, 163, 184), delegate { ShowSettingsPage(); }),
                new HubTileSpec("Donate", "Support Luna's continued development.", "donate.png", Color.FromArgb(236, 72, 153), delegate { ShowDonatePage(); }),
                new HubTileSpec("Credits", "View contributors and acknowledgements.", "Credit.png", Color.FromArgb(251, 191, 36), delegate { ShowCreditsPage(); }),
                new HubTileSpec("Connect", "Attach Luna to the active game process.", "Connection.png", Color.FromArgb(6, 182, 212), delegate { ShowConnectPage(); })
            };

            foreach (var spec in tiles)
                _hubTiles.Add(CreateHubTile(spec));

            _hubUpdateCallout = new HubUpdateCallout(OpenLunaUpdateUrl);
            _hubUpdateCallout.Visible = _lunaUpdateInfo != null;
            _hubUpdateCallout.SetUpdate(_lunaUpdateInfo);
            _hubPage.Controls.Add(_hubUpdateCallout);

            UpdateHubTileAvailability(_currentConnectionState != LunaConnectionState.Connecting);
            _hubPage.Resize += delegate { LayoutHub(); };
            LayoutHub();
        }

        private Control CreateHubTile(HubTileSpec spec)
        {
            var tile = new ModernPanel();
            tile.Size = new Size(220, 150);
            tile.FillColor = Surface;
            tile.BorderColor = Border;
            tile.CornerRadius = 12;
            tile.BorderWidth = 1.25F;
            tile.UseGradientFill = true;
            tile.Cursor = Cursors.Hand;
            _hubPage.Controls.Add(tile);

            var chip = new PictureBox();
            chip.Size = new Size(52, 52);
            chip.Image = LoadLocalAssetIconImage(spec.IconResource, spec.Accent);
            chip.SizeMode = PictureBoxSizeMode.Zoom;
            chip.Tag = "HubIcon";
            chip.BackColor = Color.Transparent;
            tile.Controls.Add(chip);

            var title = new Label();
            title.Text = spec.Title;
            title.Font = new Font("Segoe UI Semibold", 10.5F);
            title.ForeColor = TextPrimary;
            title.BackColor = Color.Transparent;
            title.Tag = "HubText";
            title.TextAlign = ContentAlignment.MiddleCenter;
            tile.Controls.Add(title);

            var sub = new Label();
            sub.Text = spec.Subtitle;
            sub.UseMnemonic = false;
            sub.Font = new Font("Segoe UI", 8.25F);
            sub.ForeColor = TextMuted;
            sub.BackColor = Color.Transparent;
            sub.Tag = "HubText";
            sub.TextAlign = ContentAlignment.TopCenter;
            tile.Controls.Add(sub);
            _hubTileStates.Add(new HubTileState
            {
                Tile = tile,
                Icon = chip,
                Title = title,
                Subtitle = sub,
                Accent = spec.Accent
            });

            Action relayout = delegate
            {
                chip.Left = (tile.Width - chip.Width) / 2;
                chip.Top = 22;
                title.SetBounds(8, 84, Math.Max(20, tile.Width - 16), 22);
                sub.SetBounds(12, 108, Math.Max(20, tile.Width - 24), 36);
            };
            tile.Resize += delegate { relayout(); };
            relayout();

            EventHandler onEnter = delegate
            {
                if (!tile.Enabled)
                    return;
                if (_hubHoveredTile != null && _hubHoveredTile != tile && !_hubHoveredTile.IsDisposed)
                {
                    _hubHoveredTile.BorderColor = Border;
                    _hubHoveredTile.FillColor = Surface;
                    _hubHoveredTile.Invalidate();
                }
                _hubHoveredTile = tile;
                tile.BorderColor = spec.Accent;
                tile.FillColor = Blend(Surface, spec.Accent, 0.075F);
                tile.Invalidate();
            };
            EventHandler onLeave = delegate
            {
                if (tile.IsDisposed || !tile.IsHandleCreated)
                    return;

                try
                {
                    tile.BeginInvoke((Action)delegate
                    {
                        if (tile.IsDisposed)
                            return;

                        var bounds = tile.RectangleToScreen(tile.ClientRectangle);
                        if (bounds.Contains(Cursor.Position))
                            return;

                    if (_hubHoveredTile == tile)
                        _hubHoveredTile = null;
                    tile.BorderColor = Border;
                    tile.FillColor = Surface;
                    tile.Invalidate();
                    });
                }
                catch (InvalidOperationException)
                {
                }
            };
            EventHandler onClick = delegate
            {
                if (!tile.Enabled)
                    return;
                if (_hubHoveredTile != null && !_hubHoveredTile.IsDisposed)
                {
                    _hubHoveredTile.BorderColor = Border;
                    _hubHoveredTile.FillColor = Surface;
                    _hubHoveredTile.Invalidate();
                }
                _hubHoveredTile = null;
                spec.Action();
            };

            var clickable = new Control[] { tile, chip, title, sub };
            foreach (var c in clickable)
            {
                c.Cursor = Cursors.Hand;
                c.MouseEnter += onEnter;
                c.MouseLeave += onLeave;
                c.Click += onClick;
            }
            return tile;
        }

        private void LayoutHub()
        {
            if (_hubPage == null || _hubPage.IsDisposed || _hubTiles == null || _hubLayoutRunning)
                return;
            var avail = _hubPage.ClientSize.Width;
            if (avail < 60)
                return;

            _hubLayoutRunning = true;
            try
            {
                const int margin = 40;
                const int gap = 16;
                const int tileH = 150;

                var cols = 4;
                if (avail < 1180) cols = 3;
                if (avail < 860) cols = 2;
                if (avail < 540) cols = 1;

                var usableW = Math.Max(240, avail - (2 * margin));
                var tileW = Math.Min(340, (usableW - ((cols - 1) * gap)) / cols);
                var gridW = (cols * tileW) + ((cols - 1) * gap);
                var startX = Math.Max(margin, (avail - gridW) / 2);

                var heroW = Math.Min(gridW, Math.Max(320, avail - (2 * margin)));
                var heroH = Math.Max(152, Math.Min(210, (int)Math.Round(heroW * 0.145F)));
                if (avail < 720)
                    heroH = Math.Max(128, Math.Min(166, (int)Math.Round(heroW * 0.19F)));
                if (_hubLogo != null)
                    _hubLogo.SetBounds((avail - heroW) / 2, 72, heroW, heroH);
                if (_hubConnectionPill != null)
                    _hubConnectionPill.SetBounds(Math.Max(18, (avail - _hubConnectionPill.Width) / 2), 16, _hubConnectionPill.Width, _hubConnectionPill.Height);

                var startY = (_hubLogo != null ? _hubLogo.Bottom : 238) + 34;
                for (var i = 0; i < _hubTiles.Count; i++)
                {
                    var col = i % cols;
                    var row = i / cols;
                    var x = startX + (col * (tileW + gap));
                    var y = startY + (row * (tileH + gap));
                    _hubTiles[i].SetBounds(x, y, tileW, tileH);
                }

                var rowCount = (_hubTiles.Count + cols - 1) / cols;
                var tileBottom = startY + (rowCount * (tileH + gap)) - gap;
                var contentBottom = tileBottom;
                if (_hubUpdateCallout != null && !_hubUpdateCallout.IsDisposed)
                {
                    var calloutW = Math.Min(Math.Max(520, gridW), Math.Max(520, avail - (2 * margin)));
                    var calloutH = 96;
                    var calloutX = Math.Max(margin, (avail - calloutW) / 2);
                    var calloutY = tileBottom + 44;
                    _hubUpdateCallout.SetBounds(calloutX, calloutY, calloutW, calloutH);
                    if (_hubUpdateCallout.Visible)
                        contentBottom = _hubUpdateCallout.Bottom;
                }

                var totalH = contentBottom + 34;
                _hubPage.AutoScrollMinSize = new Size(0, totalH);
            }
            finally
            {
                _hubLayoutRunning = false;
            }
        }

        private void ShowHubPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowHubPage);
                return;
            }
            HidePages();
            _hubPage.Visible = true;
            _hubPage.BringToFront();
            var state = _currentConnectionState == LunaConnectionState.Connecting
                ? LunaConnectionState.Connecting
                : _database != null && _database.IsAlive
                    ? LunaConnectionState.Connected
                    : LunaConnectionState.Disconnected;
            UpdateHubConnectionState(state);
            _hubHoveredTile = null;
            if (_hubTiles != null)
            {
                foreach (var control in _hubTiles)
                {
                    var tile = control as ModernPanel;
                    if (tile != null)
                    {
                        tile.BorderColor = Border;
                        tile.FillColor = Surface;
                    }
                }
            }
            LayoutHub();
            SetStatus("Hub");
            UpdateNavigationState(_navHome);
        }

        private void UpdateHubConnectionState(bool attached)
        {
            UpdateHubConnectionState(attached
                ? LunaConnectionState.Connected
                : LunaConnectionState.Disconnected);
        }

        private void UpdateHubConnectionState(LunaConnectionState state)
        {
            if (_hubConnectionPill == null || _hubConnectionPill.IsDisposed)
                return;
            if (InvokeRequired)
            {
                BeginInvoke((Action)(delegate { UpdateHubConnectionState(state); }));
                return;
            }

            _currentConnectionState = state;
            var color = state == LunaConnectionState.Connected
                ? AccentGreen
                : state == LunaConnectionState.Connecting
                    ? Color.FromArgb(234, 179, 8)
                    : AccentRed;
            _hubConnectionPill.FillColor = Blend(Surface, color, 0.08F);
            _hubConnectionPill.BorderColor = Blend(Border, color, 0.58F);
            _hubConnectionDot.ForeColor = color;
            _hubConnectionDot.BackColor = Color.Transparent;
            _hubConnectionDot.Text = state == LunaConnectionState.Connected
                ? "\u2713"
                : state == LunaConnectionState.Connecting
                    ? "\u24D8"
                    : "\u2715";
            _hubConnectionText.Text = state == LunaConnectionState.Connected
                ? TranslateUi("Connected")
                : state == LunaConnectionState.Connecting
                    ? TranslateUi("Connecting")
                    : TranslateUi("Disconnected");
            _hubConnectionText.ForeColor = TextPrimary;
            _hubConnectionText.BackColor = Color.Transparent;

            var textWidth = TextRenderer.MeasureText(
                _hubConnectionText.Text,
                _hubConnectionText.Font,
                new Size(280, 24),
                TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width;
            const int dotWidth = 20;
            const int gap = 7;
            var pillWidth = Math.Max(184, textWidth + dotWidth + gap + 30);
            _hubConnectionPill.Width = pillWidth;
            var groupWidth = dotWidth + gap + textWidth;
            var startX = Math.Max(10, (pillWidth - groupWidth) / 2);
            var iconY = Math.Max(0, (_hubConnectionPill.Height - 24) / 2);
            var textY = Math.Max(0, (_hubConnectionPill.Height - 24) / 2);
            _hubConnectionDot.SetBounds(startX, iconY, dotWidth, 24);
            _hubConnectionText.SetBounds(startX + dotWidth + gap, textY, textWidth, 24);
            if (_hubConnectionPill.Parent != null)
                _hubConnectionPill.Left = Math.Max(18, (_hubConnectionPill.Parent.ClientSize.Width - pillWidth) / 2);
            _hubConnectionPill.Invalidate(true);
            UpdateHubTileAvailability(state != LunaConnectionState.Connecting);
        }

        private void UpdateHubTileAvailability(bool enabled)
        {
            if (_hubTileStates == null)
                return;

            if (!enabled)
                _hubHoveredTile = null;

            var mutedFill = Blend(Surface, TextMuted, 0.08F);
            var mutedBorder = Blend(Border, TextMuted, 0.28F);
            foreach (var state in _hubTileStates)
            {
                if (state == null || state.Tile == null || state.Tile.IsDisposed)
                    continue;

                state.Tile.Enabled = enabled;
                state.Tile.Cursor = enabled ? Cursors.Hand : Cursors.Default;
                state.Tile.FillColor = enabled ? Surface : mutedFill;
                state.Tile.BorderColor = enabled ? Border : mutedBorder;

                if (state.Icon != null && !state.Icon.IsDisposed)
                {
                    state.Icon.Enabled = enabled;
                    state.Icon.Cursor = enabled ? Cursors.Hand : Cursors.Default;
                }
                if (state.Title != null && !state.Title.IsDisposed)
                {
                    state.Title.ForeColor = enabled ? TextPrimary : TextMuted;
                    state.Title.Cursor = enabled ? Cursors.Hand : Cursors.Default;
                }
                if (state.Subtitle != null && !state.Subtitle.IsDisposed)
                {
                    state.Subtitle.ForeColor = enabled ? TextMuted : Blend(TextMuted, AppBackground, 0.36F);
                    state.Subtitle.Cursor = enabled ? Cursors.Hand : Cursors.Default;
                }
                state.Tile.Invalidate(true);
            }
        }

        private void ToggleAppTheme()
        {
            ApplyThemePreset(!_lightMode, true);
        }

        private void ApplyThemePreset(bool light)
        {
            ApplyThemePreset(light, true);
        }

        private void ApplyThemePreset(bool light, bool persist)
        {
            _lightMode = light;
            if (light)
            {
                AppBackground = Color.FromArgb(239, 243, 248);
                SidebarBackground = Color.FromArgb(248, 250, 252);
                Surface = Color.FromArgb(255, 255, 255);
                SurfaceAlt = Color.FromArgb(244, 247, 251);
                Border = Color.FromArgb(168, 178, 195);
                TextPrimary = Color.FromArgb(17, 24, 39);
                TextMuted = Color.FromArgb(75, 85, 99);
            }
            else
            {
                AppBackground = Color.FromArgb(9, 9, 11);
                SidebarBackground = Color.FromArgb(15, 15, 17);
                Surface = Color.FromArgb(24, 24, 27);
                SurfaceAlt = Color.FromArgb(35, 35, 39);
                Border = Color.FromArgb(24, 24, 27);
                TextPrimary = Color.White;
                TextMuted = Color.FromArgb(161, 161, 170);
            }
            AccentBlueSoft = Blend(AccentBlue, light ? Color.White : Color.Black, 0.55F);

            ApplyThemeToExistingControls();
            UpdateHubConnectionState(_currentConnectionState);
            ApplySystemTitleBarColor();
            if (_themeModeButton != null)
                _themeModeButton.SetLightMode(light, persist);
            SetStatus(light ? "Light theme" : "Dark theme");
            if (persist)
                SaveAppSettings();
        }
    }
}
