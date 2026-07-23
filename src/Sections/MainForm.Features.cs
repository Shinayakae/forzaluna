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
        private void BuildFeaturesPage()
        {
            _featuresPage.AutoScroll = true;
            AddPageHeader(_featuresPage, "Features", "Type a number, turn the dot green, then press Apply. Turning the dot red stops that row right away.");

            var status = new Label();
            status.Text = "Ready";
            status.Font = new Font("Segoe UI Semibold", 9F);
            status.ForeColor = AccentBlue;
            status.BackColor = AppBackground;
            status.Location = new Point(806, 12);
            status.Size = new Size(190, 24);
            status.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            status.TextAlign = ContentAlignment.MiddleRight;
            _featuresPage.Controls.Add(status);
            _featuresStatus = status;

            const int featureColumnWidth = 720;
            const int featureColumnGap = 32;
            const int featureColumnTop = 72;
            const int featureRowGap = 28;
            var featureContentWidth = (featureColumnWidth * 2) + featureColumnGap;
            ResizeFeaturePageHeader(featureContentWidth);
            status.Location = new Point(featureContentWidth - status.Width, status.Top);

            var nextColumn = 0;
            var columnNextY = new[] { featureColumnTop, featureColumnTop };
            var curY = featureColumnTop;

            Action<string, Action<Control, int>[]> addCategory = delegate(string sectionTitle, Action<Control, int>[] rows)
            {
                var column = nextColumn;
                var columnX = column * (featureColumnWidth + featureColumnGap);
                var sectionY = columnNextY[column];

                var header = new Label();
                header.Text = sectionTitle.ToUpperInvariant();
                header.UseMnemonic = false;
                header.Font = new Font("Segoe UI Semibold", 9F);
                header.ForeColor = TextMuted;
                header.BackColor = AppBackground;
                header.Location = new Point(columnX + 6, sectionY);
                header.Size = new Size(featureColumnWidth - 12, 18);
                _featuresPage.Controls.Add(header);

                var cardHeight = (rows.Length * 34) + 20;
                var card = MakeCard(_featuresPage, columnX, sectionY + 26, featureColumnWidth, cardHeight, string.Empty, string.Empty);
                card.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                card.Tag = "FeatureCategoryCard";
                var ry = 14;
                for (var i = 0; i < rows.Length; i++)
                {
                    rows[i](card, ry);
                    ry += 34;
                }

                columnNextY[column] = card.Bottom + featureRowGap;
                nextColumn = nextColumn == 0 ? 1 : 0;
                curY = Math.Max(columnNextY[0], columnNextY[1]);
            };

            addCategory("Economy & Progression", new Action<Control, int>[]
            {
                delegate(Control c, int y) { AddProfileBoostPackRow(c, 18, y); },
                delegate(Control c, int y) { AddProfileValueRow(c, "Credits", string.Empty, 18, y, RuntimeProfileFeature.Credits, "Type the credits you want. Green turns it on. Red turns it off."); },
                delegate(Control c, int y) { AddProfileValueRow(c, "Both Wheelspins", string.Empty, 18, y, RuntimeProfileFeature.Wheelspins, "Type the wheelspins you want. This controls normal and super wheelspin values together."); },
                delegate(Control c, int y) { AddProfileValueRow(c, "Skill Points", string.Empty, 18, y, RuntimeProfileFeature.SkillPoints, "Type the skill points you want. Green turns it on. Red turns it off."); },
                delegate(Control c, int y) { AddProfileValueRow(c, "XP Gain", "50/1000/1/1/10000", 18, y, RuntimeProfileFeature.XpGain, "Auto-fires FH6's skill-point reward. Enter XP as a 200 XP event number, choose how many times it fires, turn it green, press Apply, then spend ONE skill point in the perk menu once to arm it."); }
            });

            addCategory("Manipulators", new Action<Control, int>[]
            {
                delegate(Control c, int y) { AddProfileValueRow(c, "Drift Score Multiplier", string.Empty, 18, y, RuntimeProfileFeature.DriftScoreMultiplier, "Type a drift score boost like 5 or 10. Green dot means the live hook is on. Press Apply."); },
                delegate(Control c, int y) { AddProfileValueRow(c, "Speed Zone Multiplier", string.Empty, 18, y, RuntimeProfileFeature.SpeedZoneSpeed, "Type 2, 5, or 10. Green makes speed zones count higher. Keep Luna open."); },
                delegate(Control c, int y) { AddProfileValueRow(c, "Mission Timer", "0", 18, y, RuntimeProfileFeature.MissionTimerScale, "Timer speed for missions. 0 freezes, 1 is normal, 0.5 is slower, 2 is faster."); },
                delegate(Control c, int y) { AddProfileValueRow(c, "Race Timer", "0", 18, y, RuntimeProfileFeature.RaceTimerScale, "Timer speed for races. 0 freezes, 1 is normal, 0.5 is slower, 2 is faster."); }
            });

            addCategory("Physics & Handling", new Action<Control, int>[]
            {
                delegate(Control c, int y) { AddProfileValueRow(c, "Puddle Correction", "1", 18, y, RuntimeProfileFeature.RoadWetnessPuddleControl, "Controls how strongly puddles affect the car. 1 is normal, 0 removes the puddle effect, and higher values increase it."); },
                delegate(Control c, int y) { AddProfileValueRow(c, "Super Grip", "100", 18, y, RuntimeProfileFeature.SuperGrip, "Makes the car grip the road and corner flat. The number is the overall strength (100 = normal grip; higher = grippier). Green turns it on, red restores normal."); },
                delegate(Control c, int y) { AddProfileValueRow(c, "Top Speed", "300", 18, y, RuntimeProfileFeature.TopSpeedCap, "Hard caps the car's horizontal top speed in km/h. Green enables the cap; red disables it."); },
                delegate(Control c, int y) { AddAccelerationSafeToggleRow(c, 18, y); },
                delegate(Control c, int y) { AddProfileValueRow(c, "Acceleration Ramp", "60", 18, y, RuntimeProfileFeature.AccelerationRamp, "Adds extra acceleration only while W or Up is actively held. Braking pauses the boost but keeps the feature armed, so throttle resumes it without needing a full stop. Type the boost strength, then turn it green or use the hotkey."); },
                delegate(Control c, int y) { AddProfilePercentRow(c, "Gravity", 100, GravityMinPercent, GravityMaxPercent, 18, y, RuntimeProfileFeature.Gravity, "Set gravity percent. 100% is normal, 0% is no gravity, negative gives lift, and higher positive values glue the car down harder."); },
                delegate(Control c, int y) { AddSuperBrakeKeybindRow(c, 18, y); },
                delegate(Control c, int y) { AddAdaptiveBrakeRow(c, 18, y); },
                delegate(Control c, int y) { AddProfileValueRow(c, "Cruise Control", "120", 18, y, RuntimeProfileFeature.CruiseControl, "Holds your car at the set speed while YOU steer. Type the target speed in km/h, then turn it green or use the hotkey. Once the car is moving, it will coast and hold that speed until turned off."); }
            });

            var movementRows = new List<Action<Control, int>>
            {
                delegate(Control c, int y) { AddNoClipKeybindRow(c, 18, y); },
                delegate(Control c, int y) { AddSuperStrengthKeybindRow(c, 18, y); },
                delegate(Control c, int y) { AddJumpKeybindRow(c, 18, y); },
                delegate(Control c, int y) { AddWallJumperKeybindRow(c, 18, y); },
                delegate(Control c, int y) { AddChargeJumpKeybindRow(c, 18, y); },
                delegate(Control c, int y) { AddFlyRow(c, 18, y); },
                delegate(Control c, int y) { AddRewindKeybindRow(c, 18, y); },
                delegate(Control c, int y) { AddGrapplingHookKeybindRow(c, 18, y); },
                delegate(Control c, int y) { AddPhaseDashKeybindRow(c, 18, y); },
                delegate(Control c, int y) { AddBoostKeybindRow(c, 18, y); },
                delegate(Control c, int y) { AddTeleportWaypointKeybindRow(c, 18, y, false); },
                delegate(Control c, int y) { AddAutoRaceDriveKeybindRow(c, 18, y, false); },
                delegate(Control c, int y) { AddCheckpointRecoveryKeybindRow(c, 18, y, false); },
                delegate(Control c, int y) { AddTeleportCheckpointKeybindRow(c, 18, y, false); }
            };
            movementRows.Add(delegate(Control c, int y) { AddDriftModeKeybindRow(c, 18, y); });
            addCategory("Movement & Abilities", movementRows.ToArray());

            addCategory("Environment & Rules", new Action<Control, int>[]
            {
                delegate(Control c, int y) { AddProfileValueRow(c, "Time of Day", "12", 18, y, RuntimeProfileFeature.TimeOfDay, "Type the hour you want (0-24). 0 is midnight, 12 is noon, 18 is evening. Green holds that time of day; red lets the in-game clock run again. Use it in free-roam."); },
                delegate(Control c, int y) { AddFovSliderRow(c, 18, y); },
                delegate(Control c, int y) { AddProfileToggleRow(c, "Disable Camera Collision", 18, y, RuntimeProfileFeature.DisableCameraCollision, "Green disables FH6's camera world-collision flag when the current camera config is available. Red restores the captured flag."); },
                delegate(Control c, int y) { AddProfileToggleRow(c, "No Build Limit", 18, y, RuntimeProfileFeature.NoBuildLimit, "Green removes the live build cap check. Red restores normal build limits."); },
                delegate(Control c, int y) { AddProfileToggleRow(c, "Freeze AI", 18, y, RuntimeProfileFeature.FreezeAI, "Green freezes race AI movement. Red restores normal AI movement. For free-roam civilian traffic, use Freeze AI Traffic in Database Tools."); },
                delegate(Control c, int y) { AddProfileToggleRow(c, "No Skill Break", 18, y, RuntimeProfileFeature.NoSkillBreak, "Green keeps your skill chain alive. Red turns it off right away."); }
            });

            var configColumn = nextColumn;
            var configX = configColumn * (featureColumnWidth + featureColumnGap);
            curY = columnNextY[configColumn];

            var cfgHeader = new Label();
            cfgHeader.Tag = "FeatureConfigHeader";
            cfgHeader.Text = "CONFIGURATION";
            cfgHeader.UseMnemonic = false;
            cfgHeader.Font = new Font("Segoe UI Semibold", 9F);
            cfgHeader.ForeColor = TextMuted;
            cfgHeader.BackColor = AppBackground;
            cfgHeader.Location = new Point(configX + 6, curY);
            cfgHeader.Size = new Size(featureColumnWidth - 12, 18);
            _featuresPage.Controls.Add(cfgHeader);
            curY += 26;

            var configCard = MakeCard(_featuresPage, configX, curY, featureColumnWidth, 72, string.Empty, string.Empty);
            configCard.Tag = "FeatureConfigCard";
            configCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var saveConfig = MakeButton("Save Config", 18, 20, 132, 30);
            MakeAccentButton(saveConfig, AccentBlue);
            saveConfig.Click += delegate { SaveFeatureConfig(); };
            configCard.Controls.Add(saveConfig);
            SetTranslatedToolTip(saveConfig, "Save every feature's on/off state and value to a config file you choose.");

            var loadConfig = MakeButton("Load Config", 158, 20, 132, 30);
            MakeAccentButton(loadConfig, AccentPurple);
            loadConfig.Click += delegate { LoadFeatureConfig(); };
            configCard.Controls.Add(loadConfig);
            SetTranslatedToolTip(loadConfig, "Load a saved config file. Configs never load on their own; this restores the toggles and values and applies them when Luna is attached.");

            var autosaveLabel = MakeLabel("Autosave", 320, 25);
            configCard.Controls.Add(autosaveLabel);
            SetTranslatedToolTip(autosaveLabel, "When green, Luna keeps a config file up to date automatically as you change features. It still never loads automatically.");

            var autosaveToggle = new StatusDotToggle();
            autosaveToggle.Location = new Point(398, 23);
            autosaveToggle.Size = new Size(40, 22);
            autosaveToggle.Checked = _featuresAutosaveEnabled;
            configCard.Controls.Add(autosaveToggle);
            _featuresAutosaveToggle = autosaveToggle;
            SetTranslatedToolTip(autosaveToggle, "Autosave the current feature configuration. Loading is always manual.");
            autosaveToggle.CheckedChanged += delegate { SetFeaturesAutosaveEnabled(autosaveToggle.Checked); };

            var configHint = MakeBodyLabel("Live values fill in automatically when Luna attaches.", 460, 26, featureColumnWidth - 480, 22);
            configHint.BackColor = Surface;
            configHint.ForeColor = TextMuted;
            configCard.Controls.Add(configHint);
            columnNextY[configColumn] = configCard.Bottom + 24;
            curY = Math.Max(columnNextY[0], columnNextY[1]);

            EnsureFeaturesAutosaveTimer();

            NormalizeFeatureRowControls();
            _featuresPage.AutoScrollMinSize = new Size(featureContentWidth, curY + 24);
        }

        private void ResizeFeaturePageHeader(int headerWidth)
        {
            if (_featuresPage == null)
                return;

            var subtitlePanel = _featuresPage.Controls
                .OfType<ModernPanel>()
                .FirstOrDefault(panel => object.Equals(panel.Tag, "PageHeaderSubtitlePanel"));
            if (subtitlePanel != null)
            {
                subtitlePanel.Width = headerWidth;
                var subtitleLabel = subtitlePanel.Controls
                    .OfType<Label>()
                    .FirstOrDefault(label => object.Equals(label.Tag, "PageHeaderSubtitle"));
                if (subtitleLabel != null)
                {
                    subtitleLabel.Width = Math.Max(80, headerWidth - 70);
                    subtitleLabel.TextAlign = ContentAlignment.MiddleLeft;
                }
            }

            var icon = _featuresPage.Controls
                .OfType<PictureBox>()
                .FirstOrDefault(control => object.Equals(control.Tag, "PageHeaderIcon"));
            var title = _featuresPage.Controls
                .OfType<Label>()
                .FirstOrDefault(control => object.Equals(control.Tag, "PageHeaderTitle"));
            if (icon == null || title == null)
                return;

            const int titleGap = 12;
            var groupWidth = icon.Width + titleGap + title.Width;
            var left = Math.Max(0, (headerWidth - groupWidth) / 2);
            icon.Left = left;
            title.Left = left + icon.Width + titleGap;
        }

        private void NormalizeFeatureRowControls()
        {
            foreach (var card in EnumerateFeatureModernPanels(_featuresPage))
            {
                var applyButtons = card.Controls
                    .OfType<ModernButton>()
                    .Where(button => string.Equals(button.Text, "Apply", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (applyButtons.Count == 0)
                    continue;

                var compact = object.Equals(card.Tag, "FeatureCategoryCard") || card.Width <= 760;
                var labelWidth = compact ? 216 : 190;
                var valueLeft = compact ? 250 : 438;
                var toggleLeft = compact ? 404 : 584;
                var toggleWidth = 40;
                var toggleHeight = 22;
                var applyLeft = compact ? 458 : 638;
                var applyWidth = 96;
                var applyHeight = 30;
                var hotkeyLeft = compact ? 560 : 748;
                var hotkeyWidth = compact ? 58 : 64;
                var hotkeyArmLeft = compact ? 626 : 822;
                var hotkeyArmWidth = compact ? 86 : 86;
                var valueRightLimit = toggleLeft - 10;

                foreach (var apply in applyButtons)
                {
                    apply.SetBounds(applyLeft, apply.Top, applyWidth, applyHeight);
                    apply.Tag = "FeatureApply";
                    apply.TracksTheme = true;
                    apply.FillColor = SurfaceAlt;
                    apply.HoverColor = Blend(SurfaceAlt, TextPrimary, 0.06F);
                    apply.PressedColor = Blend(SurfaceAlt, TextPrimary, 0.12F);
                    apply.BorderColor = Border;
                    apply.ForeColor = TextPrimary;

                    var rowTop = apply.Top;
                    var toggle = card.Controls
                        .OfType<StatusDotToggle>()
                        .Where(item => Math.Abs(item.Top - rowTop) <= 10)
                        .OrderBy(item => Math.Abs(item.Top - rowTop))
                        .FirstOrDefault();
                    if (toggle != null)
                        toggle.SetBounds(toggleLeft, rowTop + ((applyHeight - toggleHeight) / 2), toggleWidth, toggleHeight);

                    foreach (var label in card.Controls.OfType<Label>())
                    {
                        if (Math.Abs(label.Top - (rowTop + 6)) > 10 || label.Left >= valueLeft)
                            continue;

                        label.Width = labelWidth;
                        label.AutoSize = false;
                        label.AutoEllipsis = true;
                    }

                    var rowButtons = card.Controls
                        .OfType<Button>()
                        .Where(button => button != apply && Math.Abs(button.Top - rowTop) <= 4)
                        .OrderBy(button => button.Left)
                        .ToList();

                    var armButton = rowButtons
                        .FirstOrDefault(button =>
                            _runtimeFeatureHotkeyArmButtons.ContainsValue(button) ||
                            (button.Text ?? string.Empty).StartsWith("Key ", StringComparison.OrdinalIgnoreCase));
                    var hotkeyButton = rowButtons
                        .FirstOrDefault(button => button != armButton && _runtimeFeatureHotkeyButtons.ContainsValue(button));
                    if (hotkeyButton == null && armButton != null)
                    {
                        hotkeyButton = rowButtons
                            .Where(button => button != armButton && button.Left > apply.Left)
                            .OrderBy(button => button.Left)
                            .FirstOrDefault();
                    }

                    if (hotkeyButton != null)
                        hotkeyButton.SetBounds(hotkeyLeft, rowTop, hotkeyWidth, applyHeight);
                    if (armButton != null)
                        armButton.SetBounds(hotkeyArmLeft, rowTop, hotkeyArmWidth, applyHeight);

                    var valueButtons = rowButtons
                        .Where(button => button != hotkeyButton && button != armButton)
                        .OrderBy(button => button.Left)
                        .ToList();

                    var valueX = valueLeft;
                    for (var i = 0; i < valueButtons.Count; i++)
                    {
                        var button = valueButtons[i];
                        var remaining = Math.Max(40, valueRightLimit - valueX);
                        var preferredWidth = valueButtons.Count > 1 ? Math.Min(button.Width, 68) : Math.Min(button.Width, 116);
                        var width = Math.Max(40, Math.Min(preferredWidth, remaining));
                        button.SetBounds(valueX, rowTop, width, applyHeight);
                        valueX += width + 8;
                    }
                }
            }
        }

        private static IEnumerable<ModernPanel> EnumerateFeatureModernPanels(Control root)
        {
            if (root == null)
                yield break;

            foreach (Control child in root.Controls)
            {
                var panel = child as ModernPanel;
                if (panel != null)
                    yield return panel;

                foreach (var nested in EnumerateFeatureModernPanels(child))
                    yield return nested;
            }
        }

        private void AddProfileValueRow(Control parent, string labelText, string defaultValue, int x, int y, RuntimeProfileFeature feature, string tooltip)
        {
            var isBeta = IsBetaRuntimeFeature(feature);
            var label = MakeLabel(labelText, x, y + 4);
            label.Width = isBeta ? 160 : 190;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var valueX = x + 420;
            var toggleX = x + 566;
            var applyX = x + 620;

            if (isBeta)
            {
                var badge = new BetaBadge();
                badge.Location = new Point(x + 164, y + 5);
                badge.Size = new Size(42, 18);
                parent.Controls.Add(badge);
                SetTranslatedToolTip(badge, "Beta feature: this is still in testing and may not work on every session.");
            }

            var value = new TextBox();
            value.Text = defaultValue ?? string.Empty;
            value.Visible = false;
            value.Location = new Point(valueX, y);
            value.Size = new Size(1, 1);
            parent.Controls.Add(value);
            _profileValueBoxes[feature] = value;

            var valueButtonText = feature == RuntimeProfileFeature.XpGain
                ? FormatXpGainConfigButtonText(value.Text)
                : TranslateDynamicUi(FormatRuntimeValueButtonText(value.Text));
            var valueButton = MakeButton(valueButtonText, valueX, y - 2, 116, 30);
            valueButton.Click += delegate
            {
                string selected;
                if (feature == RuntimeProfileFeature.XpGain)
                    selected = ShowXpGainConfigPrompt(value.Text);
                else if (feature == RuntimeProfileFeature.SuperGrip)
                    selected = ShowRuntimeValuePrompt(feature, labelText, value.Text, "Adjust the Super Grip strength. 100 is normal grip; higher numbers grip the road harder and keep the car flatter through corners. Lower eases it off.");
                else
                    selected = ShowRuntimeValuePrompt(feature, labelText, value.Text, tooltip);
                if (selected == null)
                    return;
                value.Text = selected;
            };
            value.TextChanged += delegate
            {
                valueButton.Text = feature == RuntimeProfileFeature.XpGain
                    ? FormatXpGainConfigButtonText(value.Text)
                    : TranslateDynamicUi(FormatRuntimeValueButtonText(value.Text));
            };
            parent.Controls.Add(valueButton);
            SetTranslatedToolTip(valueButton, tooltip + " Click to edit this value.");

            var toggle = new StatusDotToggle();
            toggle.Location = new Point(toggleX, y + 4);
            toggle.Size = new Size(40, 22);
            parent.Controls.Add(toggle);
            SetTranslatedToolTip(toggle, tooltip + " Green is on. Red turns it off right away.");
            if (feature == RuntimeProfileFeature.AccelerationRamp)
                _accelerationRampFeatureToggle = toggle;

            var suppressBetaPrompt = false;
            if (isBeta)
            {
                toggle.CheckedChanged += delegate
                {
                    if (suppressBetaPrompt || _loadingFeatureConfig || !toggle.Checked)
                        return;

                    suppressBetaPrompt = true;
                    try
                    {
                        if (!ShowBetaFeaturePrompt(feature, labelText))
                            toggle.Checked = false;
                    }
                    finally
                    {
                        suppressBetaPrompt = false;
                    }
                };
            }
            WireImmediateRuntimeOff(toggle, feature, labelText);

            var apply = MakeButton("Apply", applyX, y - 2, 96, 30);
            MakeAccentButton(apply, AccentBlue);
            apply.Click += delegate
            {
                var text = value.Text;
                var enabled = toggle.Checked;
                RunWorker(labelText, delegate { ApplyProfileRuntimeHook(feature, labelText, text, enabled); }, apply);
            };
            parent.Controls.Add(apply);
            SetTranslatedToolTip(apply, tooltip);
            AddRuntimeFeatureHotkeyControls(parent, x, y, feature, labelText, value, toggle);
        }

        private string ShowXpGainConfigPrompt(string currentValue)
        {
            if (InvokeRequired)
                return (string)Invoke(new Func<string, string>(ShowXpGainConfigPrompt), currentValue);

            int amount;
            int speedMs;
            int fireCount;
            int xpAmount;
            bool useSkillPointMode;
            bool whilePlaying;
            ParseXpGainConfig(currentValue, out amount, out speedMs, out whilePlaying, out fireCount, out xpAmount, out useSkillPointMode);

            using (var dialog = new Form())
            {
                dialog.Text = "XP Gain Config";
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.BackColor = AppBackground;
                dialog.ForeColor = TextPrimary;
                dialog.Font = new Font("Segoe UI", 9F);
                dialog.ClientSize = new Size(492, 540);

                var card = new ModernPanel();
                card.Location = new Point(14, 14);
                card.Size = new Size(464, 512);
                card.FillColor = Surface;
                card.BorderColor = Border;
                card.CornerRadius = 14;
                dialog.Controls.Add(card);

                var title = new Label();
                title.Text = "XP Gain Config";
                title.Font = new Font("Segoe UI Semibold", 13F);
                title.ForeColor = TextPrimary;
                title.BackColor = Surface;
                title.Location = new Point(18, 14);
                title.Size = new Size(428, 26);
                title.TextAlign = ContentAlignment.MiddleCenter;
                card.Controls.Add(title);

                var useSkillPointEntry = useSkillPointMode;
                var syncingAmountBoxes = false;

                var xpMode = MakeButton("XP Amount", 82, 54, 134, 30);
                card.Controls.Add(xpMode);
                var skillMode = MakeButton("Skill Points", 228, 54, 134, 30);
                card.Controls.Add(skillMode);

                var xpLabel = MakeBodyLabel("XP amount per fire", 22, 98, 220, 22);
                xpLabel.BackColor = Surface;
                card.Controls.Add(xpLabel);
                var xpBox = new TextBox();
                xpBox.Text = xpAmount.ToString(System.Globalization.CultureInfo.InvariantCulture);
                xpBox.Location = new Point(264, 94);
                xpBox.Size = new Size(160, 26);
                StyleTextBox(xpBox);
                card.Controls.Add(xpBox);

                var skillBox = new TextBox();
                skillBox.Text = amount.ToString(System.Globalization.CultureInfo.InvariantCulture);
                skillBox.Location = xpBox.Location;
                skillBox.Size = xpBox.Size;
                StyleTextBox(skillBox);
                skillBox.Visible = false;
                card.Controls.Add(skillBox);

                var skillSummary = MakeBodyLabel("", 22, 126, 402, 22);
                skillSummary.BackColor = Surface;
                skillSummary.ForeColor = AccentGreen;
                card.Controls.Add(skillSummary);

                var fireLabel = MakeBodyLabel("Times to fire (0 = continuous)", 22, 158, 230, 22);
                fireLabel.BackColor = Surface;
                card.Controls.Add(fireLabel);
                var fireBox = new TextBox();
                fireBox.Text = fireCount.ToString(System.Globalization.CultureInfo.InvariantCulture);
                fireBox.Location = new Point(264, 154);
                fireBox.Size = new Size(160, 26);
                StyleTextBox(fireBox);
                card.Controls.Add(fireBox);

                var speedLabel = MakeBodyLabel("Trigger speed (ms, lower = faster)", 22, 200, 230, 22);
                speedLabel.BackColor = Surface;
                card.Controls.Add(speedLabel);
                var speedBox = new TextBox();
                speedBox.Text = speedMs.ToString(System.Globalization.CultureInfo.InvariantCulture);
                speedBox.Location = new Point(264, 196);
                speedBox.Size = new Size(160, 26);
                StyleTextBox(speedBox);
                card.Controls.Add(speedBox);

                var playCheck = new CheckBox();
                playCheck.Text = "Apply continuously while playing  (uncheck = apply on each pause/resume)";
                playCheck.Checked = whilePlaying;
                playCheck.ForeColor = TextPrimary;
                playCheck.BackColor = Surface;
                playCheck.Location = new Point(22, 238);
                playCheck.Size = new Size(420, 44);
                card.Controls.Add(playCheck);

                Action updateModeButtons = delegate
                {
                    StyleXpGainModeButton(xpMode, !useSkillPointEntry);
                    StyleXpGainModeButton(skillMode, useSkillPointEntry);
                    xpLabel.Text = useSkillPointEntry ? "Skill points per fire" : "XP amount per fire";
                    xpBox.Visible = !useSkillPointEntry;
                    skillBox.Visible = useSkillPointEntry;
                };

                Action updateSkillSummary = delegate
                {
                    if (useSkillPointEntry)
                    {
                        int parsedSkill;
                        if (!int.TryParse(skillBox.Text.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out parsedSkill) ||
                            parsedSkill <= 0)
                        {
                            skillSummary.Text = "Enter at least 1 skill point.";
                            skillSummary.ForeColor = AccentRed;
                            return;
                        }

                        var derivedXp = SafeSkillPointsToXp(parsedSkill);
                        skillSummary.Text = "Will apply " + parsedSkill.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                            " skill point(s) (" + derivedXp.ToString(System.Globalization.CultureInfo.InvariantCulture) + " XP) per fire.";
                        skillSummary.ForeColor = AccentGreen;
                        return;
                    }

                    int parsedXp;
                    if (!int.TryParse(xpBox.Text.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out parsedXp) ||
                        parsedXp < 0)
                    {
                        skillSummary.Text = "Enter a whole XP number.";
                        skillSummary.ForeColor = AccentRed;
                        return;
                    }

                    var exact = parsedXp % XpGainXpPerSkillPoint == 0;
                    var roundedXp = RoundXpToSkillPointEvent(parsedXp);
                    var points = roundedXp / XpGainXpPerSkillPoint;
                    skillSummary.Text = exact
                        ? "Will apply " + points.ToString(System.Globalization.CultureInfo.InvariantCulture) + " skill point(s) per fire."
                        : "Will round to " + roundedXp.ToString(System.Globalization.CultureInfo.InvariantCulture) + " XP (" + points.ToString(System.Globalization.CultureInfo.InvariantCulture) + " skill point(s)).";
                    skillSummary.ForeColor = exact ? AccentGreen : AccentOrange;
                };
                xpBox.TextChanged += delegate
                {
                    if (!syncingAmountBoxes)
                    {
                        int parsedXp;
                        if (int.TryParse(xpBox.Text.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out parsedXp) && parsedXp > 0)
                        {
                            syncingAmountBoxes = true;
                            skillBox.Text = (RoundXpToSkillPointEvent(parsedXp) / XpGainXpPerSkillPoint).ToString(System.Globalization.CultureInfo.InvariantCulture);
                            syncingAmountBoxes = false;
                        }
                    }
                    updateSkillSummary();
                };
                skillBox.TextChanged += delegate
                {
                    if (!syncingAmountBoxes)
                    {
                        int parsedSkill;
                        if (int.TryParse(skillBox.Text.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out parsedSkill) && parsedSkill >= 0)
                        {
                            syncingAmountBoxes = true;
                            xpBox.Text = SafeSkillPointsToXp(parsedSkill).ToString(System.Globalization.CultureInfo.InvariantCulture);
                            syncingAmountBoxes = false;
                        }
                    }
                    updateSkillSummary();
                };
                Action<bool> setMode = delegate(bool skillModeOn)
                {
                    useSkillPointEntry = skillModeOn;
                    updateModeButtons();
                    updateSkillSummary();
                    if (useSkillPointEntry)
                        skillBox.Focus();
                    else
                        xpBox.Focus();
                };
                xpMode.Click += delegate { setMode(false); };
                skillMode.Click += delegate { setMode(true); };
                updateModeButtons();
                updateSkillSummary();

                var howToTitle = new Label();
                howToTitle.Text = "How to use it";
                howToTitle.Font = new Font("Segoe UI Semibold", 10F);
                howToTitle.ForeColor = TextPrimary;
                howToTitle.BackColor = Surface;
                howToTitle.Location = new Point(22, 292);
                howToTitle.Size = new Size(420, 22);
                card.Controls.Add(howToTitle);

                var steps = new Label();
                steps.Text =
                    "1.  Choose XP Amount or Skill Points. Luna keeps both values matched.\n" +
                    "2.  Turn XP Gain green and press Apply.\n" +
                    "3.  In FH6, spend ONE skill point in the perk menu once to arm it. It then keeps adding that reward automatically until you turn it red.";
                steps.Font = new Font("Segoe UI", 9F);
                steps.ForeColor = TextMuted;
                steps.BackColor = Surface;
                steps.Location = new Point(22, 318);
                steps.Size = new Size(424, 126);
                card.Controls.Add(steps);

                string result = null;
                var ok = MakeButton("Apply", 252, 454, 90, 34);
                ok.Click += delegate
                {
                    int skillPoints;
                    int xp;
                    if (useSkillPointEntry)
                    {
                        if (!int.TryParse(skillBox.Text.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out skillPoints) ||
                            skillPoints <= 0)
                        {
                            ShowInfo("Skill points per fire must be a whole number 1 or greater.");
                            return;
                        }
                        xp = SafeSkillPointsToXp(skillPoints);
                    }
                    else
                    {
                        if (!int.TryParse(xpBox.Text.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out xp) ||
                            xp <= 0)
                        {
                            ShowInfo("XP amount must be a whole number greater than zero.");
                            return;
                        }
                        var roundedXp = RoundXpToSkillPointEvent(xp);
                        if (roundedXp != xp)
                        {
                            if (!ShowXpRoundedChoice(xp, roundedXp))
                            {
                                xpBox.Focus();
                                xpBox.SelectAll();
                                updateSkillSummary();
                                return;
                            }

                            xp = roundedXp;
                            xpBox.Text = xp.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        }
                        skillPoints = xp / XpGainXpPerSkillPoint;
                    }

                    int fires;
                    if (!int.TryParse(fireBox.Text.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out fires) || fires < 0)
                    {
                        ShowInfo("Times to fire must be a whole number 0 or greater. Use 0 for continuous.");
                        return;
                    }
                    int s;
                    if (!int.TryParse(speedBox.Text.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out s))
                        s = 1000;
                    s = Math.Max(100, Math.Min(10000, s));
                    result = skillPoints.ToString(System.Globalization.CultureInfo.InvariantCulture) + "/" +
                             s.ToString(System.Globalization.CultureInfo.InvariantCulture) + "/" +
                             (playCheck.Checked ? "1" : "0") + "/" +
                             fires.ToString(System.Globalization.CultureInfo.InvariantCulture) + "/" +
                             xp.ToString(System.Globalization.CultureInfo.InvariantCulture) + "/" +
                             (useSkillPointEntry ? "sp" : "xp");
                    dialog.DialogResult = DialogResult.OK;
                    dialog.Close();
                };
                card.Controls.Add(ok);

                var cancel = MakeButton("Cancel", 348, 454, 90, 34);
                cancel.DialogResult = DialogResult.Cancel;
                card.Controls.Add(cancel);
                dialog.CancelButton = cancel;

                PrepareDialogForLanguage(dialog);
                return dialog.ShowDialog(this) == DialogResult.OK ? result : null;
            }
        }

        private static int RoundXpToSkillPointEvent(int xp)
        {
            if (xp <= 0)
                return XpGainXpPerSkillPoint;

            var maxXp = (int.MaxValue / XpGainXpPerSkillPoint) * XpGainXpPerSkillPoint;
            var rounded = (int)(Math.Round((double)xp / XpGainXpPerSkillPoint, MidpointRounding.AwayFromZero) * XpGainXpPerSkillPoint);
            if (rounded < XpGainXpPerSkillPoint)
                rounded = XpGainXpPerSkillPoint;
            if (rounded > maxXp)
                rounded = maxXp;
            return rounded;
        }

        private static int SafeSkillPointsToXp(int skillPoints)
        {
            if (skillPoints <= 0)
                return 0;

            var maxSkillPoints = int.MaxValue / XpGainXpPerSkillPoint;
            if (skillPoints > maxSkillPoints)
                skillPoints = maxSkillPoints;
            return skillPoints * XpGainXpPerSkillPoint;
        }

        private static void StyleXpGainModeButton(Button button, bool active)
        {
            var modern = button as ModernButton;
            if (modern != null)
            {
                var fill = active ? AccentBlue : SurfaceAlt;
                modern.FillColor = fill;
                modern.HoverColor = active ? ControlPaint.Light(fill, 0.08F) : Blend(fill, Color.White, 0.08F);
                modern.PressedColor = active ? ControlPaint.Dark(fill, 0.08F) : Blend(fill, Color.Black, 0.08F);
                modern.BorderColor = active ? AccentBlue : Border;
                modern.TracksTheme = false;
            }
            button.ForeColor = active ? Color.White : TextPrimary;
        }

        private bool ShowXpRoundedChoice(int requestedXp, int roundedXp)
        {
            if (InvokeRequired)
                return (bool)Invoke(new Func<int, int, bool>(ShowXpRoundedChoice), requestedXp, roundedXp);

            using (var dialog = new Form())
            {
                dialog.Text = "XP Amount Rounded";
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.BackColor = AppBackground;
                dialog.ForeColor = TextPrimary;
                dialog.Font = new Font("Segoe UI", 9F);
                dialog.ClientSize = new Size(420, 224);

                var card = new ModernPanel();
                card.Location = new Point(14, 14);
                card.Size = new Size(392, 196);
                card.FillColor = Surface;
                card.BorderColor = Border;
                card.CornerRadius = 14;
                dialog.Controls.Add(card);

                var title = new Label();
                title.Text = "XP Rounded";
                title.Font = new Font("Segoe UI Semibold", 13F);
                title.ForeColor = TextPrimary;
                title.BackColor = Surface;
                title.Location = new Point(18, 16);
                title.Size = new Size(356, 28);
                title.TextAlign = ContentAlignment.MiddleCenter;
                card.Controls.Add(title);

                var skillPoints = roundedXp / XpGainXpPerSkillPoint;
                var body = MakeBodyLabel(
                    "1 skill point = 200 XP, so Luna rounded " +
                    requestedXp.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    " XP to " +
                    roundedXp.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    " XP (" +
                    skillPoints.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    " skill point(s)).\n\nKeep the rounded value or change it?",
                    24,
                    56,
                    344,
                    82);
                body.BackColor = Surface;
                body.ForeColor = TextMuted;
                card.Controls.Add(body);

                var keep = MakeButton("Keep", 110, 148, 92, 32);
                MakeAccentButton(keep, AccentGreen);
                keep.Click += delegate
                {
                    dialog.DialogResult = DialogResult.OK;
                    dialog.Close();
                };
                card.Controls.Add(keep);

                var change = MakeButton("Change", 214, 148, 92, 32);
                change.Click += delegate
                {
                    dialog.DialogResult = DialogResult.Cancel;
                    dialog.Close();
                };
                card.Controls.Add(change);
                dialog.CancelButton = change;

                PrepareDialogForLanguage(dialog);
                return dialog.ShowDialog(this) == DialogResult.OK;
            }
        }

        private void AddRuntimeFeatureHotkeyControls(Control parent, int x, int y, RuntimeProfileFeature feature, string labelText, TextBox valueBox, StatusDotToggle toggle)
        {
            if (!IsRuntimeFeatureHotkeySupported(feature))
                return;

            if (toggle != null)
                _runtimeFeatureToggles[feature] = toggle;
            if (valueBox != null)
                _profileValueBoxes[feature] = valueBox;

            var key = MakeButton(GetRuntimeFeatureHotkeyText(feature), x + 730, y - 2, 64, 30);
            key.Click += delegate
            {
                var selected = ShowKeybindPrompt("Set " + labelText + " Key", "Press a key for " + labelText);
                if (!selected.HasValue)
                    return;

                SetRuntimeFeatureHotkey(feature, selected.Value);
                SaveAppSettings();
                UpdateRuntimeFeatureHotkeyButtons(feature);
                ReRegisterRuntimeFeatureHotkey(feature);
                SetFeaturesStatus(labelText + " key set to " + GetRuntimeFeatureHotkeyText(feature), AccentPurple);
            };
            parent.Controls.Add(key);
            _runtimeFeatureHotkeyButtons[feature] = key;
            SetTranslatedToolTip(key, "Click to choose the optional hotkey for " + labelText + ".");

            var arm = MakeButton("Key OFF", x + 804, y - 2, 86, 30);
            arm.Click += delegate { ToggleRuntimeFeatureHotkeyArm(feature); };
            parent.Controls.Add(arm);
            _runtimeFeatureHotkeyArmButtons[feature] = arm;
            SetTranslatedToolTip(arm, "Turns the optional " + labelText + " hotkey on or off.");

            UpdateRuntimeFeatureHotkeyButtons(feature);
        }

        private void AddProfileTextRow(Control parent, string labelText, string defaultValue, int x, int y, RuntimeProfileFeature feature, string tooltip)
        {
            var label = MakeLabel(labelText, x, y + 4);
            label.Width = 190;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var value = new TextBox();
            value.Text = defaultValue ?? string.Empty;
            value.Location = new Point(x + 420, y);
            value.Size = new Size(116, 28);
            value.TextAlign = HorizontalAlignment.Right;
            StyleTextBox(value);
            parent.Controls.Add(value);
            _profileValueBoxes[feature] = value;
            SetTranslatedToolTip(value, tooltip);

            var toggle = new StatusDotToggle();
            toggle.Location = new Point(x + 566, y + 3);
            toggle.Size = new Size(24, 24);
            parent.Controls.Add(toggle);
            SetTranslatedToolTip(toggle, tooltip + " Green is on. Red turns it off right away.");

            WireImmediateRuntimeOff(toggle, feature, labelText);

            var apply = MakeButton("Apply", x + 620, y - 2, 96, 30);
            apply.Click += delegate
            {
                var text = value.Text;
                var enabled = toggle.Checked;
                RunWorker(labelText, delegate { ApplyProfileRuntimeHook(feature, labelText, text, enabled); }, apply);
            };
            parent.Controls.Add(apply);
            SetTranslatedToolTip(apply, tooltip);
        }

        private static string FormatRuntimeValueButtonText(string value)
        {
            value = (value ?? string.Empty).Trim();
            if (value.Length == 0)
                return "Click here";
            return value.Length > 12 ? value.Substring(0, 12) : value;
        }

        private static string FormatXpGainConfigButtonText(string value)
        {
            int amount;
            int speedMs;
            int fireCount;
            int xpAmount;
            bool useSkillPointMode;
            bool whilePlaying;
            ParseXpGainConfig(value, out amount, out speedMs, out whilePlaying, out fireCount, out xpAmount, out useSkillPointMode);
            return useSkillPointMode
                ? amount.ToString(System.Globalization.CultureInfo.InvariantCulture) + " SP"
                : xpAmount.ToString(System.Globalization.CultureInfo.InvariantCulture) + " XP";
        }

        private void AddAccelerationSafeToggleRow(Control parent, int x, int y)
        {
            const string labelText = "Acceleration";
            const string tooltip = "Green applies a softened acceleration boost through the working FH6 vehicle-control path while W or Up is held.";

            var label = MakeLabel(labelText, x, y + 4);
            label.Width = 172;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var value = MakeButton(FormatAccelerationButtonText(), x + 420, y - 2, 116, 30);
            value.Click += delegate
            {
                var config = ShowAccelerationConfigPrompt();
                if (config == null)
                    return;

                _accelerationUsePercentage = config.UsePercentage;
                _accelerationToggleMultiplier = config.CustomMultiplier;
                _accelerationPercentage = config.Percentage;
                if (config.MakeDefaultCustom || config.MakeDefaultPercentage)
                {
                    _accelerationDefaultUsePercentage = config.MakeDefaultPercentage;
                    _accelerationDefaultMultiplier = config.CustomMultiplier;
                    _accelerationDefaultPercentage = config.Percentage;
                    SaveAppSettings();
                }
                UpdateAccelerationButtonText();
                SetFeaturesStatus("Acceleration set to " + FormatAccelerationButtonText(), AccentPurple);
                Log("Acceleration set to " + FormatAccelerationLogText() + ". Toggle it on and press Apply.");
            };
            parent.Controls.Add(value);
            SetTranslatedToolTip(value, "Click to edit acceleration. Pick Custom Speed or Percentage. Percentage is 0% to 100%; 100% writes the 103% cap. Only one mode can be active.");
            _accelerationMultiplierButton = value;

            var toggle = new StatusDotToggle();
            toggle.Location = new Point(x + 566, y + 3);
            toggle.Size = new Size(24, 24);
            parent.Controls.Add(toggle);
            _accelerationFeatureToggle = toggle;
            _runtimeFeatureToggles[RuntimeProfileFeature.Acceleration] = toggle;
            SetTranslatedToolTip(toggle, "Green is on. Red turns it off right away.");
            WireImmediateRuntimeOff(toggle, RuntimeProfileFeature.Acceleration, labelText);

            var apply = MakeButton("Apply", x + 620, y - 2, 96, 30);
            apply.Click += delegate
            {
                var text = FormatFloat(GetAccelerationRequestedMultiplier());
                var enabled = toggle.Checked;
                if (enabled && !_accelerationWarningAcknowledged)
                {
                    ShowInfo("Highly recommended: toggle Adaptive Brake with Acceleration." + Environment.NewLine + Environment.NewLine +
                        "Acceleration works best with Adaptive Brake and supporting Driving Editor features enabled so the car stays controllable." + Environment.NewLine + Environment.NewLine +
                        "This note will not show again this session.");
                    _accelerationWarningAcknowledged = true;
                }
                RunWorker(labelText, delegate { ApplyProfileRuntimeHook(RuntimeProfileFeature.Acceleration, labelText, text, enabled); }, apply);
            };
            parent.Controls.Add(apply);
            SetTranslatedToolTip(apply, tooltip);
            AddRuntimeFeatureHotkeyControls(parent, x, y, RuntimeProfileFeature.Acceleration, labelText, null, toggle);
        }

        private void AddSuperBrakeKeybindRow(Control parent, int x, int y)
        {
            const string labelText = "Super Brake";
            const string tooltip = "Green arms instant brake. It hard-stops for the first second you hold the brake key, then lets the car reverse.";

            var label = MakeLabel(labelText, x, y + 4);
            label.Width = 172;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var key = MakeButton(GetBrakeKeyText(), x + 420, y - 2, 116, 30);
            key.Click += delegate
            {
                var selected = ShowKeybindPrompt("Set Brake Key", "Press the key for Super Brake and Adaptive Brake");
                if (!selected.HasValue)
                    return;

                _brakeVirtualKey = selected.Value;
                UpdateBrakeKeyButton();
                SetFeaturesStatus("Brake key set to " + GetBrakeKeyText(), AccentPurple);
                Log("Brake keybind set to " + GetBrakeKeyText() + ".");
            };
            parent.Controls.Add(key);
            SetTranslatedToolTip(key, "Default is S. Click to choose another brake key.");
            _brakeKeyButton = key;

            var toggle = new StatusDotToggle();
            toggle.Location = new Point(x + 566, y + 3);
            toggle.Size = new Size(24, 24);
            parent.Controls.Add(toggle);
            SetTranslatedToolTip(toggle, tooltip + " Green is on. Red turns it off right away.");
            WireImmediateRuntimeOff(toggle, RuntimeProfileFeature.SuperBrake, labelText);

            var apply = MakeButton("Apply", x + 620, y - 2, 96, 30);
            apply.Click += delegate
            {
                var enabled = toggle.Checked;
                RunWorker(labelText, delegate { ApplyProfileRuntimeHook(RuntimeProfileFeature.SuperBrake, labelText, string.Empty, enabled); }, apply);
            };
            parent.Controls.Add(apply);
            SetTranslatedToolTip(apply, tooltip);
        }

        private void AddAdaptiveBrakeRow(Control parent, int x, int y)
        {
            const string labelText = "Adaptive Brake";
            const string tooltip = "Green arms a brake multiplier while held. Values are uncapped; lower values brake harder without forcing a full stop.";

            var label = MakeLabel(labelText, x, y + 4);
            label.Width = 172;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var value = MakeButton("x" + FormatFloat(_adaptiveBrakeMultiplier), x + 420, y - 2, 62, 30);
            value.Click += delegate
            {
                var custom = ShowBrakeStrengthPrompt(_adaptiveBrakeMultiplier);
                if (!custom.HasValue)
                    return;

                _adaptiveBrakeMultiplier = custom.Value;
                UpdateAdaptiveBrakeButtonText();
                SetFeaturesStatus("Adaptive Brake set to x" + FormatFloat(_adaptiveBrakeMultiplier), AccentPurple);
                Log("Adaptive Brake strength set to x" + FormatFloat(_adaptiveBrakeMultiplier) + ".");
            };
            parent.Controls.Add(value);
            SetTranslatedToolTip(value, "Click to edit brake strength. Lower is stronger. Default is x0.97.");
            _adaptiveBrakeButton = value;

            var key = MakeButton(GetBrakeKeyText(), x + 488, y - 2, 68, 30);
            key.Click += delegate
            {
                var selected = ShowKeybindPrompt("Set Adaptive Brake Key", "Press the key for Adaptive Brake and Super Brake");
                if (!selected.HasValue)
                    return;

                _brakeVirtualKey = selected.Value;
                UpdateBrakeKeyButton();
                SetFeaturesStatus("Brake key set to " + GetBrakeKeyText(), AccentPurple);
                Log("Brake keybind set to " + GetBrakeKeyText() + ".");
            };
            parent.Controls.Add(key);
            SetTranslatedToolTip(key, "Default is S. Click to choose the brake key used by Adaptive Brake.");
            _adaptiveBrakeKeyButton = key;

            var toggle = new StatusDotToggle();
            toggle.Location = new Point(x + 566, y + 3);
            toggle.Size = new Size(24, 24);
            parent.Controls.Add(toggle);
            SetTranslatedToolTip(toggle, tooltip + " Green is on. Red turns it off right away.");
            WireImmediateRuntimeOff(toggle, RuntimeProfileFeature.AdaptiveBrake, labelText);

            var apply = MakeButton("Apply", x + 620, y - 2, 96, 30);
            apply.Click += delegate
            {
                var enabled = toggle.Checked;
                RunWorker(labelText, delegate { ApplyProfileRuntimeHook(RuntimeProfileFeature.AdaptiveBrake, labelText, FormatFloat(_adaptiveBrakeMultiplier), enabled); }, apply);
            };
            parent.Controls.Add(apply);
            SetTranslatedToolTip(apply, tooltip);
        }

        private void UpdateAccelerationButtonText()
        {
            if (_accelerationMultiplierButton != null && !_accelerationMultiplierButton.IsDisposed)
                _accelerationMultiplierButton.Text = FormatAccelerationButtonText();
        }

        private string FormatAccelerationButtonText()
        {
            NormalizeAccelerationConfigState();
            if (_accelerationUsePercentage)
                return _accelerationPercentage.ToString(CultureInfo.InvariantCulture) + "%";
            return FormatFloat(_accelerationToggleMultiplier);
        }

        private string FormatAccelerationLogText()
        {
            NormalizeAccelerationConfigState();
            if (_accelerationUsePercentage)
                return _accelerationPercentage.ToString(CultureInfo.InvariantCulture) + "% requested x" + FormatFloat(GetAccelerationRequestedMultiplier());
            return "x" + FormatFloat(_accelerationToggleMultiplier);
        }

        private float GetAccelerationRequestedMultiplier()
        {
            NormalizeAccelerationConfigState();
            if (_accelerationUsePercentage)
                return AccelerationPercentToMultiplier(_accelerationPercentage);
            return _accelerationToggleMultiplier;
        }

        private void NormalizeAccelerationConfigState()
        {
            if (float.IsNaN(_accelerationToggleMultiplier) || float.IsInfinity(_accelerationToggleMultiplier) || _accelerationToggleMultiplier <= 0F)
                _accelerationToggleMultiplier = SafeAccelerationMultiplier;
            if (float.IsNaN(_accelerationDefaultMultiplier) || float.IsInfinity(_accelerationDefaultMultiplier) || _accelerationDefaultMultiplier <= 0F)
                _accelerationDefaultMultiplier = SafeAccelerationMultiplier;
            _accelerationPercentage = Math.Max(0, Math.Min(MaxAccelerationPercent, _accelerationPercentage));
            _accelerationDefaultPercentage = Math.Max(0, Math.Min(MaxAccelerationPercent, _accelerationDefaultPercentage));
        }

        private void UpdateAdaptiveBrakeButtonText()
        {
            if (_adaptiveBrakeButton != null && !_adaptiveBrakeButton.IsDisposed)
                _adaptiveBrakeButton.Text = "x" + FormatFloat(_adaptiveBrakeMultiplier);
        }

        private void UpdateBrakeKeyButton()
        {
            if (_brakeKeyButton != null && !_brakeKeyButton.IsDisposed)
                _brakeKeyButton.Text = GetBrakeKeyText();
            if (_adaptiveBrakeKeyButton != null && !_adaptiveBrakeKeyButton.IsDisposed)
                _adaptiveBrakeKeyButton.Text = GetBrakeKeyText();
        }

        private void AddJumpKeybindRow(Control parent, int x, int y)
        {
            const string labelText = "Jump";
            const string tooltip = "Green arms Jump. Press the selected key in-game to launch the car using the chosen strength.";

            var label = MakeLabel(labelText, x, y + 4);
            label.Width = 190;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var value = MakeButton(FormatFloat(_jumpBoost), x + 420, y - 2, 62, 30);
            value.Click += delegate
            {
                var custom = ShowJumpStrengthPrompt(_jumpBoost);
                if (!custom.HasValue)
                    return;

                _jumpBoost = custom.Value;
                UpdateJumpValueButton();
                SaveAppSettings();
                SetFeaturesStatus("Jump strength set to " + FormatFloat(_jumpBoost), AccentPurple);
                Log("Jump strength set to " + FormatFloat(_jumpBoost) + ".");
            };
            parent.Controls.Add(value);
            SetTranslatedToolTip(value, "Click to edit Jump strength. Default is 5. Use a positive number.");
            _jumpValueButton = value;

            var key = MakeButton(GetJumpKeyText(), x + 488, y - 2, 58, 30);
            key.Click += delegate
            {
                var selected = ShowJumpKeybindPrompt();
                if (!selected.HasValue)
                    return;

                _jumpVirtualKey = selected.Value;
                UpdateJumpKeyButton();
                SaveAppSettings();
                SetFeaturesStatus("Jump key set to " + GetJumpKeyText(), AccentPurple);
                Log("Jump keybind set to " + GetJumpKeyText() + ".");
            };
            parent.Controls.Add(key);
            SetTranslatedToolTip(key, "Click, then press the key you want to use for Jump. Default is Space.");
            _jumpKeyButton = key;

            var toggle = new StatusDotToggle();
            toggle.Location = new Point(x + 566, y + 3);
            toggle.Size = new Size(24, 24);
            parent.Controls.Add(toggle);
            SetTranslatedToolTip(toggle, tooltip + " Green is on. Red turns it off right away.");
            WireImmediateJumpOff(toggle, labelText);
            _runtimeFeatureToggles[RuntimeProfileFeature.Jump] = toggle;

            var apply = MakeButton("Apply", x + 620, y - 2, 96, 30);
            apply.Click += delegate
            {
                var enabled = toggle.Checked;
                RunWorker(labelText, delegate { ApplyJumpRuntimeHook(enabled); }, apply);
            };
            parent.Controls.Add(apply);
            SetTranslatedToolTip(apply, tooltip);
        }

        private void AddWallJumperKeybindRow(Control parent, int x, int y)
        {
            const string labelText = "Wall Jumper";
            const string tooltip = "Green arms Wall Jumper. Press the selected key in-game to fire a strong upward and forward wall jump using the selected strength.";

            var label = MakeLabel(labelText, x, y + 4);
            label.Width = 190;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var valueBox = new TextBox();
            valueBox.Text = FormatFloat(_wallJumperStrength);
            valueBox.Visible = false;
            parent.Controls.Add(valueBox);
            _profileValueBoxes[RuntimeProfileFeature.WallClimber] = valueBox;

            var value = MakeButton(FormatFloat(_wallJumperStrength), x + 420, y - 2, 62, 30);
            value.Click += delegate
            {
                var custom = ShowWallJumperStrengthPrompt(_wallJumperStrength);
                if (!custom.HasValue)
                    return;

                _wallJumperStrength = custom.Value;
                valueBox.Text = FormatFloat(_wallJumperStrength);
                UpdateWallJumperValueButton();
                SaveAppSettings();
                SetFeaturesStatus("Wall Jumper strength set to " + FormatFloat(_wallJumperStrength), AccentPurple);
                Log("Wall Jumper strength set to " + FormatFloat(_wallJumperStrength) + ".");
            };
            parent.Controls.Add(value);
            SetTranslatedToolTip(value, "Click to edit Wall Jumper strength. Default is 1. Use 0.1 to 10.");
            _wallJumperValueButton = value;

            var key = MakeButton(GetWallJumperKeyText(), x + 488, y - 2, 58, 30);
            key.Click += delegate
            {
                var selected = ShowKeybindPrompt("Set Wall Jumper Key", "Press a key for Wall Jumper");
                if (!selected.HasValue)
                    return;

                _wallJumperVirtualKey = selected.Value;
                UpdateWallJumperKeyButton();
                SaveAppSettings();
                SetFeaturesStatus("Wall Jumper key set to " + GetWallJumperKeyText(), AccentPurple);
                Log("Wall Jumper keybind set to " + GetWallJumperKeyText() + ".");
            };
            parent.Controls.Add(key);
            SetTranslatedToolTip(key, "Click, then press the key you want to use for Wall Jumper. Default is J.");
            _wallJumperKeyButton = key;

            var toggle = new StatusDotToggle();
            toggle.Location = new Point(x + 566, y + 3);
            toggle.Size = new Size(24, 24);
            parent.Controls.Add(toggle);
            SetTranslatedToolTip(toggle, tooltip + " Green is on. Red turns it off right away.");
            toggle.CheckedChanged += delegate
            {
                RequestFeatureOverlayRefresh();
                if (toggle.Checked)
                    return;
                if (_database == null || !_database.IsAlive)
                    return;

                RunWorker(labelText + " Off", delegate { ApplyWallJumperRuntimeHook(false); });
            };
            _runtimeFeatureToggles[RuntimeProfileFeature.WallClimber] = toggle;

            var apply = MakeButton("Apply", x + 620, y - 2, 96, 30);
            apply.Click += delegate
            {
                var enabled = toggle.Checked;
                valueBox.Text = FormatFloat(_wallJumperStrength);
                RunWorker(labelText, delegate { ApplyWallJumperRuntimeHook(enabled); }, apply);
            };
            parent.Controls.Add(apply);
            SetTranslatedToolTip(apply, tooltip);
        }

        private void AddChargeJumpKeybindRow(Control parent, int x, int y)
        {
            const string labelText = "Charge Jump";
            const string tooltip = "Green arms Charge Jump. Hold the selected key to charge, release to catapult upward and forward. The overlay shows charge percent.";

            var label = MakeLabel(labelText, x, y + 4);
            label.Width = 190;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var config = MakeButton("Config", x + 400, y - 2, 82, 30);
            config.Click += delegate
            {
                if (!ShowChargeJumpConfigPrompt())
                    return;

                UpdateChargeJumpConfigButton();
                SaveAppSettings();
                try
                {
                    ApplyChargeJumpRuntimeConfigIfAttached();
                    SetFeaturesStatus("Charge Jump config saved: " + BuildChargeJumpConfigSummary(), AccentPurple);
                    Log("Charge Jump config saved: " + BuildChargeJumpConfigSummary() + ".");
                }
                catch (Exception ex)
                {
                    SetFeaturesStatus("Charge Jump config saved locally, but live update failed: " + ex.Message, AccentOrange);
                    Log("Charge Jump config saved locally, but live update failed: " + ex.Message);
                }
            };
            parent.Controls.Add(config);
            SetTranslatedToolTip(config, "Open Charge Jump values. Changes apply to the active hook after saving.");
            _chargeJumpConfigButton = config;

            var key = MakeButton(GetChargeJumpKeyText(), x + 490, y - 2, 96, 30);
            key.Click += delegate
            {
                var selected = ShowKeybindPrompt("Set Charge Jump Key", "Press a key for Charge Jump");
                if (!selected.HasValue)
                    return;

                _chargeJumpVirtualKey = selected.Value;
                UpdateChargeJumpKeyButton();
                SaveAppSettings();
                SetFeaturesStatus("Charge Jump key set to " + GetChargeJumpKeyText(), AccentPurple);
                Log("Charge Jump keybind set to " + GetChargeJumpKeyText() + ".");
            };
            parent.Controls.Add(key);
            SetTranslatedToolTip(key, "Default is Space. Hold to charge, release to launch.");
            _chargeJumpKeyButton = key;

            var toggle = new StatusDotToggle();
            toggle.Location = new Point(x + 604, y + 3);
            toggle.Size = new Size(24, 24);
            parent.Controls.Add(toggle);
            SetTranslatedToolTip(toggle, tooltip + " Green is armed. Red turns it off right away.");
            toggle.CheckedChanged += delegate
            {
                RequestFeatureOverlayRefresh();
                if (toggle.Checked)
                    return;
                if (_database == null || !_database.IsAlive)
                    return;

                RunWorker(labelText + " Off", delegate { ApplyChargeJumpRuntimeHook(false); });
            };
            _runtimeFeatureToggles[RuntimeProfileFeature.ChargeJump] = toggle;

            var apply = MakeButton("Apply", x + 646, y - 2, 96, 30);
            apply.Click += delegate
            {
                var enabled = toggle.Checked;
                RunWorker(labelText, delegate { ApplyChargeJumpRuntimeHook(enabled); }, apply);
            };
            parent.Controls.Add(apply);
            SetTranslatedToolTip(apply, tooltip);
        }

        private void AddBoostKeybindRow(Control parent, int x, int y)
        {
            const string labelText = "Boost";
            const string tooltip = "Green arms a held nitro boost. Hold the selected key in-game; Boost uses the same acceleration-style multiplier path while held.";

            var label = MakeLabel(labelText, x, y + 4);
            label.Width = 190;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var value = MakeButton(FormatFloat(_boostForce), x + 420, y - 2, 62, 30);
            value.Click += delegate
            {
                var custom = ShowBoostForcePrompt(_boostForce);
                if (!custom.HasValue)
                    return;

                _boostForce = custom.Value;
                UpdateBoostValueButton();
                SetFeaturesStatus("Boost amount set to " + FormatFloat(_boostForce), AccentPurple);
                Log("Boost amount set to " + FormatFloat(_boostForce) + ".");
            };
            parent.Controls.Add(value);
            SetTranslatedToolTip(value, "Click to edit boost amount. Default is 0.02, which becomes x1.02 while held. Higher numbers use Luna's stable input curve.");
            _boostValueButton = value;

            var key = MakeButton(GetBoostKeyText(), x + 488, y - 2, 58, 30);
            key.Click += delegate
            {
                var selected = ShowKeybindPrompt("Set Boost Key", "Press a key for Boost");
                if (!selected.HasValue)
                    return;

                _boostVirtualKey = selected.Value;
                UpdateBoostKeyButton();
                SetFeaturesStatus("Boost key set to " + GetBoostKeyText(), AccentPurple);
                Log("Boost keybind set to " + GetBoostKeyText() + ".");
            };
            parent.Controls.Add(key);
            SetTranslatedToolTip(key, "Default is B. Click to choose another boost key.");
            _boostKeyButton = key;

            var toggle = new StatusDotToggle();
            toggle.Location = new Point(x + 566, y + 3);
            toggle.Size = new Size(24, 24);
            parent.Controls.Add(toggle);
            SetTranslatedToolTip(toggle, tooltip + " Green is on. Red turns it off right away.");
            WireImmediateBoostOff(toggle, labelText);
            _runtimeFeatureToggles[RuntimeProfileFeature.Boost] = toggle;

            var apply = MakeButton("Apply", x + 620, y - 2, 96, 30);
            apply.Click += delegate
            {
                var enabled = toggle.Checked;
                RunWorker(labelText, delegate { ApplyBoostRuntimeHook(enabled); }, apply);
            };
            parent.Controls.Add(apply);
            SetTranslatedToolTip(apply, tooltip);
        }

        private void AddFlyRow(Control parent, int x, int y)
        {
            const string labelText = "Fly";
            const string tooltip = "Green enables stable flight. Space or Jump rises, Ctrl descends, W/A/S/D drives and turns, arrow keys nudge the car's position (left/right strafe, up/down slide), and releasing keys holds position.";

            var label = MakeLabel(labelText, x, y + 4);
            label.Width = 190;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var valueBox = new TextBox();
            valueBox.Text = FormatFloat(_flySpeed);
            valueBox.Visible = false;
            valueBox.Location = new Point(x + 420, y);
            valueBox.Size = new Size(1, 1);
            parent.Controls.Add(valueBox);
            _profileValueBoxes[RuntimeProfileFeature.Hover] = valueBox;

            var speed = MakeButton(TranslateDynamicUi("Speed") + " " + FormatFloat(_flySpeed), x + 420, y - 2, 116, 30);
            speed.Click += delegate
            {
                var custom = ShowFlySpeedPrompt(_flySpeed);
                if (!custom.HasValue)
                    return;

                _flySpeed = custom.Value;
                valueBox.Text = FormatFloat(_flySpeed);
                UpdateFlySpeedButton();
                SaveAppSettings();
                SetFeaturesStatus("Fly speed set to " + FormatFloat(_flySpeed), AccentPurple);
                Log("Fly speed set to " + FormatFloat(_flySpeed) + ".");
                StatusDotToggle flyToggle;
                if (_runtimeFeatureToggles.TryGetValue(RuntimeProfileFeature.Hover, out flyToggle) &&
                    flyToggle != null &&
                    flyToggle.Checked &&
                    _database != null &&
                    _database.IsAlive)
                    RunWorker("Fly Speed", delegate { ApplyProfileRuntimeHook(RuntimeProfileFeature.Hover, labelText, valueBox.Text, true); });
            };
            parent.Controls.Add(speed);
            _flySpeedButton = speed;
            SetTranslatedToolTip(speed, "Click to edit Fly movement speed. Default is " + FormatFloat(DefaultFlySpeed) + ".");

            var toggle = new StatusDotToggle();
            toggle.Location = new Point(x + 566, y + 3);
            toggle.Size = new Size(24, 24);
            parent.Controls.Add(toggle);
            SetTranslatedToolTip(toggle, tooltip + " Green is on. Red turns it off right away.");
            WireImmediateRuntimeOff(toggle, RuntimeProfileFeature.Hover, labelText);
            toggle.CheckedChanged += delegate
            {
                if (!toggle.Checked)
                    return;
                if (_database == null || !_database.IsAlive)
                    return;

                RunWorker(labelText + " On", delegate { ApplyProfileRuntimeHook(RuntimeProfileFeature.Hover, labelText, valueBox.Text, true); });
            };

            var apply = MakeButton("Apply", x + 620, y - 2, 96, 30);
            apply.Click += delegate
            {
                var enabled = toggle.Checked;
                if (enabled && !_flyRecommendationAcknowledged)
                {
                    ShowInfo("It's highly recommended to drive forward and use the hotkey (" + ((Keys)_flyHotkeyVirtualKey) + ") to turn on Fly." + Environment.NewLine + Environment.NewLine +
                        "Starting Fly while the car is already rolling forward gives the cleanest takeoff and keeps your heading." + Environment.NewLine + Environment.NewLine +
                        "This note will not show again this session.");
                    _flyRecommendationAcknowledged = true;
                }
                RunWorker(labelText, delegate { ApplyProfileRuntimeHook(RuntimeProfileFeature.Hover, labelText, valueBox.Text, enabled); }, apply);
            };
            parent.Controls.Add(apply);
            SetTranslatedToolTip(apply, tooltip);
            AddRuntimeFeatureHotkeyControls(parent, x, y, RuntimeProfileFeature.Hover, labelText, valueBox, toggle);
        }

        private void AddNoClipKeybindRow(Control parent, int x, int y)
        {
            const string labelText = "No Clip";
            const string tooltip = "Green arms No Clip. Hold the selected key to lock into Fly-style phase mode, then use W/A/S/D to shift through objects.";

            var label = MakeLabel(labelText, x, y + 4);
            label.Width = 190;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var key = MakeButton(GetNoClipKeyText(), x + 420, y - 2, 116, 30);
            key.Click += delegate
            {
                var selected = ShowKeybindPrompt("Set No Clip Key", "Press a key for No Clip");
                if (!selected.HasValue)
                    return;

                _noClipVirtualKey = selected.Value;
                UpdateNoClipKeyButton();
                SaveAppSettings();
                SetFeaturesStatus("No Clip key set to " + GetNoClipKeyText(), AccentPurple);
                Log("No Clip keybind set to " + GetNoClipKeyText() + ".");
            };
            parent.Controls.Add(key);
            SetTranslatedToolTip(key, "Default is V. Hold this key to lock the car into phase mode.");
            _noClipKeyButton = key;

            var toggle = new StatusDotToggle();
            toggle.Location = new Point(x + 566, y + 3);
            toggle.Size = new Size(24, 24);
            parent.Controls.Add(toggle);
            SetTranslatedToolTip(toggle, tooltip + " Green is armed. Red turns it off right away.");
            WireImmediateNoClipOff(toggle, labelText);
            _runtimeFeatureToggles[RuntimeProfileFeature.NoClip] = toggle;

            var apply = MakeButton("Apply", x + 620, y - 2, 96, 30);
            apply.Click += delegate
            {
                var enabled = toggle.Checked;
                RunWorker(labelText, delegate { ApplyNoClipRuntimeHook(enabled); }, apply);
            };
            parent.Controls.Add(apply);
            SetTranslatedToolTip(apply, tooltip);
        }

        private void AddPhaseDashKeybindRow(Control parent, int x, int y)
        {
            const string labelText = "Phase Dash";
            const string tooltip = "Green arms Phase Dash. Press the selected key to teleport the car forward by the configured distance.";

            var label = MakeLabel(labelText, x, y + 4);
            label.Width = 190;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var distance = MakeButton("Distance", x + 400, y - 2, 84, 30);
            distance.Click += delegate
            {
                var selected = ShowPhaseDashDistancePrompt(_phaseDashDistanceMeters);
                if (!selected.HasValue)
                    return;

                _phaseDashDistanceMeters = selected.Value;
                NormalizePhaseDashDistance();
                UpdatePhaseDashDistanceButton();
                SaveAppSettings();
                try
                {
                    ApplyPhaseDashRuntimeConfigIfAttached();
                    SetFeaturesStatus("Phase Dash distance set to " + FormatFloat(_phaseDashDistanceMeters) + " m", AccentPurple);
                    Log("Phase Dash distance set to " + FormatFloat(_phaseDashDistanceMeters) + " meters.");
                }
                catch (Exception ex)
                {
                    Log("Phase Dash distance saved, but live update is waiting: " + ex.Message);
                }
            };
            parent.Controls.Add(distance);
            SetTranslatedToolTip(distance, "Click to edit how many meters Phase Dash teleports forward. Default is " + FormatFloat(DefaultPhaseDashDistanceMeters) + " m.");
            _phaseDashDistanceButton = distance;
            UpdatePhaseDashDistanceButton();

            var key = MakeButton(GetPhaseDashKeyText(), x + 490, y - 2, 96, 30);
            key.Click += delegate
            {
                var selected = ShowKeybindPrompt("Set Phase Dash Key", "Press a key for Phase Dash");
                if (!selected.HasValue)
                    return;

                _phaseDashVirtualKey = selected.Value;
                UpdatePhaseDashKeyButton();
                SaveAppSettings();
                SetFeaturesStatus("Phase Dash key set to " + GetPhaseDashKeyText(), AccentPurple);
                Log("Phase Dash keybind set to " + GetPhaseDashKeyText() + ".");
            };
            parent.Controls.Add(key);
            SetTranslatedToolTip(key, "Default is H. Press this key to dash forward by the configured distance.");
            _phaseDashKeyButton = key;

            var toggle = new StatusDotToggle();
            toggle.Location = new Point(x + 604, y + 3);
            toggle.Size = new Size(24, 24);
            parent.Controls.Add(toggle);
            SetTranslatedToolTip(toggle, tooltip + " Green is armed. Red turns it off right away.");
            WireImmediatePhaseDashOff(toggle, labelText);
            _runtimeFeatureToggles[RuntimeProfileFeature.PhaseDash] = toggle;

            var apply = MakeButton("Apply", x + 646, y - 2, 78, 30);
            apply.Click += delegate
            {
                var enabled = toggle.Checked;
                RunWorker(labelText, delegate { ApplyPhaseDashRuntimeHook(enabled); }, apply);
            };
            parent.Controls.Add(apply);
            SetTranslatedToolTip(apply, tooltip);
        }

        private void AddRewindKeybindRow(Control parent, int x, int y)
        {
            const string labelText = "Rewind";
            const string tooltip = "Green keeps a rolling car-state buffer ready. Hold the selected key to scrub backward through recent driving, release to resume.";

            var label = MakeLabel(labelText, x, y + 4);
            label.Width = 190;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var config = MakeButton("Config", x + 400, y - 2, 82, 30);
            config.Click += delegate
            {
                if (!ShowRewindConfigPrompt())
                    return;

                UpdateRewindConfigButton();
                SaveAppSettings();
                try
                {
                    ApplyRewindRuntimeConfigIfAttached();
                    SetFeaturesStatus("Rewind config saved: " + BuildRewindConfigSummary(), AccentPurple);
                    Log("Rewind config saved: " + BuildRewindConfigSummary() + ".");
                }
                catch (Exception ex)
                {
                    SetFeaturesStatus("Rewind config saved locally, but live update failed: " + ex.Message, AccentOrange);
                    Log("Rewind config saved locally, but live update failed: " + ex.Message);
                }
            };
            parent.Controls.Add(config);
            SetTranslatedToolTip(config, "Open Rewind buffer and scrub speed values. Changes apply to the active hook after saving.");
            _rewindConfigButton = config;

            var key = MakeButton(GetRewindKeyText(), x + 490, y - 2, 96, 30);
            key.Click += delegate
            {
                var selected = ShowKeybindPrompt("Set Rewind Key", "Press a key for Rewind");
                if (!selected.HasValue)
                    return;

                _rewindVirtualKey = selected.Value;
                UpdateRewindKeyButton();
                SaveAppSettings();
                SetFeaturesStatus("Rewind key set to " + GetRewindKeyText(), AccentPurple);
                Log("Rewind keybind set to " + GetRewindKeyText() + ".");
            };
            parent.Controls.Add(key);
            SetTranslatedToolTip(key, "Default is R. Hold this key to scrub backward through the rolling Rewind buffer.");
            _rewindKeyButton = key;

            var toggle = new StatusDotToggle();
            toggle.Location = new Point(x + 604, y + 3);
            toggle.Size = new Size(24, 24);
            parent.Controls.Add(toggle);
            SetTranslatedToolTip(toggle, tooltip + " Green is armed and recording. Red stops the buffer.");
            WireImmediateRuntimeOff(toggle, RuntimeProfileFeature.Rewind, labelText);
            _runtimeFeatureToggles[RuntimeProfileFeature.Rewind] = toggle;

            var apply = MakeButton("Apply", x + 646, y - 2, 96, 30);
            apply.Click += delegate
            {
                var enabled = toggle.Checked;
                RunWorker(labelText, delegate { ApplyRewindRuntimeHook(enabled); }, apply);
            };
            parent.Controls.Add(apply);
            SetTranslatedToolTip(apply, tooltip);
        }

        private void AddGrapplingHookKeybindRow(Control parent, int x, int y)
        {
            const string labelText = "Grappling Hook";
            const string tooltip = "Green arms Grappling Hook. Aim with the camera, hold the selected key to pull toward that point, release to keep the exit momentum.";

            var label = MakeLabel(labelText, x, y + 4);
            label.Width = 190;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var config = MakeButton("Config", x + 400, y - 2, 82, 30);
            config.Click += delegate
            {
                if (!ShowGrapplingHookConfigPrompt())
                    return;

                UpdateGrapplingHookConfigButton();
                SaveAppSettings();
                try
                {
                    ApplyGrapplingHookRuntimeConfigIfAttached();
                    SetFeaturesStatus("Grappling Hook config saved: " + BuildGrapplingHookConfigSummary(), AccentPurple);
                    Log("Grappling Hook config saved: " + BuildGrapplingHookConfigSummary() + ".");
                }
                catch (Exception ex)
                {
                    SetFeaturesStatus("Grappling Hook config saved locally, but live update failed: " + ex.Message, AccentOrange);
                    Log("Grappling Hook config saved locally, but live update failed: " + ex.Message);
                }
            };
            parent.Controls.Add(config);
            SetTranslatedToolTip(config, "Open Grappling Hook pull values. Changes apply to the active hook after saving.");
            _grapplingHookConfigButton = config;

            var key = MakeButton(GetGrapplingHookKeyText(), x + 490, y - 2, 96, 30);
            key.Click += delegate
            {
                var selected = ShowKeybindPrompt("Set Grappling Hook Key", "Press a key for Grappling Hook");
                if (!selected.HasValue)
                    return;

                _grapplingHookVirtualKey = selected.Value;
                UpdateGrapplingHookKeyButton();
                SaveAppSettings();
                SetFeaturesStatus("Grappling Hook key set to " + GetGrapplingHookKeyText(), AccentPurple);
                Log("Grappling Hook keybind set to " + GetGrapplingHookKeyText() + ".");
            };
            parent.Controls.Add(key);
            SetTranslatedToolTip(key, "Default is Q. Hold this key to pull toward the camera aim point.");
            _grapplingHookKeyButton = key;

            var toggle = new StatusDotToggle();
            toggle.Location = new Point(x + 604, y + 3);
            toggle.Size = new Size(24, 24);
            parent.Controls.Add(toggle);
            SetTranslatedToolTip(toggle, tooltip + " Green is armed. Red turns it off right away.");
            WireImmediateRuntimeOff(toggle, RuntimeProfileFeature.GrapplingHook, labelText);
            _runtimeFeatureToggles[RuntimeProfileFeature.GrapplingHook] = toggle;

            var apply = MakeButton("Apply", x + 646, y - 2, 96, 30);
            apply.Click += delegate
            {
                var enabled = toggle.Checked;
                RunWorker(labelText, delegate { ApplyGrapplingHookRuntimeHook(enabled); }, apply);
            };
            parent.Controls.Add(apply);
            SetTranslatedToolTip(apply, tooltip);
        }

        private void AddSuperStrengthKeybindRow(Control parent, int x, int y)
        {
            const string labelText = "Super Strength";
            const string tooltip = "Green arms Super Strength. Hold the selected key in-game to stabilize the car and shove forward through anything in the way.";

            var label = MakeLabel(labelText, x, y + 4);
            label.Width = 190;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var config = MakeButton("Config", x + 400, y - 2, 82, 30);
            config.Click += delegate
            {
                if (!ShowSuperStrengthConfigPrompt())
                    return;

                UpdateSuperStrengthConfigButton();
                SaveAppSettings();
                try
                {
                    ApplySuperStrengthRuntimeConfigIfAttached();
                    SetFeaturesStatus("Super Strength config saved: " + BuildSuperStrengthConfigSummary(), AccentPurple);
                    Log("Super Strength config saved: " + BuildSuperStrengthConfigSummary() + ".");
                }
                catch (Exception ex)
                {
                    SetFeaturesStatus("Super Strength config saved locally, but live update failed: " + ex.Message, AccentOrange);
                    Log("Super Strength config saved locally, but live update failed: " + ex.Message);
                }
            };
            parent.Controls.Add(config);
            SetTranslatedToolTip(config, "Open Super Strength values. Changes apply to the active hook after saving.");
            _superStrengthConfigButton = config;

            var key = MakeButton(GetSuperStrengthKeyText(), x + 490, y - 2, 96, 30);
            key.Click += delegate
            {
                var selected = ShowKeybindPrompt("Set Super Strength Key", "Press a key for Super Strength");
                if (!selected.HasValue)
                    return;

                _superStrengthVirtualKey = selected.Value;
                UpdateSuperStrengthKeyButton();
                SaveAppSettings();
                SetFeaturesStatus("Super Strength key set to " + GetSuperStrengthKeyText(), AccentPurple);
                Log("Super Strength keybind set to " + GetSuperStrengthKeyText() + ".");
            };
            parent.Controls.Add(key);
            SetTranslatedToolTip(key, "Default is G. Hold this key to apply controlled unstoppable shove.");
            _superStrengthKeyButton = key;

            var toggle = new StatusDotToggle();
            toggle.Location = new Point(x + 604, y + 3);
            toggle.Size = new Size(24, 24);
            parent.Controls.Add(toggle);
            SetTranslatedToolTip(toggle, tooltip + " Green is armed. Red turns it off right away.");
            WireImmediateSuperStrengthOff(toggle, labelText);
            _runtimeFeatureToggles[RuntimeProfileFeature.SuperStrength] = toggle;

            var apply = MakeButton("Apply", x + 646, y - 2, 96, 30);
            apply.Click += delegate
            {
                var enabled = toggle.Checked;
                RunWorker(labelText, delegate { ApplySuperStrengthRuntimeHook(enabled); }, apply);
            };
            parent.Controls.Add(apply);
            SetTranslatedToolTip(apply, tooltip);
        }

        private void AddTeleportWaypointKeybindRow(Control parent, int x, int y)
        {
            AddTeleportWaypointKeybindRow(parent, x, y, false);
        }

        private void AddTeleportWaypointKeybindRow(Control parent, int x, int y, bool compact)
        {
            const string labelText = "Teleport To Waypoint";
            const string tooltip = "Green arms waypoint teleport. Turn it on, set or refresh a waypoint once, then press the selected key to move the car there.";

            var label = MakeLabel(labelText, x, y + 4);
            label.Width = compact ? 190 : 190;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var keyX = compact ? x + 208 : x + 420;
            var toggleX = compact ? x + 354 : x + 566;
            var applyX = compact ? x + 400 : x + 620;
            var hintX = compact ? x + 516 : -1;

            var key = MakeButton(GetTeleportWaypointKeyText(), keyX, y - 2, 116, 30);
            key.Click += delegate
            {
                var selected = ShowKeybindPrompt("Set Waypoint Key", "Press a key for Teleport To Waypoint");
                if (!selected.HasValue)
                    return;

                _teleportWaypointVirtualKey = selected.Value;
                UpdateTeleportWaypointKeyButton();
                SaveAppSettings();
                SetFeaturesStatus("Waypoint key set to " + GetTeleportWaypointKeyText(), AccentPurple);
                Log("Teleport To Waypoint keybind set to " + GetTeleportWaypointKeyText() + ".");
                if (_teleportWaypointToggle != null && _teleportWaypointToggle.Checked)
                    RunWorker(labelText, delegate { ApplyTeleportWaypointRuntimeHook(true); });
            };
            parent.Controls.Add(key);
            SetTranslatedToolTip(key, "Default is Y. This key is independent from saved-location teleport.");
            _teleportWaypointKeyButton = key;

            var toggle = new StatusDotToggle();
            toggle.Location = new Point(toggleX, y + 4);
            toggle.Size = new Size(40, 22);
            parent.Controls.Add(toggle);
            SetTranslatedToolTip(toggle, tooltip + " Green is on. Red turns it off right away.");
            _teleportWaypointToggle = toggle;
            WireImmediateTeleportOff(toggle, labelText);

            var apply = MakeButton("Apply", applyX, y - 2, 96, 30);
            apply.Click += delegate
            {
                var enabled = toggle.Checked;
                RunWorker(labelText, delegate { ApplyTeleportWaypointRuntimeHook(enabled); }, apply);
            };
            parent.Controls.Add(apply);
            SetTranslatedToolTip(apply, tooltip);

            if (compact)
            {
                var hint = MakeBodyLabel("Use a map waypoint, then press this key in game.", hintX, y + 1, 328, 26);
                hint.BackColor = Surface;
                hint.ForeColor = TextMuted;
                parent.Controls.Add(hint);
            }
        }

        private void AddTeleportCheckpointKeybindRow(Control parent, int x, int y)
        {
            AddTeleportCheckpointKeybindRow(parent, x, y, false);
        }

        private void AddCheckpointRecoveryKeybindRow(Control parent, int x, int y)
        {
            AddCheckpointRecoveryKeybindRow(parent, x, y, false);
        }

        private void AddCheckpointRecoveryKeybindRow(Control parent, int x, int y, bool compact)
        {
            const string labelText = "Recovery Path";
            const string tooltip = "Green arms Recovery Path. Press the key in-game to teleport once straight to the active route checkpoint.";

            var label = MakeLabel(labelText, x, y + 4);
            label.Width = compact ? 190 : 190;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var keyX = compact ? x + 208 : x + 420;
            var toggleX = x + 566;
            var applyX = compact ? x + 400 : x + 620;

            var key = MakeButton(GetCheckpointRecoveryKeyText(), keyX, y - 2, 116, 30);
            key.Click += delegate
            {
                var selected = ShowKeybindPrompt("Set Recovery Path Key", "Press a key for Recovery Path");
                if (!selected.HasValue)
                    return;

                _checkpointRecoveryVirtualKey = selected.Value;
                UpdateCheckpointRecoveryKeyButton();
                SaveAppSettings();
                SetFeaturesStatus("Recovery Path key set to " + GetCheckpointRecoveryKeyText(), AccentPurple);
                Log("Recovery Path keybind set to " + GetCheckpointRecoveryKeyText() + ".");
                if (_checkpointRecoveryToggle != null && _checkpointRecoveryToggle.Checked)
                    RunWorker(labelText, delegate { ApplyCheckpointRecoveryRuntimeHook(true); });
            };
            parent.Controls.Add(key);
            SetTranslatedToolTip(key, "Default is K. In-game this key teleports once to the current active route checkpoint.");
            _checkpointRecoveryKeyButton = key;

            var toggle = new StatusDotToggle();
            toggle.Location = new Point(toggleX, y + 4);
            toggle.Size = new Size(40, 22);
            parent.Controls.Add(toggle);
            SetTranslatedToolTip(toggle, tooltip + " Green is on. Red turns it off right away.");
            _checkpointRecoveryToggle = toggle;
            WireImmediateCheckpointRecoveryOff(toggle, labelText);

            var apply = MakeButton("Apply", applyX, y - 2, 96, 30);
            apply.Click += delegate
            {
                var enabled = toggle.Checked;
                RunWorker(labelText, delegate { ApplyCheckpointRecoveryRuntimeHook(enabled); }, apply);
            };
            parent.Controls.Add(apply);
            SetTranslatedToolTip(apply, tooltip);

            if (compact)
            {
                var hint = MakeBodyLabel("Press once in-game to snap to the active checkpoint.", applyX + 106, y + 1, 270, 26);
                hint.BackColor = Surface;
                hint.ForeColor = TextMuted;
                parent.Controls.Add(hint);
            }
        }

        private void AddTeleportCheckpointKeybindRow(Control parent, int x, int y, bool compact)
        {
            const string labelText = "Auto Race (Teleport)";
            const string tooltip = "Green arms Auto Race (Teleport). In-game the key is a toggle: press once to start teleporting along the active route checkpoints, press again to stop.";

            var label = MakeLabel(labelText, x, y + 4);
            label.Width = compact ? 190 : 190;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var keyX = compact ? x + 208 : x + 420;
            var toggleX = x + 566;
            var applyX = compact ? x + 400 : x + 620;
            var configX = compact ? x + 516 : x + 730;

            var key = MakeButton(GetTeleportCheckpointKeyText(), keyX, y - 2, 116, 30);
            key.Click += delegate
            {
                var selected = ShowKeybindPrompt("Set Auto Race Key", "Press a key for Auto Race");
                if (!selected.HasValue)
                    return;

                _teleportCheckpointVirtualKey = selected.Value;
                UpdateTeleportCheckpointKeyButton();
                SaveAppSettings();
                SetFeaturesStatus("Auto Race key set to " + GetTeleportCheckpointKeyText(), AccentPurple);
                Log("Auto Race keybind set to " + GetTeleportCheckpointKeyText() + ".");
                if (_teleportCheckpointToggle != null && _teleportCheckpointToggle.Checked)
                    RunWorker(labelText, delegate { ApplyTeleportCheckpointRuntimeHook(true); });
            };
            parent.Controls.Add(key);
            SetTranslatedToolTip(key, "Default is U. In-game this key toggles Auto Race on and off. Independent from saved-location and waypoint teleport.");
            _teleportCheckpointKeyButton = key;

            var toggle = new StatusDotToggle();
            toggle.Location = new Point(toggleX, y + 4);
            toggle.Size = new Size(40, 22);
            parent.Controls.Add(toggle);
            SetTranslatedToolTip(toggle, tooltip + " Green is on. Red turns it off right away.");
            _teleportCheckpointToggle = toggle;
            WireImmediateTeleportCheckpointOff(toggle, labelText);

            var apply = MakeButton("Apply", applyX, y - 2, 96, 30);
            apply.Click += delegate
            {
                var enabled = toggle.Checked;
                RunWorker(labelText, delegate { ApplyTeleportCheckpointRuntimeHook(enabled); }, apply);
            };
            parent.Controls.Add(apply);
            SetTranslatedToolTip(apply, tooltip);

            var config = MakeButton("Config", configX, y - 2, 90, 30);
            config.Click += delegate { ShowAutoRaceConfigPrompt(); };
            parent.Controls.Add(config);
            SetTranslatedToolTip(config, "Adjust how often Auto Race teleports to the next checkpoint (default 0.5s).");

            if (compact)
            {
                var hint = MakeBodyLabel("Press the key in-game to start/stop. Config sets the interval.", configX + 98, y + 1, 250, 26);
                hint.BackColor = Surface;
                hint.ForeColor = TextMuted;
                parent.Controls.Add(hint);
            }
        }

        private void AddAutoRaceDriveKeybindRow(Control parent, int x, int y)
        {
            AddAutoRaceDriveKeybindRow(parent, x, y, false);
        }

        private void AddAutoRaceDriveKeybindRow(Control parent, int x, int y, bool compact)
        {
            const string labelText = "Auto Race (Drive)";
            const string tooltip = "Green arms Auto Race (Drive). In-game the key is a toggle: press once to physically auto-drive the active route line, press again to stop. Bundles Acceleration, Adaptive Brake 0.92, No Water Drag, Super Handling, Landing Stabilizer, and Slide Calmer.";

            var label = MakeLabel(labelText, x, y + 4);
            label.Width = compact ? 190 : 190;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var keyX = compact ? x + 208 : x + 420;
            var toggleX = x + 566;
            var applyX = compact ? x + 400 : x + 620;
            var configX = compact ? x + 516 : x + 730;

            var key = MakeButton(GetAutoRaceDriveKeyText(), keyX, y - 2, 116, 30);
            key.Click += delegate
            {
                var selected = ShowKeybindPrompt("Set Auto Race (Drive) Key", "Press a key for Auto Race (Drive)");
                if (!selected.HasValue)
                    return;

                _autoRaceDriveVirtualKey = selected.Value;
                UpdateAutoRaceDriveKeyButton();
                SaveAppSettings();
                SetFeaturesStatus("Auto Race (Drive) key set to " + GetAutoRaceDriveKeyText(), AccentPurple);
                Log("Auto Race (Drive) keybind set to " + GetAutoRaceDriveKeyText() + ".");
                if (_autoRaceDriveToggle != null && _autoRaceDriveToggle.Checked)
                    RunWorker(labelText, delegate { ApplyAutoRaceDriveRuntimeHook(true); });
            };
            parent.Controls.Add(key);
            SetTranslatedToolTip(key, "Default is I. In-game this key toggles Auto Race (Drive) on and off. Independent from the teleport variants.");
            _autoRaceDriveKeyButton = key;

            var toggle = new StatusDotToggle();
            toggle.Location = new Point(toggleX, y + 4);
            toggle.Size = new Size(40, 22);
            parent.Controls.Add(toggle);
            SetTranslatedToolTip(toggle, tooltip + " Green is on. Red turns it off right away.");
            _autoRaceDriveToggle = toggle;
            WireImmediateAutoRaceDriveOff(toggle, labelText);

            var apply = MakeButton("Apply", applyX, y - 2, 96, 30);
            apply.Click += delegate
            {
                var enabled = toggle.Checked;
                RunWorker(labelText, delegate { ApplyAutoRaceDriveRuntimeHook(enabled); }, apply);
            };
            parent.Controls.Add(apply);
            SetTranslatedToolTip(apply, tooltip);

            var config = MakeButton("Config", configX, y - 2, 90, 30);
            config.Click += delegate { ShowAutoRaceDriveConfigPrompt(); };
            parent.Controls.Add(config);
            SetTranslatedToolTip(config, "Tune Auto Race (Drive): cruise/top speed, acceleration, steering, and the bundled performance hooks.");

            if (compact)
            {
                var hint = MakeBodyLabel("Press the key in-game to start/stop auto-driving the line.", configX + 98, y + 1, 250, 26);
                hint.BackColor = Surface;
                hint.ForeColor = TextMuted;
                parent.Controls.Add(hint);
            }
        }

        private void WireImmediateAutoRaceDriveOff(StatusDotToggle toggle, string labelText)
        {
            if (toggle == null)
                return;

            toggle.CheckedChanged += delegate
            {
                RequestFeatureOverlayRefresh();
                if (toggle.Checked)
                {
                    RunWorker(labelText + " On", delegate { ApplyAutoRaceDriveRuntimeHook(true); });
                    return;
                }
                if (_database == null || !_database.IsAlive)
                    return;

                RunWorker(labelText + " Off", delegate { ApplyAutoRaceDriveRuntimeHook(false); });
            };
        }

        private void WireImmediateCheckpointRecoveryOff(StatusDotToggle toggle, string labelText)
        {
            if (toggle == null)
                return;

            toggle.CheckedChanged += delegate
            {
                RequestFeatureOverlayRefresh();
                if (toggle.Checked)
                {
                    RunWorker(labelText + " On", delegate { ApplyCheckpointRecoveryRuntimeHook(true); });
                    return;
                }
                if (_database == null || !_database.IsAlive)
                    return;

                RunWorker(labelText + " Off", delegate { ApplyCheckpointRecoveryRuntimeHook(false); });
            };
        }

        private void ShowAutoRaceDriveConfigPrompt()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowAutoRaceDriveConfigPrompt);
                return;
            }

            using (var dialog = new Form())
            {
                dialog.Text = "Auto Race (Drive) Config";
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.ClientSize = new Size(560, 596);
                dialog.BackColor = AppBackground;
                dialog.ForeColor = TextPrimary;
                dialog.Font = new Font("Segoe UI", 9F);

                var title = new Label();
                title.Text = "Auto Race (Drive)";
                title.Location = new Point(18, 14);
                title.Size = new Size(524, 24);
                title.Font = new Font("Segoe UI Semibold", 11F);
                title.ForeColor = TextPrimary;
                title.BackColor = AppBackground;
                dialog.Controls.Add(title);

                var body = new Label();
                body.Text = "Tune how Auto Race (Drive) drives the line. Tick the box to enable an option; unticked options fall back to their default (and that hook is not armed). Speeds are in km/h.";
                body.Location = new Point(18, 42);
                body.Size = new Size(524, 38);
                body.ForeColor = TextMuted;
                body.BackColor = AppBackground;
                dialog.Controls.Add(body);

                Func<int, string, string, string, bool, KeyValuePair<CheckBox, TextBox>> addField =
                    delegate(int top, string labelText, string valueText, string hintText, bool optionOn)
                {
                    var cb = new CheckBox();
                    cb.Checked = optionOn;
                    cb.Location = new Point(18, top + 3);
                    cb.Size = new Size(18, 20);
                    cb.ForeColor = TextPrimary;
                    cb.BackColor = AppBackground;
                    dialog.Controls.Add(cb);

                    var lbl = new Label();
                    lbl.Text = labelText;
                    lbl.Location = new Point(42, top + 2);
                    lbl.Size = new Size(150, 24);
                    lbl.ForeColor = TextPrimary;
                    lbl.BackColor = AppBackground;
                    lbl.Font = new Font("Segoe UI Semibold", 9F);
                    dialog.Controls.Add(lbl);

                    var tb = new TextBox();
                    tb.Text = valueText;
                    tb.Location = new Point(198, top);
                    tb.Size = new Size(76, 28);
                    tb.TextAlign = HorizontalAlignment.Right;
                    tb.Enabled = optionOn;
                    StyleTextBox(tb);
                    dialog.Controls.Add(tb);

                    cb.CheckedChanged += delegate { tb.Enabled = cb.Checked; };

                    var hint = MakeBodyLabel(hintText, 282, top + 2, 266, 24);
                    hint.BackColor = AppBackground;
                    hint.ForeColor = TextMuted;
                    dialog.Controls.Add(hint);
                    return new KeyValuePair<CheckBox, TextBox>(cb, tb);
                };

                var cruiseRow = addField(88, "Cruise speed (km/h)", _autoRaceDriveCruiseKmh.ToString(CultureInfo.InvariantCulture), "Target speed.", _autoRaceDriveCruiseEnabled);
                var topRow = addField(122, "Top speed (km/h)", _autoRaceDriveTopSpeedKmh.ToString(CultureInfo.InvariantCulture), "Hard cap.", _autoRaceDriveTopSpeedEnabled);
                var accelRow = addField(156, "Acceleration ramp", _autoRaceDriveAccelStrength.ToString(CultureInfo.InvariantCulture), "How fast it reaches cruise.", _autoRaceDriveAccelStrengthEnabled);
                var steerRow = addField(190, "Steering strength", FormatFloat(_autoRaceDriveSteerStrength), "How sharply it holds the line.", _autoRaceDriveSteerEnabled);
                var accelMultRow = addField(224, "Acceleration x", FormatFloat(_autoRaceDriveAccelMultiplier), "Gentle on-top increase.", _autoRaceDriveAccelMultEnabled);
                var handlingRow = addField(258, "Super Handling", FormatFloat(_autoRaceDriveSuperHandling), "Downforce/grip.", _autoRaceDriveSuperHandlingEnabled);
                var slideRow = addField(292, "Slide Calmer", FormatFloat(_autoRaceDriveSlideCalmer), "Anti-slide downforce.", _autoRaceDriveSlideCalmerEnabled);
                var landingRow = addField(326, "Landing Stabilizer", FormatFloat(_autoRaceDriveLandingStabilizer), "Bounce damping.", _autoRaceDriveLandingStabilizerEnabled);
                var brakeRow = addField(360, "Adaptive Brake", FormatFloat(_autoRaceDriveAdaptiveBrake), "Manual-brake strength.", _autoRaceDriveAdaptiveBrakeEnabled);
                var scanRow = addField(394, "Route refresh (ms)", _autoRaceDriveScanIntervalMs.ToString(CultureInfo.InvariantCulture), "How often it re-reads the route. Lower = faster/more accurate.", _autoRaceDriveScanEnabled);
                var offRow = addField(428, "Off-Waypoint Amount", FormatFloat(_autoRaceDriveOffWaypointMeters), "Metres to aim outside of bends so tight-turn checkpoints aren't missed.", _autoRaceDriveOffWaypointEnabled);

                var cruiseBox = cruiseRow.Value;
                var topBox = topRow.Value;
                var accelBox = accelRow.Value;
                var steerBox = steerRow.Value;
                var accelMultBox = accelMultRow.Value;
                var handlingBox = handlingRow.Value;
                var slideBox = slideRow.Value;
                var landingBox = landingRow.Value;
                var brakeBox = brakeRow.Value;
                var scanBox = scanRow.Value;
                var offBox = offRow.Value;

                var noWater = new CheckBox();
                noWater.Text = "No Water Drag (skip water slowdown)";
                noWater.Checked = _autoRaceDriveNoWaterDrag;
                noWater.Location = new Point(18, 474);
                noWater.Size = new Size(524, 24);
                noWater.ForeColor = TextPrimary;
                noWater.BackColor = AppBackground;
                dialog.Controls.Add(noWater);

                var save = MakeButton("Save", 446, 544, 96, 30);
                MakeAccentButton(save, AccentBlue);
                save.Click += delegate
                {
                    int cruise, top, accel, scan;
                    float steer, accelMult, handling, slide, landing, brake, offWaypoint;
                    if (!int.TryParse(cruiseBox.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out cruise) ||
                        !int.TryParse(topBox.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out top) ||
                        !int.TryParse(accelBox.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out accel) ||
                        !float.TryParse(steerBox.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out steer) ||
                        !float.TryParse(accelMultBox.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out accelMult) ||
                        !float.TryParse(handlingBox.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out handling) ||
                        !float.TryParse(slideBox.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out slide) ||
                        !float.TryParse(landingBox.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out landing) ||
                        !float.TryParse(brakeBox.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out brake) ||
                        !int.TryParse(scanBox.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out scan) ||
                        !float.TryParse(offBox.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out offWaypoint))
                    {
                        ShowTranslatedMessageBox(dialog, "Every field must be a number.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    _autoRaceDriveCruiseKmh = cruise;
                    _autoRaceDriveTopSpeedKmh = top;
                    _autoRaceDriveAccelStrength = accel;
                    _autoRaceDriveSteerStrength = steer;
                    _autoRaceDriveAccelMultiplier = accelMult;
                    _autoRaceDriveSuperHandling = handling;
                    _autoRaceDriveSlideCalmer = slide;
                    _autoRaceDriveLandingStabilizer = landing;
                    _autoRaceDriveAdaptiveBrake = brake;
                    _autoRaceDriveScanIntervalMs = scan;
                    _autoRaceDriveOffWaypointMeters = offWaypoint;
                    _autoRaceDriveNoWaterDrag = noWater.Checked;

                    _autoRaceDriveCruiseEnabled = cruiseRow.Key.Checked;
                    _autoRaceDriveTopSpeedEnabled = topRow.Key.Checked;
                    _autoRaceDriveAccelStrengthEnabled = accelRow.Key.Checked;
                    _autoRaceDriveSteerEnabled = steerRow.Key.Checked;
                    _autoRaceDriveAccelMultEnabled = accelMultRow.Key.Checked;
                    _autoRaceDriveSuperHandlingEnabled = handlingRow.Key.Checked;
                    _autoRaceDriveSlideCalmerEnabled = slideRow.Key.Checked;
                    _autoRaceDriveLandingStabilizerEnabled = landingRow.Key.Checked;
                    _autoRaceDriveAdaptiveBrakeEnabled = brakeRow.Key.Checked;
                    _autoRaceDriveScanEnabled = scanRow.Key.Checked;
                    _autoRaceDriveOffWaypointEnabled = offRow.Key.Checked;
                    SaveAppSettings();

                    if (_database != null && _database.IsAlive)
                    {
                        PushAutoRaceDriveConfig(_database);
                        if (_autoRaceDriveToggle != null && _autoRaceDriveToggle.Checked)
                            RunWorker("Auto Race (Drive)", delegate { ApplyAutoRaceDriveRuntimeHook(true); });
                    }

                    SetFeaturesStatus("Auto Race (Drive) config saved", AccentPurple);
                    Log("Auto Race (Drive) config: cruise " + cruise + " km/h, top " + top + " km/h, ramp " + accel + ", steering " + FormatFloat(steer) +
                        ", Acceleration x" + FormatFloat(accelMult) + ", route refresh " + scan + " ms, off-waypoint " + (offRow.Key.Checked ? FormatFloat(offWaypoint) + " m" : "off") + ".");
                    dialog.DialogResult = DialogResult.OK;
                    dialog.Close();
                };
                dialog.Controls.Add(save);

                var cancel = MakeButton("Cancel", 342, 544, 96, 30);
                cancel.Click += delegate { dialog.DialogResult = DialogResult.Cancel; dialog.Close(); };
                dialog.Controls.Add(cancel);

                var reset = MakeButton("Defaults", 18, 544, 96, 30);
                reset.Click += delegate
                {
                    cruiseBox.Text = "450";
                    topBox.Text = "612";
                    accelBox.Text = "130";
                    steerBox.Text = "4.6";
                    accelMultBox.Text = "1.001";
                    handlingBox.Text = "0.24";
                    slideBox.Text = "0.18";
                    landingBox.Text = "1";
                    brakeBox.Text = "0.92";
                    scanBox.Text = "50";
                    offBox.Text = "5";
                    noWater.Checked = true;
                    cruiseRow.Key.Checked = true;
                    topRow.Key.Checked = true;
                    accelRow.Key.Checked = true;
                    steerRow.Key.Checked = true;
                    accelMultRow.Key.Checked = true;
                    handlingRow.Key.Checked = true;
                    slideRow.Key.Checked = true;
                    landingRow.Key.Checked = true;
                    brakeRow.Key.Checked = true;
                    scanRow.Key.Checked = true;
                    offRow.Key.Checked = false;
                };
                dialog.Controls.Add(reset);
                SetTranslatedToolTip(reset, "Restore the default Auto Race (Drive) values and enable every option.");

                dialog.AcceptButton = save;
                dialog.CancelButton = cancel;
                PrepareDialogForLanguage(dialog);
                dialog.ShowDialog(this);
            }
        }

        private void ShowAutoRaceConfigPrompt()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowAutoRaceConfigPrompt);
                return;
            }

            using (var dialog = new Form())
            {
                dialog.Text = "Auto Race Config";
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.ClientSize = new Size(540, 360);
                dialog.BackColor = AppBackground;
                dialog.ForeColor = TextPrimary;
                dialog.Font = new Font("Segoe UI", 9F);

                var title = new Label();
                title.Text = "Auto Race";
                title.Location = new Point(18, 16);
                title.Size = new Size(504, 24);
                title.Font = new Font("Segoe UI Semibold", 11F);
                title.ForeColor = TextPrimary;
                title.BackColor = AppBackground;
                dialog.Controls.Add(title);

                var body = new Label();
                body.Text = "While Auto Race runs it keeps teleporting your car to the next route checkpoint. Defaults match the original feel; everything below is optional.";
                body.Location = new Point(18, 46);
                body.Size = new Size(504, 40);
                body.ForeColor = TextMuted;
                body.BackColor = AppBackground;
                dialog.Controls.Add(body);

                Func<int, string, string, TextBox> addField = delegate(int top, string labelText, string valueText)
                {
                    var lbl = new Label();
                    lbl.Text = labelText;
                    lbl.Location = new Point(18, top + 2);
                    lbl.Size = new Size(150, 24);
                    lbl.ForeColor = TextPrimary;
                    lbl.BackColor = AppBackground;
                    lbl.Font = new Font("Segoe UI Semibold", 9F);
                    dialog.Controls.Add(lbl);

                    var tb = new TextBox();
                    tb.Text = valueText;
                    tb.Location = new Point(176, top);
                    tb.Size = new Size(110, 28);
                    tb.TextAlign = HorizontalAlignment.Right;
                    StyleTextBox(tb);
                    dialog.Controls.Add(tb);
                    return tb;
                };

                var intervalBox = addField(96, "Interval (s)", (_autoRaceIntervalMs / 1000.0).ToString("0.##", CultureInfo.InvariantCulture));
                var intervalHint = MakeBodyLabel("How often it jumps. Default 0.5. Range 0.1 - 5.", 296, 98, 230, 24);
                intervalHint.BackColor = AppBackground; intervalHint.ForeColor = TextMuted;
                dialog.Controls.Add(intervalHint);

                var holdBox = addField(140, "Hold time (s)", (_autoRaceHoldMs / 1000.0).ToString("0.##", CultureInfo.InvariantCulture));
                var holdHint = MakeBodyLabel("How long each jump pins the car. Default 0.65. Range 0.05 - 3.", 296, 142, 230, 36);
                holdHint.BackColor = AppBackground; holdHint.ForeColor = TextMuted;
                dialog.Controls.Add(holdHint);

                var resetSpeed = new CheckBox();
                resetSpeed.Text = "Reset speed on each teleport (stop the car)";
                resetSpeed.Checked = _autoRaceResetSpeed;
                resetSpeed.Location = new Point(18, 196);
                resetSpeed.Size = new Size(504, 24);
                resetSpeed.ForeColor = TextPrimary;
                resetSpeed.BackColor = AppBackground;
                dialog.Controls.Add(resetSpeed);

                var resetHint = MakeBodyLabel("On (default) zeros your speed each jump. Off keeps momentum for a smoother run.", 18, 222, 504, 36);
                resetHint.BackColor = AppBackground; resetHint.ForeColor = TextMuted;
                dialog.Controls.Add(resetHint);

                var save = MakeButton("Save", 426, 308, 96, 30);
                MakeAccentButton(save, AccentBlue);
                save.Click += delegate
                {
                    float intervalSeconds;
                    if (!float.TryParse(intervalBox.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out intervalSeconds) || intervalSeconds < 0.1F || intervalSeconds > 5F)
                    {
                        ShowTranslatedMessageBox(dialog, "Enter an interval between 0.1 and 5 seconds.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    float holdSeconds;
                    if (!float.TryParse(holdBox.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out holdSeconds) || holdSeconds < 0.05F || holdSeconds > 3F)
                    {
                        ShowTranslatedMessageBox(dialog, "Enter a hold time between 0.05 and 3 seconds.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    _autoRaceIntervalMs = Math.Max(100, Math.Min(5000, (int)Math.Round(intervalSeconds * 1000F)));
                    _autoRaceHoldMs = Math.Max(50, Math.Min(3000, (int)Math.Round(holdSeconds * 1000F)));
                    _autoRaceResetSpeed = resetSpeed.Checked;
                    SaveAppSettings();
                    if (_database != null && _database.IsAlive)
                        PushAutoRaceConfig(_database);
                    SetFeaturesStatus("Auto Race config saved", AccentPurple);
                    Log("Auto Race config: interval " + _autoRaceIntervalMs.ToString(CultureInfo.InvariantCulture) + " ms, hold " +
                        _autoRaceHoldMs.ToString(CultureInfo.InvariantCulture) + " ms, reset speed " + (_autoRaceResetSpeed ? "on" : "off") + ".");
                    dialog.DialogResult = DialogResult.OK;
                    dialog.Close();
                };
                dialog.Controls.Add(save);

                var cancel = MakeButton("Cancel", 322, 308, 96, 30);
                cancel.Click += delegate { dialog.DialogResult = DialogResult.Cancel; dialog.Close(); };
                dialog.Controls.Add(cancel);

                var reset = MakeButton("Defaults", 18, 308, 96, 30);
                reset.Click += delegate
                {
                    intervalBox.Text = "0.5";
                    holdBox.Text = "0.65";
                    resetSpeed.Checked = true;
                };
                dialog.Controls.Add(reset);
                SetTranslatedToolTip(reset, "Restore the original Auto Race values.");

                dialog.AcceptButton = save;
                dialog.CancelButton = cancel;
                PrepareDialogForLanguage(dialog);
                dialog.ShowDialog(this);
            }
        }

        private void AddDriftModeKeybindRow(Control parent, int x, int y)
        {
            const string labelText = "Drift Mode";
            const string tooltip = "Green arms held drift assist. Hold the selected key while turning to add a smooth controlled slide, then release to stop.";

            var label = MakeLabel(labelText, x, y + 4);
            label.Width = 190;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var value = MakeButton(FormatFloat(_driftModeForce), x + 420, y - 2, 62, 30);
            value.Click += delegate
            {
                var custom = ShowDriftModeForcePrompt(_driftModeForce);
                if (!custom.HasValue)
                    return;

                _driftModeForce = custom.Value;
                UpdateDriftModeValueButton();
                SetFeaturesStatus("Drift Mode amount set to " + FormatFloat(_driftModeForce), AccentPurple);
                Log("Drift Mode amount set to " + FormatFloat(_driftModeForce) + ".");
            };
            parent.Controls.Add(value);
            SetTranslatedToolTip(value, "Click to edit the max drift strength. Your steering chooses the slide direction. Default is 0.035.");
            _driftModeValueButton = value;

            var key = MakeButton(GetDriftModeKeyText(), x + 488, y - 2, 58, 30);
            key.Click += delegate
            {
                var selected = ShowKeybindPrompt("Set Drift Key", "Press a key for Drift Mode");
                if (!selected.HasValue)
                    return;

                _driftModeVirtualKey = selected.Value;
                UpdateDriftModeKeyButton();
                SetFeaturesStatus("Drift Mode key set to " + GetDriftModeKeyText(), AccentPurple);
                Log("Drift Mode keybind set to " + GetDriftModeKeyText() + ".");
            };
            parent.Controls.Add(key);
            SetTranslatedToolTip(key, "Default is X. Click to choose another drift key.");
            _driftModeKeyButton = key;

            var toggle = new StatusDotToggle();
            toggle.Location = new Point(x + 566, y + 3);
            toggle.Size = new Size(24, 24);
            parent.Controls.Add(toggle);
            SetTranslatedToolTip(toggle, tooltip + " Green is on. Red turns it off right away.");
            WireImmediateDriftModeOff(toggle, labelText);
            _runtimeFeatureToggles[RuntimeProfileFeature.DriftMode] = toggle;

            var apply = MakeButton("Apply", x + 620, y - 2, 96, 30);
            apply.Click += delegate
            {
                var enabled = toggle.Checked;
                RunWorker(labelText, delegate { ApplyDriftModeRuntimeHook(enabled); }, apply);
            };
            parent.Controls.Add(apply);
            SetTranslatedToolTip(apply, tooltip);
        }

        private void AddAutoMasteryRow(Control parent, int x, int y)
        {
            const string labelText = "Auto Mastery";
            const string tooltip = "Record the exact FH6 mouse path once, then run it after Luna adds BMW M2 FE cars. Press = to run.";

            var label = MakeLabel(labelText, x, y + 4);
            label.Width = 190;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var record = MakeButton("Record", x + 214, y - 2, 72, 30);
            MakeAccentButton(record, AccentBlue);
            record.Click += delegate { StartAutoMasteryRecordingWorkflow(); };
            parent.Controls.Add(record);
            SetTranslatedToolTip(record, "Adds 10 BMW M2 FE cars, then records your FH6 mouse clicks until you press F8.");
            _autoMasteryRecordButton = record;

            var runs = MakeButton("x" + _autoMasteryRuns.ToString(CultureInfo.InvariantCulture), x + 292, y - 2, 50, 30);
            runs.Click += delegate
            {
                var value = ShowAutoMasteryRunsPrompt(_autoMasteryRuns);
                if (!value.HasValue)
                    return;
                _autoMasteryRuns = value.Value;
                UpdateAutoMasteryRunsButton();
                SaveAppSettings();
                SetFeaturesStatus("Auto Mastery runs set to " + _autoMasteryRuns.ToString(CultureInfo.InvariantCulture), AccentPurple);
                Log("Auto Mastery runs set to " + _autoMasteryRuns.ToString(CultureInfo.InvariantCulture) + ".");
            };
            parent.Controls.Add(runs);
            SetTranslatedToolTip(runs, "How many times to replay the recorded macro. Luna batches 10 BMWs at a time.");
            _autoMasteryRunsButton = runs;

            var start = MakeButton("Run / =", x + 348, y - 2, 86, 30);
            MakeAccentButton(start, AccentGreen);
            start.Click += delegate { StartAutoMasteryWorkflow(); };
            parent.Controls.Add(start);
            SetTranslatedToolTip(start, tooltip);
            _autoMasteryStartButton = start;

            var stop = MakeButton("Stop", x + 440, y - 2, 62, 30);
            MakeAccentButton(stop, AccentRed);
            stop.Enabled = false;
            stop.Click += delegate { CancelAutoMasteryWorkflow(); };
            parent.Controls.Add(stop);
            SetTranslatedToolTip(stop, "Stops Auto Mastery after the current small menu step.");
            _autoMasteryStopButton = stop;
        }

        private void AddProfileBoostPackRow(Control parent, int x, int y)
        {
            const string labelText = "Profile Boost Pack";
            const string tooltip = "Uses the Credits, Both Wheelspins, Skill Points, and XP Gain hooks together.";

            var label = MakeLabel(labelText, x, y + 4);
            label.Width = 190;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var edit = MakeButton("Edit Pack", x + 420, y - 2, 116, 30);
            edit.Click += delegate
            {
                if (!ShowProfileBoostPackPrompt())
                    return;

                UpdateProfileBoostPackButton();
                SetFeaturesStatus("Profile Boost Pack values updated", AccentPurple);
                Log("Profile Boost Pack values set. Credits=" + _profilePackCredits.ToString(CultureInfo.InvariantCulture) +
                    ", Both Wheelspins=" + _profilePackWheelspins.ToString(CultureInfo.InvariantCulture) +
                    ", Skill Points=" + _profilePackSkillPoints.ToString(CultureInfo.InvariantCulture) +
                    ", XP Gain=" + _profilePackXpGainXp.ToString(CultureInfo.InvariantCulture) + " XP (" +
                    (_profilePackXpGainXp / XpGainXpPerSkillPoint).ToString(CultureInfo.InvariantCulture) +
                    " skill point(s)), Fires=" + _profilePackXpGainFireCount.ToString(CultureInfo.InvariantCulture) + ".");
            };
            parent.Controls.Add(edit);
            SetTranslatedToolTip(edit, "Click to edit Credits, Both Wheelspins, Skill Points, and XP Gain preset values.");
            _profileBoostPackButton = edit;

            var toggle = new StatusDotToggle();
            toggle.Location = new Point(x + 566, y + 3);
            toggle.Size = new Size(24, 24);
            parent.Controls.Add(toggle);
            _profileBoostPackToggle = toggle;
            SetTranslatedToolTip(toggle, tooltip + " Green applies the pack. Red turns the pack hooks off.");
            toggle.CheckedChanged += delegate
            {
                RequestFeatureOverlayRefresh();
                if (toggle.Checked)
                    return;
                if (_database == null || !_database.IsAlive)
                    return;

                RunWorker(labelText + " Off", delegate { ApplyProfileBoostPack(false); });
            };

            var apply = MakeButton("Apply", x + 620, y - 2, 96, 30);
            apply.Click += delegate
            {
                var enabled = toggle.Checked;
                RunWorker(labelText, delegate { ApplyProfileBoostPack(enabled); }, apply);
            };
            parent.Controls.Add(apply);
            SetTranslatedToolTip(apply, tooltip);
        }

        private void AddProfilePercentRow(Control parent, string labelText, int defaultPercent, int minPercent, int maxPercent, int x, int y, RuntimeProfileFeature feature, string tooltip)
        {
            if (feature == RuntimeProfileFeature.Gravity)
            {
                var gravityLabel = MakeLabel(labelText, x, y + 4);
                gravityLabel.Width = 190;
                gravityLabel.AutoSize = false;
                gravityLabel.AutoEllipsis = true;
                parent.Controls.Add(gravityLabel);
                SetTranslatedToolTip(gravityLabel, tooltip);

                var gravityValue = new TextBox();
                gravityValue.Text = defaultPercent.ToString(CultureInfo.InvariantCulture);
                gravityValue.Visible = false;
                gravityValue.Location = new Point(x + 420, y);
                gravityValue.Size = new Size(1, 1);
                parent.Controls.Add(gravityValue);
                _profileValueBoxes[feature] = gravityValue;

                var gravityButton = MakeButton(TranslateDynamicUi(FormatPercentButtonText(gravityValue.Text)), x + 420, y - 2, 116, 30);
                gravityButton.TabStop = false;
                parent.Controls.Add(gravityButton);
                SetTranslatedToolTip(gravityButton, tooltip + " Click to open the gravity slider.");
                gravityValue.TextChanged += delegate { gravityButton.Text = TranslateDynamicUi(FormatPercentButtonText(gravityValue.Text)); };

                EventHandler openGravityEditor = delegate
                {
                    int current;
                    if (!int.TryParse((gravityValue.Text ?? string.Empty).Replace(",", string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out current))
                        current = defaultPercent;
                    var selected = ShowPercentSliderPrompt("Gravity", current, minPercent, maxPercent);
                    if (!selected.HasValue)
                        return;
                    gravityValue.Text = selected.Value.ToString(CultureInfo.InvariantCulture);
                };
                gravityButton.Click += openGravityEditor;

                var gravityToggle = new StatusDotToggle();
                gravityToggle.Location = new Point(x + 566, y + 3);
                gravityToggle.Size = new Size(24, 24);
                parent.Controls.Add(gravityToggle);
                SetTranslatedToolTip(gravityToggle, tooltip + " Green is on. Red turns it off right away.");
                WireImmediateRuntimeOff(gravityToggle, feature, labelText);

                var gravityApply = MakeButton("Apply", x + 620, y - 2, 96, 30);
                gravityApply.Click += delegate
                {
                    var text = gravityValue.Text;
                    var enabled = gravityToggle.Checked;
                    RunWorker(labelText, delegate { ApplyProfileRuntimeHook(feature, labelText, text, enabled); }, gravityApply);
                };
                parent.Controls.Add(gravityApply);
                SetTranslatedToolTip(gravityApply, tooltip);
                return;
            }

            var label = MakeLabel(labelText, x, y + 4);
            label.Width = 132;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var slider = new TrackBar();
            slider.Location = new Point(x + 154, y - 4);
            slider.Size = new Size(180, 34);
            slider.Minimum = minPercent;
            slider.Maximum = maxPercent;
            slider.Value = Math.Max(minPercent, Math.Min(maxPercent, defaultPercent));
            slider.TickStyle = TickStyle.None;
            slider.SmallChange = feature == RuntimeProfileFeature.Gravity ? 10 : 1;
            slider.LargeChange = feature == RuntimeProfileFeature.Gravity ? 100 : 10;
            slider.BackColor = Surface;
            parent.Controls.Add(slider);
            SetTranslatedToolTip(slider, tooltip);

            var value = new TextBox();
            value.Text = slider.Value.ToString(CultureInfo.InvariantCulture);
            value.Location = new Point(x + 346, y);
            value.Width = 58;
            value.TextAlign = HorizontalAlignment.Right;
            StyleTextBox(value);
            parent.Controls.Add(value);
            SetTranslatedToolTip(value, tooltip + " Type a percent number or use the slider.");
            _profileValueBoxes[feature] = value;

            var suffix = new Label();
            suffix.Text = "%";
            suffix.Location = new Point(x + 408, y + 5);
            suffix.Size = new Size(18, 20);
            suffix.ForeColor = TextMuted;
            suffix.BackColor = Surface;
            suffix.Font = new Font("Segoe UI Semibold", 9F);
            parent.Controls.Add(suffix);

            var changingText = false;
            slider.ValueChanged += delegate
            {
                if (changingText)
                    return;
                value.Text = slider.Value.ToString(CultureInfo.InvariantCulture);
            };

            value.TextChanged += delegate
            {
                int parsed;
                var normalized = (value.Text ?? string.Empty).Replace(",", string.Empty).Trim();
                if (!int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
                    return;
                if (parsed < slider.Minimum || parsed > slider.Maximum)
                    return;

                changingText = true;
                try
                {
                    slider.Value = parsed;
                }
                finally
                {
                    changingText = false;
                }
            };

            var toggle = new StatusDotToggle();
            toggle.Location = new Point(x + 438, y + 3);
            toggle.Size = new Size(24, 24);
            parent.Controls.Add(toggle);
            SetTranslatedToolTip(toggle, tooltip + " Green is on. Red turns it off right away.");
            WireImmediateRuntimeOff(toggle, feature, labelText);

            var apply = MakeButton("Apply", x + 482, y - 2, 96, 30);
            apply.Click += delegate
            {
                var text = value.Text;
                var enabled = toggle.Checked;
                RunWorker(labelText, delegate { ApplyProfileRuntimeHook(feature, labelText, text, enabled); }, apply);
            };
            parent.Controls.Add(apply);
            SetTranslatedToolTip(apply, tooltip);
        }

        private static string FormatPercentButtonText(string text)
        {
            var value = (text ?? string.Empty).Replace("%", string.Empty).Trim();
            if (value.Length == 0)
                return "Click here";
            return value + "%";
        }

        private void AddFovSliderRow(Control parent, int x, int y)
        {
            const string labelText = "FOV Slider";
            const string tooltip = "Set FOV per camera view, or control all camera views with one value. 70 is close to normal. Higher is wider.";

            var label = MakeLabel(labelText, x, y + 4);
            label.Width = 190;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var valueBox = new TextBox();
            valueBox.Text = "70";
            valueBox.Visible = false;
            valueBox.Location = new Point(x + 420, y);
            valueBox.Size = new Size(1, 1);
            parent.Controls.Add(valueBox);
            _profileValueBoxes[RuntimeProfileFeature.FovSlider] = valueBox;

            var value = MakeButton(TranslateDynamicUi("Click here"), x + 420, y - 2, 116, 30);
            value.TabStop = false;
            value.Font = new Font("Segoe UI Semibold", 8.75F);
            parent.Controls.Add(value);
            SetTranslatedToolTip(value, tooltip + " Click to open the FOV camera editor.");
            _fovValueButton = value;
            UpdateFovValueButton();

            EventHandler openFovEditor = delegate
            {
                if (!ShowFovCameraConfigPrompt())
                    return;
                valueBox.Text = FormatFloat(_fovAllDegrees);
                UpdateFovValueButton();
                SaveAppSettings();

                StatusDotToggle activeToggle;
                if (_runtimeFeatureToggles.TryGetValue(RuntimeProfileFeature.FovSlider, out activeToggle) &&
                    activeToggle != null &&
                    activeToggle.Checked &&
                    _database != null &&
                    _database.IsAlive)
                {
                    RunWorker(labelText, delegate { ApplyFovRuntimeHook(true); });
                }
            };
            value.Click += openFovEditor;

            var toggle = new StatusDotToggle();
            toggle.Location = new Point(x + 566, y + 3);
            toggle.Size = new Size(24, 24);
            parent.Controls.Add(toggle);
            SetTranslatedToolTip(toggle, tooltip + " Green is on. Red turns it off right away.");
            WireImmediateRuntimeOff(toggle, RuntimeProfileFeature.FovSlider, labelText);

            var apply = MakeButton("Apply", x + 620, y - 2, 96, 30);
            apply.Click += delegate
            {
                var enabled = toggle.Checked;
                RunWorker(labelText, delegate { ApplyFovRuntimeHook(enabled); }, apply);
            };
            parent.Controls.Add(apply);
            SetTranslatedToolTip(apply, tooltip);
        }

        private void WireImmediateRuntimeOff(StatusDotToggle toggle, RuntimeProfileFeature feature, string labelText)
        {
            if (toggle == null)
                return;

            _runtimeFeatureToggles[feature] = toggle;

            toggle.CheckedChanged += delegate
            {
                RequestFeatureOverlayRefresh();
                if (toggle.Checked)
                    return;
                if (_database == null || !_database.IsAlive)
                    return;

                RunWorker(labelText + " Off", delegate { ApplyProfileRuntimeHook(feature, labelText, string.Empty, false); });
            };
        }

        private void WireImmediateJumpOff(StatusDotToggle toggle, string labelText)
        {
            if (toggle == null)
                return;

            toggle.CheckedChanged += delegate
            {
                RequestFeatureOverlayRefresh();
                if (toggle.Checked)
                    return;
                if (_database == null || !_database.IsAlive)
                    return;

                RunWorker(labelText + " Off", delegate { ApplyJumpRuntimeHook(false); });
            };
        }

        private void WireImmediateBoostOff(StatusDotToggle toggle, string labelText)
        {
            if (toggle == null)
                return;

            toggle.CheckedChanged += delegate
            {
                RequestFeatureOverlayRefresh();
                if (toggle.Checked)
                    return;
                if (_database == null || !_database.IsAlive)
                    return;

                RunWorker(labelText + " Off", delegate { ApplyBoostRuntimeHook(false); });
            };
        }

        private void WireImmediateNoClipOff(StatusDotToggle toggle, string labelText)
        {
            if (toggle == null)
                return;

            toggle.CheckedChanged += delegate
            {
                RequestFeatureOverlayRefresh();
                if (toggle.Checked)
                    return;
                if (_database == null || !_database.IsAlive)
                    return;

                RunWorker(labelText + " Off", delegate { ApplyNoClipRuntimeHook(false); });
            };
        }

        private void WireImmediatePhaseDashOff(StatusDotToggle toggle, string labelText)
        {
            if (toggle == null)
                return;

            toggle.CheckedChanged += delegate
            {
                RequestFeatureOverlayRefresh();
                if (toggle.Checked)
                    return;
                if (_database == null || !_database.IsAlive)
                    return;

                RunWorker(labelText + " Off", delegate { ApplyPhaseDashRuntimeHook(false); });
            };
        }

        private void WireImmediateSuperStrengthOff(StatusDotToggle toggle, string labelText)
        {
            if (toggle == null)
                return;

            toggle.CheckedChanged += delegate
            {
                RequestFeatureOverlayRefresh();
                if (toggle.Checked)
                    return;
                if (_database == null || !_database.IsAlive)
                    return;

                RunWorker(labelText + " Off", delegate { ApplySuperStrengthRuntimeHook(false); });
            };
        }

        private void WireImmediateTeleportOff(StatusDotToggle toggle, string labelText)
        {
            if (toggle == null)
                return;

            toggle.CheckedChanged += delegate
            {
                RequestFeatureOverlayRefresh();
                if (toggle.Checked)
                {
                    RunWorker(labelText + " On", delegate { ApplyTeleportWaypointRuntimeHook(true); });
                    return;
                }
                if (_database == null || !_database.IsAlive)
                    return;

                RunWorker(labelText + " Off", delegate { ApplyTeleportWaypointRuntimeHook(false); });
            };
        }

        private void WireImmediateTeleportCheckpointOff(StatusDotToggle toggle, string labelText)
        {
            if (toggle == null)
                return;

            toggle.CheckedChanged += delegate
            {
                RequestFeatureOverlayRefresh();
                if (toggle.Checked)
                {
                    RunWorker(labelText + " On", delegate { ApplyTeleportCheckpointRuntimeHook(true); });
                    return;
                }
                if (_database == null || !_database.IsAlive)
                    return;

                RunWorker(labelText + " Off", delegate { ApplyTeleportCheckpointRuntimeHook(false); });
            };
        }

        private void WireImmediateDriftModeOff(StatusDotToggle toggle, string labelText)
        {
            if (toggle == null)
                return;

            toggle.CheckedChanged += delegate
            {
                RequestFeatureOverlayRefresh();
                if (toggle.Checked)
                    return;
                if (_database == null || !_database.IsAlive)
                    return;

                RunWorker(labelText + " Off", delegate { ApplyDriftModeRuntimeHook(false); });
            };
        }

        private static bool IsBetaRuntimeFeature(RuntimeProfileFeature feature)
        {
            return false;
        }

        private void AddProfileWipRow(Control parent, string labelText, int x, int y, string tooltip)
        {
            var label = MakeLabel(labelText, x, y + 4);
            label.Width = 190;
            label.AutoSize = false;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var wip = new Label();
            wip.Text = "Work In Progress Coming Soon!";
            wip.Location = new Point(x + 200, y + 3);
            wip.Size = new Size(294, 24);
            wip.ForeColor = AccentRed;
            wip.BackColor = Surface;
            wip.Font = new Font("Segoe UI Semibold", 9.5F);
            wip.TextAlign = ContentAlignment.MiddleLeft;
            parent.Controls.Add(wip);
            SetTranslatedToolTip(wip, tooltip);
        }

        private void AddProfileToggleRow(Control parent, string labelText, int x, int y, RuntimeProfileFeature feature, string tooltip)
        {
            var label = MakeLabel(labelText, x, y + 4);
            label.Width = 290;
            label.AutoSize = false;
            parent.Controls.Add(label);
            SetTranslatedToolTip(label, tooltip);

            var toggle = new StatusDotToggle();
            toggle.Location = new Point(x + 566, y + 3);
            toggle.Size = new Size(24, 24);
            parent.Controls.Add(toggle);
            SetTranslatedToolTip(toggle, tooltip + " Green is on. Red turns it off right away.");
            WireImmediateRuntimeOff(toggle, feature, labelText);

            var apply = MakeButton("Apply", x + 620, y - 2, 96, 30);
            apply.Click += delegate
            {
                var enabled = toggle.Checked;
                RunWorker(labelText, delegate { ApplyProfileRuntimeHook(feature, labelText, string.Empty, enabled); }, apply);
            };
            parent.Controls.Add(apply);
            SetTranslatedToolTip(apply, tooltip);
            AddRuntimeFeatureHotkeyControls(parent, x, y, feature, labelText, null, toggle);
        }

        private int AddProfileNotesGrid(Control parent, int left, int top, int width, int columns)
        {
            const int columnGap = 14;
            const int rowGap = 10;
            const int chipHeight = 58;
            var chipWidth = (width - ((columns - 1) * columnGap)) / columns;

            var index = 0;
            Action<string, string, string, Color> add = delegate(string badge, string title, string body, Color color)
            {
                var row = index / columns;
                var col = index % columns;
                var x = left + (col * (chipWidth + columnGap));
                var y = top + (row * (chipHeight + rowGap));
                AddProfileNoteChip(parent, x, y, chipWidth, chipHeight, badge, title, body, color);
                index++;
            };

            add("\u21BB", "Load", "Live values fill in automatically when Luna attaches. Use Save/Load Config to keep feature setups.", Color.FromArgb(77, 171, 247));
            add("\u270E", "Edit", "Type the number you want, or click the value button.", Color.FromArgb(255, 212, 59));
            add("\u25CF", "Green", "Turn the row green, then press Apply.", AccentGreen);
            add("\u25CB", "Red", "Turning a row red sends the off command right away.", AccentRed);
            add("$", "Credits", "Enter Autoshow, then leave.", Color.FromArgb(32, 201, 151));
            add("\u21BA", "Spins", "Teleport, or enter and leave Autoshow.", Color.FromArgb(116, 192, 252));
            add("\u2726", "Skill", "Spend 1 skill point after applying.", Color.FromArgb(255, 146, 43));
            add("SP", "Series", "Open Festival Playlist after applying.", Color.FromArgb(255, 212, 59));
            add("\u224B", "Drift", "Turn it on before drifting. Keep Luna open.", Color.FromArgb(240, 101, 149));
            add("SZ", "Speed Zone", "Turn it on before a speed zone. 1 is normal.", Color.FromArgb(105, 219, 124));
            add("MT", "Mission Timer", "Use 0 to freeze missions, 1 for normal, or decimals to slow time.", Color.FromArgb(255, 212, 59));
            add("RT", "Race Timer", "Use 0 to freeze races, 1 for normal, or decimals to slow time.", Color.FromArgb(255, 146, 43));
            add("TOD", "Time of Day", "Type the hour (0-24) and turn it green to hold that time of day in free-roam.", Color.FromArgb(116, 192, 252));
            add("FOV", "Camera", "Click the FOV box, pick a value, turn it green, then press Apply.", Color.FromArgb(83, 177, 255));
            add("CAP", "Build Limit", "Turn it on before upgrade/tuning menus.", Color.FromArgb(255, 169, 77));
            add("AI", "Freeze AI", "Turn it on to stop AI car movement.", Color.FromArgb(255, 107, 107));
            add("\u2193", "Gravity", "Click the percent box to open the slider.", Color.FromArgb(132, 94, 247));
            add("NC", "No Clip", "Hold its key to lock into Fly-style phase movement without altitude hold.", Color.FromArgb(190, 128, 255));
            add("\u21C8", "Accel", "Click the speed box to edit the boost.", Color.FromArgb(105, 219, 124));
            add("\u25A0", "Brakes", "Super Brake and Adaptive Brake only act while you hold brake.", Color.FromArgb(250, 82, 82));
            add("\u2191", "Jump", "Tap for a normal hop. Hold for repeated hops.", Color.FromArgb(34, 184, 207));
            add("FLY", "Fly", "W/S drives through the air, A/D steers movement and body yaw.", Color.FromArgb(54, 181, 125));
            add("B", "Boost", "Hold the boost key for a small forward push.", Color.FromArgb(255, 169, 77));
            add("DM", "Drift Mode", "Turn it on, then hold the drift key while turning.", Color.FromArgb(240, 101, 149));
            add("NS", "Skill Chain", "No Skill Break keeps the skill chain alive.", Color.FromArgb(81, 207, 102));

            var rows = (index + columns - 1) / columns;
            return rows <= 0 ? 0 : ((rows * chipHeight) + ((rows - 1) * rowGap));
        }

        private void AddProfileNoteChip(Control parent, int x, int y, int width, int height, string badgeText, string title, string body, Color badgeColor)
        {
            var row = new ModernPanel();
            row.Location = new Point(x, y);
            row.Size = new Size(width, height);
            row.FillColor = SurfaceAlt;
            row.BorderColor = Blend(SurfaceAlt, Border, 0.35F);
            row.CornerRadius = 10;
            row.Tag = "NoteRow";
            parent.Controls.Add(row);

            var badge = new NoteBadge();
            badge.Text = badgeText;
            badge.Location = new Point(12, (height - 32) / 2);
            badge.Size = new Size(32, 32);
            badge.FillColor = Blend(SurfaceAlt, badgeColor, 0.28F);
            badge.GlyphColor = badgeColor;
            badge.Font = new Font("Segoe UI Semibold", 9F);
            badge.Tag = badgeColor;
            if (badgeText.Length > 1 || badgeText[0] > 127)
                badge.Font = new Font("Segoe UI Symbol", 14F, FontStyle.Bold);
            row.Controls.Add(badge);

            var textLeft = 54;
            var textWidth = Math.Max(60, width - textLeft - 10);

            var titleLabel = new Label();
            titleLabel.Text = title;
            titleLabel.Location = new Point(textLeft, 8);
            titleLabel.Size = new Size(textWidth, 18);
            titleLabel.BackColor = SurfaceAlt;
            titleLabel.ForeColor = TextPrimary;
            titleLabel.Font = new Font("Segoe UI Semibold", 9F);
            row.Controls.Add(titleLabel);

            var bodyLabel = new Label();
            bodyLabel.Text = body;
            bodyLabel.Location = new Point(textLeft, 26);
            bodyLabel.Size = new Size(textWidth, 26);
            bodyLabel.BackColor = SurfaceAlt;
            bodyLabel.ForeColor = TextMuted;
            bodyLabel.Font = new Font("Segoe UI", 8.25F);
            row.Controls.Add(bodyLabel);
        }

        private void ShowFeaturesPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowFeaturesPage);
                return;
            }
            HidePages();
            _featuresPage.Visible = true;
            _featuresPage.BringToFront();
            SetStatus("Features");
            SetFeaturesStatus("Ready");
            UpdateNavigationState(_navFeatures);
        }
    }
}
