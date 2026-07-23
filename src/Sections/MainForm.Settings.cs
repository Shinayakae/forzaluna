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
        private void BuildSettingsPage()
        {
            _settingsPage.AutoScroll = true;
            AddPageHeader(_settingsPage, "Settings", "Choose how Luna attaches, where it reads FH6 files, and which language the UI uses.");

            var versionPill = new ModernPanel();
            versionPill.SetBounds(646, 0, 250, 38);
            versionPill.FillColor = Surface;
            versionPill.BorderColor = Border;
            versionPill.CornerRadius = 12;
            versionPill.BackColor = AppBackground;
            _settingsPage.Controls.Add(versionPill);

            var versionTitle = new Label();
            versionTitle.Text = "Luna Version:";
            versionTitle.UseMnemonic = false;
            versionTitle.AutoSize = false;
            versionTitle.Font = new Font("Segoe UI Semibold", 9.5F);
            versionTitle.ForeColor = TextMuted;
            versionTitle.BackColor = Color.Transparent;
            versionTitle.TextAlign = ContentAlignment.MiddleRight;
            versionTitle.SetBounds(10, 0, 112, 38);
            versionPill.Controls.Add(versionTitle);

            _version = new Label();
            _version.Text = Program.AppVersion;
            _version.UseMnemonic = false;
            _version.AutoSize = false;
            _version.ForeColor = TextPrimary;
            _version.Font = new Font("Segoe UI Semibold", 9.5F);
            _version.TextAlign = ContentAlignment.MiddleLeft;
            _version.SetBounds(126, 0, 114, 38);
            _version.BackColor = Color.Transparent;
            versionPill.Controls.Add(_version);

            var attach = MakeCard(_settingsPage, 0, 72, ContentWidth, 274, "Connection", "Attach finds the FH6 game process automatically. Manual folder mode is only a fallback for game files.");
            attach.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var processPanel = new ModernPanel();
            processPanel.SetBounds(18, 58, 858, 64);
            processPanel.FillColor = SurfaceAlt;
            processPanel.BorderColor = Border;
            processPanel.CornerRadius = 10;
            processPanel.BackColor = Surface;
            attach.Controls.Add(processPanel);

            var procLabel = MakeLabel("Detected process", 18, 20);
            procLabel.BackColor = Color.Transparent;
            processPanel.Controls.Add(procLabel);

            _processName = MakeRoundedReadonlyField(processPanel, 160, 14, 220, 34, "ForzaHorizon6");

            _attach = MakeButton("Attach", 398, 15, 150, 34);
            MakeAccentButton(_attach, AccentBlue);
            _attach.Click += delegate { RunWorker("Attach", Attach, _attach); };
            processPanel.Controls.Add(_attach);
            SetTranslatedToolTip(_attach, "Find the running FH6 process, including supported renamed FH6 executables, and attach Luna.");

            var processNote = MakeBodyLabel("Normal use: leave manual folder mode off and let Luna find FH6 automatically.", 568, 20, 270, 24);
            processNote.BackColor = Color.Transparent;
            processNote.ForeColor = TextMuted;
            processPanel.Controls.Add(processNote);

            var folderPanel = new ModernPanel();
            folderPanel.SetBounds(18, 138, 858, 112);
            folderPanel.FillColor = SurfaceAlt;
            folderPanel.BorderColor = Border;
            folderPanel.CornerRadius = 10;
            folderPanel.BackColor = Surface;
            attach.Controls.Add(folderPanel);

            var folderTitle = MakeLabel("Game folder fallback", 18, 14);
            folderTitle.BackColor = Color.Transparent;
            folderTitle.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
            folderPanel.Controls.Add(folderTitle);

            _manualAttachMode = new CheckBox();
            _manualAttachMode.Text = "Use manual game folder";
            _manualAttachMode.Location = new Point(188, 13);
            _manualAttachMode.Size = new Size(220, 26);
            _manualAttachMode.ForeColor = TextPrimary;
            _manualAttachMode.BackColor = SurfaceAlt;
            _manualAttachMode.Checked = false;
            folderPanel.Controls.Add(_manualAttachMode);
            SetTranslatedToolTip(_manualAttachMode, "Leave this off unless Attach cannot find the FH6 folder. Manual mode accepts supported FH6 executable names in that folder.");

            _gamePath = MakeRoundedReadonlyField(folderPanel, 18, 44, 654, 34, DefaultGamePath);

            _browse = MakeButton("Browse", 690, 45, 118, 34);
            _browse.Click += delegate { BrowseForGameFolder(); };
            folderPanel.Controls.Add(_browse);

            var attachNote = MakeBodyLabel("Manual only changes the folder Luna reads names from; it does not change which process is attached.", 18, 84, 790, 22);
            attachNote.BackColor = Color.Transparent;
            attachNote.ForeColor = TextMuted;
            folderPanel.Controls.Add(attachNote);

            var liveValues = MakeCard(_settingsPage, 0, 372, ContentWidth, 150, "Live Values", "Read your current Credits, Wheelspins, and Skill Points from the game.");
            liveValues.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var liveValuesPanel = new ModernPanel();
            liveValuesPanel.SetBounds(18, 58, 858, 70);
            liveValuesPanel.FillColor = SurfaceAlt;
            liveValuesPanel.BorderColor = Border;
            liveValuesPanel.CornerRadius = 10;
            liveValuesPanel.BackColor = SurfaceAlt;
            liveValues.Controls.Add(liveValuesPanel);

            var liveValuesLabel = MakeLabel("Load current values after attach", 18, 25);
            liveValuesLabel.BackColor = Color.Transparent;
            liveValuesLabel.AutoSize = false;
            liveValuesLabel.Size = new Size(280, 22);
            liveValuesPanel.Controls.Add(liveValuesLabel);

            _loadCurrentOnAttachToggle = new StatusDotToggle();
            _loadCurrentOnAttachToggle.Size = new Size(46, 24);
            _loadCurrentOnAttachToggle.Location = new Point(304, 23);
            _loadCurrentOnAttachToggle.BackColor = SurfaceAlt;
            _loadCurrentOnAttachToggle.Checked = _loadCurrentOnAttachEnabled;
            _loadCurrentOnAttachToggle.CheckedChanged += delegate
            {
                var enabled = _loadCurrentOnAttachToggle.Checked;
                if (_loadCurrentOnAttachEnabled == enabled)
                    return;
                _loadCurrentOnAttachEnabled = enabled;
                SaveAppSettings();
                Log("Load Current after attach " + (enabled ? "ON" : "OFF") + ".");
                if (enabled && _database != null && _database.IsAlive)
                    RunWorker("Load Current", LoadCurrentRuntimeValues);
            };
            liveValuesPanel.Controls.Add(_loadCurrentOnAttachToggle);
            SetTranslatedToolTip(_loadCurrentOnAttachToggle, "When on, Luna reads Credits, Wheelspins, and Skill Points after attach (and now, if already attached). Defaults off for faster, safer startup.");

            var liveValuesNote = MakeBodyLabel("Turn on to load them now (if attached) and again on each attach. Off skips it for a faster, safer startup.", 374, 26, 466, 40);
            liveValuesNote.BackColor = Color.Transparent;
            liveValuesNote.ForeColor = TextMuted;
            liveValuesPanel.Controls.Add(liveValuesNote);

            var overlay = MakeCard(_settingsPage, 0, 546, ContentWidth, 150, "Overlay", "Shows a small draggable Luna overlay with every currently enabled feature.");
            overlay.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var overlayPanel = new ModernPanel();
            overlayPanel.SetBounds(18, 58, 858, 70);
            overlayPanel.FillColor = SurfaceAlt;
            overlayPanel.BorderColor = Border;
            overlayPanel.CornerRadius = 10;
            overlayPanel.BackColor = SurfaceAlt;
            overlay.Controls.Add(overlayPanel);

            var overlayLabel = MakeLabel("Enabled feature overlay", 18, 25);
            overlayLabel.BackColor = Color.Transparent;
            overlayLabel.AutoSize = false;
            overlayLabel.Size = new Size(230, 22);
            overlayPanel.Controls.Add(overlayLabel);

            _featureOverlayToggle = new StatusDotToggle();
            _featureOverlayToggle.Size = new Size(46, 24);
            _featureOverlayToggle.Location = new Point(246, 23);
            _featureOverlayToggle.BackColor = SurfaceAlt;
            _featureOverlayToggle.Checked = _featureOverlayEnabled;
            _featureOverlayToggle.CheckedChanged += delegate
            {
                var enabled = _featureOverlayToggle.Checked;
                if (_featureOverlayEnabled == enabled)
                    return;
                SetFeatureOverlayEnabled(enabled, true);
            };
            overlayPanel.Controls.Add(_featureOverlayToggle);
            SetTranslatedToolTip(_featureOverlayToggle, "Shows or hides the always-on-top enabled-feature overlay.");

            var overlayKeyLabel = MakeLabel("Hotkey", 316, 25);
            overlayKeyLabel.BackColor = Color.Transparent;
            overlayKeyLabel.AutoSize = false;
            overlayKeyLabel.Size = new Size(62, 22);
            overlayPanel.Controls.Add(overlayKeyLabel);

            _featureOverlayHotkeyButton = MakeButton(FormatVirtualKeyText(_featureOverlayVirtualKey), 384, 18, 92, 34);
            _featureOverlayHotkeyButton.Click += delegate
            {
                var selected = ShowKeybindPrompt("Set Overlay Key", "Press a key for the enabled-feature overlay");
                if (!selected.HasValue)
                    return;
                if (selected.Value <= 0 || selected.Value > 0xFE)
                {
                    ShowInfo("Overlay hotkey needs a keyboard key.");
                    return;
                }
                _featureOverlayVirtualKey = selected.Value;
                UpdateFeatureOverlayHotkeyButton();
                ReRegisterFeatureOverlayHotkey();
                SaveAppSettings();
                Log("Feature Overlay hotkey set to " + FormatVirtualKeyText(_featureOverlayVirtualKey) + ".");
            };
            overlayPanel.Controls.Add(_featureOverlayHotkeyButton);
            SetTranslatedToolTip(_featureOverlayHotkeyButton, "Click to pick the key that toggles the overlay on or off.");

            var overlayNote = MakeBodyLabel("Drag the overlay header to move it. Drag the corner handle to resize it. The list refreshes live.", 500, 17, 338, 44);
            overlayNote.BackColor = Color.Transparent;
            overlayNote.ForeColor = TextMuted;
            overlayPanel.Controls.Add(overlayNote);

            var folders = MakeCard(_settingsPage, 0, 720, ContentWidth, 126, "Folders", "Open Luna logs, dumps, backups, configs, and exported files.");
            folders.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var foldersPanel = new ModernPanel();
            foldersPanel.SetBounds(18, 58, 858, 46);
            foldersPanel.FillColor = SurfaceAlt;
            foldersPanel.BorderColor = Border;
            foldersPanel.CornerRadius = 10;
            foldersPanel.BackColor = SurfaceAlt;
            folders.Controls.Add(foldersPanel);

            var foldersLabel = MakeLabel("Luna output folder", 18, 12);
            foldersLabel.BackColor = Color.Transparent;
            foldersLabel.AutoSize = false;
            foldersLabel.Size = new Size(220, 22);
            foldersPanel.Controls.Add(foldersLabel);

            var openFolder = MakeButton("Open Folder", 250, 6, 150, 34);
            MakeAccentButton(openFolder, AccentBlue);
            openFolder.Click += delegate { OpenResultsFolder(); };
            foldersPanel.Controls.Add(openFolder);
            SetTranslatedToolTip(openFolder, "Open Luna's logs, dumps, configs, backups, and output folder.");

            var foldersNote = MakeBodyLabel("This replaces the old top-level Open Folder menu item.", 422, 12, 398, 22);
            foldersNote.BackColor = Color.Transparent;
            foldersNote.ForeColor = TextMuted;
            foldersPanel.Controls.Add(foldersNote);

            var language = MakeCard(_settingsPage, 0, 870, ContentWidth, 172, "Interface", "Changes buttons, labels, and UI help text. Console logs stay in English.");
            language.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var languagePanel = new ModernPanel();
            languagePanel.SetBounds(18, 58, 858, 78);
            languagePanel.FillColor = SurfaceAlt;
            languagePanel.BorderColor = Border;
            languagePanel.CornerRadius = 10;
            languagePanel.BackColor = Surface;
            language.Controls.Add(languagePanel);

            var languageLabel = MakeLabel("Display language", 18, 26);
            languageLabel.BackColor = Color.Transparent;
            languagePanel.Controls.Add(languageLabel);

            _selectedLanguageChoice = string.IsNullOrWhiteSpace(_language) ? "English" : _language;
            _languageField = MakeRoundedSelectField(languagePanel, 178, 18, 240, 38, _selectedLanguageChoice, out _languageFieldLabel);
            EventHandler openPicker = delegate
            {
                var chosen = ShowLanguagePicker(_selectedLanguageChoice);
                if (!string.IsNullOrWhiteSpace(chosen))
                {
                    _selectedLanguageChoice = chosen;
                    _languageFieldLabel.Text = chosen;
                }
            };
            _languageField.Click += openPicker;
            foreach (Control fieldChild in _languageField.Controls)
                fieldChild.Click += openPicker;
            SetTranslatedToolTip(_languageField, "Click to choose the display language.");

            var applyLanguage = MakeButton("Apply Language", 438, 18, 164, 36);
            MakeAccentButton(applyLanguage, AccentBlue);
            applyLanguage.Click += delegate
            {
                if (!string.IsNullOrWhiteSpace(_selectedLanguageChoice))
                    _language = _selectedLanguageChoice;
                ApplyLanguage();
                SaveAppSettings();
            };
            languagePanel.Controls.Add(applyLanguage);

            var languageNote = MakeBodyLabel("Only interface text is translated. Logs, numbers, paths, and raw game values stay unchanged.", 620, 18, 218, 40);
            languageNote.BackColor = Color.Transparent;
            languageNote.ForeColor = TextMuted;
            languagePanel.Controls.Add(languageNote);

            _settingsPage.AutoScrollMinSize = new Size(ContentWidth, 1076);
        }

        private TextBox MakeRoundedReadonlyField(Control parent, int x, int y, int width, int height, string text)
        {
            var lightTheme = AppBackground.GetBrightness() > 0.62F;
            var frame = new ModernPanel();
            frame.SetBounds(x, y, width, height);
            frame.FillColor = SurfaceAlt;
            frame.BorderColor = Blend(Border, AccentBlue, lightTheme ? 0.18F : 0.24F);
            frame.CornerRadius = 9;
            frame.BorderWidth = 1F;
            frame.BackColor = parent is ModernPanel ? ((ModernPanel)parent).FillColor : Surface;
            frame.Tag = "SearchField";
            parent.Controls.Add(frame);

            var box = new TextBox();
            box.BorderStyle = BorderStyle.None;
            box.BackColor = SurfaceAlt;
            box.ForeColor = TextPrimary;
            box.Font = new Font("Segoe UI", 9.5F);
            box.Text = text ?? string.Empty;
            box.ReadOnly = true;
            box.TabStop = false;
            box.Location = new Point(11, Math.Max(4, (height - 19) / 2));
            box.Size = new Size(Math.Max(20, width - 22), 19);
            frame.Controls.Add(box);
            return box;
        }

        private ModernPanel MakeRoundedSelectField(Control parent, int x, int y, int width, int height, string text, out Label valueLabel)
        {
            var lightTheme = AppBackground.GetBrightness() > 0.62F;
            var frame = new ModernPanel();
            frame.SetBounds(x, y, width, height);
            frame.FillColor = SurfaceAlt;
            frame.BorderColor = Blend(Border, AccentBlue, lightTheme ? 0.20F : 0.28F);
            frame.CornerRadius = 9;
            frame.BorderWidth = 1F;
            frame.BackColor = parent is ModernPanel ? ((ModernPanel)parent).FillColor : Surface;
            frame.Tag = "SearchField";
            frame.Cursor = Cursors.Hand;
            parent.Controls.Add(frame);

            var label = new Label();
            label.Text = text ?? string.Empty;
            label.AutoSize = false;
            label.Font = new Font("Segoe UI Semibold", 9.5F);
            label.ForeColor = TextPrimary;
            label.BackColor = Color.Transparent;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.Cursor = Cursors.Hand;
            label.SetBounds(12, 0, width - 40, height);
            frame.Controls.Add(label);

            var chevron = new Label();
            chevron.Text = "▾";
            chevron.AutoSize = false;
            chevron.Font = new Font("Segoe UI Symbol", 9F);
            chevron.ForeColor = AccentBlue;
            chevron.BackColor = Color.Transparent;
            chevron.TextAlign = ContentAlignment.MiddleCenter;
            chevron.Cursor = Cursors.Hand;
            chevron.SetBounds(width - 28, 0, 22, height);
            frame.Controls.Add(chevron);

            valueLabel = label;
            return frame;
        }

        private string ShowLanguagePicker(string current)
        {
            if (InvokeRequired)
                return (string)Invoke(new Func<string, string>(ShowLanguagePicker), current);

            var langs = new[] { "English", "Japanese", "Chinese", "Spanish", "Arabic", "Turkish", "Polish", "German",
                "Swedish", "Farsi", "French", "Lithuanian", "Portuguese", "Indonesian", "Georgian", "Vietnamese", "Dutch", "Korean" };
            var native = new[] { "English", "日本語", "中文", "Español", "العربية", "Türkçe", "Polski", "Deutsch",
                "Svenska", "فارسی", "Français", "Lietuvių", "Português", "Indonesia", "ქართული", "Tiếng Việt", "Nederlands", "한국어" };

            using (var dialog = new Form())
            {
                dialog.Text = "Select Language";
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.BackColor = AppBackground;
                dialog.ForeColor = TextPrimary;
                dialog.Font = new Font("Segoe UI", 9F);

                const int cols = 3, cellW = 176, cellH = 52, gapX = 12, gapY = 12, padX = 18, padTop = 6;
                var rows = (langs.Length + cols - 1) / cols;
                dialog.ClientSize = new Size((padX * 2) + (cols * cellW) + ((cols - 1) * gapX), padTop + (rows * (cellH + gapY)) + 6);

                string result = null;
                for (var i = 0; i < langs.Length; i++)
                {
                    var lang = langs[i];
                    var col = i % cols;
                    var row = i / cols;
                    var btn = (ModernButton)MakeButton(lang, padX + (col * (cellW + gapX)), padTop + (row * (cellH + gapY)), cellW, cellH);
                    btn.CornerRadius = 10;
                    btn.Font = new Font("Segoe UI Semibold", 10F);
                    btn.CenterContent = true;
                    if (string.Equals(lang, current, StringComparison.OrdinalIgnoreCase))
                        btn.Selected = true;
                    SetTranslatedToolTip(btn, native[i]);
                    var picked = lang;
                    btn.Click += delegate
                    {
                        result = picked;
                        dialog.DialogResult = DialogResult.OK;
                        dialog.Close();
                    };
                    dialog.Controls.Add(btn);
                }

                PrepareDialogForLanguage(dialog);
                return dialog.ShowDialog(this) == DialogResult.OK ? result : null;
            }
        }

        private void LoadAppSettings()
        {
            try
            {
                if (!File.Exists(_settingsPath))
                    return;

                foreach (var line in File.ReadAllLines(_settingsPath))
                {
                    var index = line.IndexOf('=');
                    if (index <= 0)
                        continue;
                    var key = line.Substring(0, index).Trim();
                    var value = line.Substring(index + 1).Trim();
                    if (string.Equals(key, "Language", StringComparison.OrdinalIgnoreCase))
                        _language = string.IsNullOrWhiteSpace(value) ? "English" : value;
                    else if (string.Equals(key, "FeaturesAutosave", StringComparison.OrdinalIgnoreCase))
                        _featuresAutosaveEnabled = value.Equals("True", StringComparison.OrdinalIgnoreCase) || value == "1" || value.Equals("on", StringComparison.OrdinalIgnoreCase);
                    else if (string.Equals(key, "LoadCurrentOnAttach", StringComparison.OrdinalIgnoreCase))
                        _loadCurrentOnAttachEnabled = value.Equals("True", StringComparison.OrdinalIgnoreCase) || value == "1" || value.Equals("on", StringComparison.OrdinalIgnoreCase);
                    else if (string.Equals(key, "FeatureOverlayEnabled", StringComparison.OrdinalIgnoreCase))
                        _featureOverlayEnabled = value.Equals("True", StringComparison.OrdinalIgnoreCase) || value == "1" || value.Equals("on", StringComparison.OrdinalIgnoreCase);
                    else if (string.Equals(key, "ConsoleOverlayEnabled", StringComparison.OrdinalIgnoreCase))
                        _consoleOverlayEnabled = value.Equals("True", StringComparison.OrdinalIgnoreCase) || value == "1" || value.Equals("on", StringComparison.OrdinalIgnoreCase);
                    else if (TryLoadRolePlaySetting(key, value))
                    {
                    }
                    else if (string.Equals(key, "FeatureOverlayKey", StringComparison.OrdinalIgnoreCase))
                    {
                        int keyCode;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out keyCode) &&
                            keyCode > 0 &&
                            keyCode <= 0xFE)
                        {
                            _featureOverlayVirtualKey = keyCode;
                        }
                    }
                    else if (string.Equals(key, "FeatureOverlayBounds", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = value.Split(',');
                        if (parts.Length == 4)
                        {
                            int x, y, width, height;
                            if (int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out x) &&
                                int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out y) &&
                                int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out width) &&
                                int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out height))
                            {
                                width = Math.Max(260, Math.Min(900, width));
                                height = Math.Max(150, Math.Min(900, height));
                                _featureOverlayBounds = new Rectangle(x, y, width, height);
                            }
                        }
                    }
                    else if (string.Equals(key, "ConsoleOverlayBounds", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = value.Split(',');
                        if (parts.Length == 4)
                        {
                            int x, y, width, height;
                            if (int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out x) &&
                                int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out y) &&
                                int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out width) &&
                                int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out height))
                            {
                                width = Math.Max(360, Math.Min(1200, width));
                                height = Math.Max(220, Math.Min(900, height));
                                _consoleOverlayBounds = new Rectangle(x, y, width, height);
                            }
                        }
                    }
                    else if (string.Equals(key, "LightMode", StringComparison.OrdinalIgnoreCase))
                        _lightMode = value.Equals("True", StringComparison.OrdinalIgnoreCase) || value == "1" || value.Equals("on", StringComparison.OrdinalIgnoreCase);
                    else if (string.Equals(key, "AccelerationMode", StringComparison.OrdinalIgnoreCase))
                    {
                        var usePercent = value.Equals("Percentage", StringComparison.OrdinalIgnoreCase) || value.Equals("Percent", StringComparison.OrdinalIgnoreCase);
                        _accelerationDefaultUsePercentage = usePercent;
                        _accelerationUsePercentage = usePercent;
                    }
                    else if (string.Equals(key, "AccelerationCustomMultiplier", StringComparison.OrdinalIgnoreCase))
                    {
                        float multiplier;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out multiplier) &&
                            multiplier > 0F &&
                            !float.IsNaN(multiplier) &&
                            !float.IsInfinity(multiplier))
                        {
                            _accelerationDefaultMultiplier = multiplier;
                            _accelerationToggleMultiplier = multiplier;
                        }
                    }
                    else if (string.Equals(key, "AccelerationPercent", StringComparison.OrdinalIgnoreCase))
                    {
                        int percent;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out percent))
                        {
                            percent = Math.Max(0, Math.Min(MaxAccelerationPercent, percent));
                            _accelerationDefaultPercentage = percent;
                            _accelerationPercentage = percent;
                        }
                    }
                    else if (string.Equals(key, "AutoRaceIntervalMs", StringComparison.OrdinalIgnoreCase))
                    {
                        int ms;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ms))
                            _autoRaceIntervalMs = Math.Max(100, Math.Min(5000, ms));
                    }
                    else if (string.Equals(key, "AutoRaceHoldMs", StringComparison.OrdinalIgnoreCase))
                    {
                        int ms;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ms))
                            _autoRaceHoldMs = Math.Max(50, Math.Min(3000, ms));
                    }
                    else if (string.Equals(key, "AutoRaceResetSpeed", StringComparison.OrdinalIgnoreCase))
                        _autoRaceResetSpeed = value.Equals("True", StringComparison.OrdinalIgnoreCase) || value == "1" || value.Equals("on", StringComparison.OrdinalIgnoreCase);
                    else if (string.Equals(key, "AutoMasteryRuns", StringComparison.OrdinalIgnoreCase))
                    {
                        int runs;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out runs))
                            _autoMasteryRuns = Math.Max(1, Math.Min(999, runs));
                    }
                    else if (string.Equals(key, "TeleportKey", StringComparison.OrdinalIgnoreCase))
                    {
                        int keyCode;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out keyCode) &&
                            keyCode > 0 &&
                            keyCode <= 0xFE)
                        {
                            _teleportVirtualKey = keyCode;
                        }
                    }
                    else if (string.Equals(key, "TeleportSaveKey", StringComparison.OrdinalIgnoreCase))
                    {
                        int keyCode;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out keyCode) &&
                            keyCode > 0 &&
                            keyCode <= 0xFE)
                        {
                            _teleportSaveVirtualKey = keyCode;
                        }
                    }
                    else if (string.Equals(key, "TeleportWaypointKey", StringComparison.OrdinalIgnoreCase))
                    {
                        int keyCode;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out keyCode) &&
                            keyCode > 0 &&
                            keyCode <= 0xFE)
                        {
                            _teleportWaypointVirtualKey = keyCode;
                        }
                    }
                    else if (string.Equals(key, "TeleportCheckpointKey", StringComparison.OrdinalIgnoreCase))
                    {
                        int keyCode;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out keyCode) &&
                            keyCode > 0 &&
                            keyCode <= 0xFE)
                        {
                            _teleportCheckpointVirtualKey = keyCode;
                        }
                    }
                    else if (string.Equals(key, "CheckpointRecoveryKey", StringComparison.OrdinalIgnoreCase))
                    {
                        int keyCode;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out keyCode) &&
                            keyCode > 0 &&
                            keyCode <= 0xFE)
                        {
                            _checkpointRecoveryVirtualKey = keyCode;
                        }
                    }
                    else if (string.Equals(key, "AutoRaceDriveKey", StringComparison.OrdinalIgnoreCase))
                    {
                        int keyCode;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out keyCode) &&
                            keyCode > 0 &&
                            keyCode <= 0xFE)
                        {
                            _autoRaceDriveVirtualKey = keyCode;
                        }
                    }
                    else if (string.Equals(key, "AutoRaceDriveCruiseKmh", StringComparison.OrdinalIgnoreCase))
                    {
                        int v;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out v))
                            _autoRaceDriveCruiseKmh = v;
                    }
                    else if (string.Equals(key, "AutoRaceDriveTopKmh", StringComparison.OrdinalIgnoreCase))
                    {
                        int v;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out v))
                            _autoRaceDriveTopSpeedKmh = v;
                    }
                    else if (string.Equals(key, "AutoRaceDriveAccel", StringComparison.OrdinalIgnoreCase))
                    {
                        int v;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out v))
                            _autoRaceDriveAccelStrength = v;
                    }
                    else if (string.Equals(key, "AutoRaceDriveSteer", StringComparison.OrdinalIgnoreCase))
                    {
                        float v;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                            _autoRaceDriveSteerStrength = Math.Abs(v - 5.8F) < 0.001F ? 4.6F : v;
                    }
                    else if (string.Equals(key, "AutoRaceDriveAccelMult", StringComparison.OrdinalIgnoreCase))
                    {
                        float v;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                            _autoRaceDriveAccelMultiplier = v;
                    }
                    else if (string.Equals(key, "AutoRaceDriveSuperHandling", StringComparison.OrdinalIgnoreCase))
                    {
                        float v;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                            _autoRaceDriveSuperHandling = v;
                    }
                    else if (string.Equals(key, "AutoRaceDriveSlideCalmer", StringComparison.OrdinalIgnoreCase))
                    {
                        float v;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                            _autoRaceDriveSlideCalmer = v;
                    }
                    else if (string.Equals(key, "AutoRaceDriveLandingStabilizer", StringComparison.OrdinalIgnoreCase))
                    {
                        float v;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                            _autoRaceDriveLandingStabilizer = v;
                    }
                    else if (string.Equals(key, "AutoRaceDriveAdaptiveBrake", StringComparison.OrdinalIgnoreCase))
                    {
                        float v;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                            _autoRaceDriveAdaptiveBrake = v;
                    }
                    else if (string.Equals(key, "AutoRaceDriveScanMs", StringComparison.OrdinalIgnoreCase))
                    {
                        int v;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out v))
                            _autoRaceDriveScanIntervalMs = v;
                    }
                    else if (string.Equals(key, "AutoRaceDriveScanOn", StringComparison.OrdinalIgnoreCase))
                    {
                        _autoRaceDriveScanEnabled = !string.Equals(value, "0", StringComparison.Ordinal);
                    }
                    else if (string.Equals(key, "AutoRaceDriveOffWaypoint", StringComparison.OrdinalIgnoreCase))
                    {
                        float v;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                            _autoRaceDriveOffWaypointMeters = v;
                    }
                    else if (string.Equals(key, "AutoRaceDriveOffWaypointOn", StringComparison.OrdinalIgnoreCase))
                    {
                        _autoRaceDriveOffWaypointEnabled = !string.Equals(value, "0", StringComparison.Ordinal);
                    }
                    else if (string.Equals(key, "AutoRaceDriveNoWaterDrag", StringComparison.OrdinalIgnoreCase))
                    {
                        _autoRaceDriveNoWaterDrag = !string.Equals(value, "0", StringComparison.Ordinal);
                    }
                    else if (string.Equals(key, "AutoRaceDriveCruiseOn", StringComparison.OrdinalIgnoreCase))
                    {
                        _autoRaceDriveCruiseEnabled = !string.Equals(value, "0", StringComparison.Ordinal);
                    }
                    else if (string.Equals(key, "AutoRaceDriveTopOn", StringComparison.OrdinalIgnoreCase))
                    {
                        _autoRaceDriveTopSpeedEnabled = !string.Equals(value, "0", StringComparison.Ordinal);
                    }
                    else if (string.Equals(key, "AutoRaceDriveAccelOn", StringComparison.OrdinalIgnoreCase))
                    {
                        _autoRaceDriveAccelStrengthEnabled = !string.Equals(value, "0", StringComparison.Ordinal);
                    }
                    else if (string.Equals(key, "AutoRaceDriveSteerOn", StringComparison.OrdinalIgnoreCase))
                    {
                        _autoRaceDriveSteerEnabled = !string.Equals(value, "0", StringComparison.Ordinal);
                    }
                    else if (string.Equals(key, "AutoRaceDriveAccelMultOn", StringComparison.OrdinalIgnoreCase))
                    {
                        _autoRaceDriveAccelMultEnabled = !string.Equals(value, "0", StringComparison.Ordinal);
                    }
                    else if (string.Equals(key, "AutoRaceDriveSuperHandlingOn", StringComparison.OrdinalIgnoreCase))
                    {
                        _autoRaceDriveSuperHandlingEnabled = !string.Equals(value, "0", StringComparison.Ordinal);
                    }
                    else if (string.Equals(key, "AutoRaceDriveSlideCalmerOn", StringComparison.OrdinalIgnoreCase))
                    {
                        _autoRaceDriveSlideCalmerEnabled = !string.Equals(value, "0", StringComparison.Ordinal);
                    }
                    else if (string.Equals(key, "AutoRaceDriveLandingStabilizerOn", StringComparison.OrdinalIgnoreCase))
                    {
                        _autoRaceDriveLandingStabilizerEnabled = !string.Equals(value, "0", StringComparison.Ordinal);
                    }
                    else if (string.Equals(key, "AutoRaceDriveAdaptiveBrakeOn", StringComparison.OrdinalIgnoreCase))
                    {
                        _autoRaceDriveAdaptiveBrakeEnabled = !string.Equals(value, "0", StringComparison.Ordinal);
                    }
                    else if (string.Equals(key, "TeleportPlaylistKey", StringComparison.OrdinalIgnoreCase))
                    {
                        int keyCode;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out keyCode) &&
                            keyCode > 0 &&
                            keyCode <= 0xFE)
                        {
                            _teleportPlaylistVirtualKey = keyCode;
                        }
                    }
                    else if (string.Equals(key, "TeleportPlaylistSeconds", StringComparison.OrdinalIgnoreCase))
                    {
                        float seconds;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out seconds) &&
                            seconds >= 0.25F &&
                            seconds <= 3600F)
                        {
                            _teleportPlaylistSeconds = seconds;
                        }
                    }
                    else if (string.Equals(key, "NoClipKey", StringComparison.OrdinalIgnoreCase))
                    {
                        int keyCode;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out keyCode) &&
                            keyCode > 0 &&
                            keyCode <= 0xFE)
                        {
                            _noClipVirtualKey = keyCode;
                        }
                    }
                    else if (string.Equals(key, "SuperStrengthKey", StringComparison.OrdinalIgnoreCase))
                    {
                        int keyCode;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out keyCode) &&
                            keyCode > 0 &&
                            keyCode <= 0xFE)
                        {
                            _superStrengthVirtualKey = keyCode;
                        }
                    }
                    else if (string.Equals(key, "PhaseDashKey", StringComparison.OrdinalIgnoreCase))
                    {
                        int keyCode;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out keyCode) &&
                            keyCode > 0 &&
                            keyCode <= 0xFE)
                        {
                            _phaseDashVirtualKey = keyCode;
                        }
                    }
                    else if (string.Equals(key, "RewindKey", StringComparison.OrdinalIgnoreCase))
                    {
                        int keyCode;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out keyCode) &&
                            keyCode > 0 &&
                            keyCode <= 0xFE)
                        {
                            _rewindVirtualKey = keyCode;
                        }
                    }
                    else if (string.Equals(key, "GrapplingHookKey", StringComparison.OrdinalIgnoreCase))
                    {
                        int keyCode;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out keyCode) &&
                            keyCode > 0 &&
                            keyCode <= 0xFE)
                        {
                            _grapplingHookVirtualKey = keyCode;
                        }
                    }
                    else if (string.Equals(key, "PhaseDashDistanceMeters", StringComparison.OrdinalIgnoreCase))
                    {
                        float parsed;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) && parsed >= 1F && parsed <= 500F && !float.IsNaN(parsed) && !float.IsInfinity(parsed))
                            _phaseDashDistanceMeters = parsed;
                    }
                    else if (string.Equals(key, "RewindBufferSeconds", StringComparison.OrdinalIgnoreCase))
                    {
                        float parsed;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) && parsed >= 2F && parsed <= 30F && !float.IsNaN(parsed) && !float.IsInfinity(parsed))
                            _rewindBufferSeconds = parsed;
                    }
                    else if (string.Equals(key, "RewindScrubSpeed", StringComparison.OrdinalIgnoreCase))
                    {
                        float parsed;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) && parsed >= 0.25F && parsed <= 5F && !float.IsNaN(parsed) && !float.IsInfinity(parsed))
                            _rewindScrubSpeed = parsed;
                    }
                    else if (string.Equals(key, "GrapplingHookTargetDistance", StringComparison.OrdinalIgnoreCase))
                    {
                        float parsed;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) && parsed >= 10F && parsed <= 1000F && !float.IsNaN(parsed) && !float.IsInfinity(parsed))
                            _grapplingHookTargetDistance = parsed;
                    }
                    else if (string.Equals(key, "GrapplingHookMinimumSpeed", StringComparison.OrdinalIgnoreCase))
                    {
                        float parsed;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) && parsed >= 1F && parsed <= 500F && !float.IsNaN(parsed) && !float.IsInfinity(parsed))
                            _grapplingHookMinimumSpeed = parsed;
                    }
                    else if (string.Equals(key, "GrapplingHookMaximumSpeed", StringComparison.OrdinalIgnoreCase))
                    {
                        float parsed;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) && parsed >= 1F && parsed <= 700F && !float.IsNaN(parsed) && !float.IsInfinity(parsed))
                            _grapplingHookMaximumSpeed = parsed;
                    }
                    else if (string.Equals(key, "GrapplingHookRampPerSecond", StringComparison.OrdinalIgnoreCase))
                    {
                        float parsed;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) && parsed >= 0F && parsed <= 1000F && !float.IsNaN(parsed) && !float.IsInfinity(parsed))
                            _grapplingHookRampPerSecond = parsed;
                    }
                    else if (string.Equals(key, "GrapplingHookVerticalScale", StringComparison.OrdinalIgnoreCase))
                    {
                        float parsed;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) && parsed >= 0F && parsed <= 1.5F && !float.IsNaN(parsed) && !float.IsInfinity(parsed))
                            _grapplingHookVerticalScale = parsed;
                    }
                    else if (string.Equals(key, "FovControlAll", StringComparison.OrdinalIgnoreCase))
                    {
                        bool parsed;
                        if (bool.TryParse(value, out parsed))
                            _fovControlAll = parsed;
                    }
                    else if (string.Equals(key, "FovAllDegrees", StringComparison.OrdinalIgnoreCase))
                    {
                        float parsed;
                        if (TryParseFovDegrees(value, out parsed))
                            _fovAllDegrees = parsed;
                    }
                    else if (string.Equals(key, "FovDashboardDegrees", StringComparison.OrdinalIgnoreCase))
                    {
                        float parsed;
                        if (TryParseFovDegrees(value, out parsed))
                            _fovDashboardDegrees = parsed;
                    }
                    else if (string.Equals(key, "FovChaseDegrees", StringComparison.OrdinalIgnoreCase))
                    {
                        float parsed;
                        if (TryParseFovDegrees(value, out parsed))
                            _fovChaseDegrees = parsed;
                    }
                    else if (string.Equals(key, "FovFarChaseDegrees", StringComparison.OrdinalIgnoreCase))
                    {
                        float parsed;
                        if (TryParseFovDegrees(value, out parsed))
                            _fovFarChaseDegrees = parsed;
                    }
                    else if (string.Equals(key, "FovHoodDegrees", StringComparison.OrdinalIgnoreCase))
                    {
                        float parsed;
                        if (TryParseFovDegrees(value, out parsed))
                            _fovHoodDegrees = parsed;
                    }
                    else if (string.Equals(key, "FovBumperDegrees", StringComparison.OrdinalIgnoreCase))
                    {
                        float parsed;
                        if (TryParseFovDegrees(value, out parsed))
                            _fovBumperDegrees = parsed;
                    }
                    else if (string.Equals(key, "SuperStrengthShoveSpeed", StringComparison.OrdinalIgnoreCase))
                    {
                        float parsed;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) && parsed > 0F && parsed <= 500F && !float.IsNaN(parsed) && !float.IsInfinity(parsed))
                            _superStrengthShoveSpeed = parsed;
                    }
                    else if (string.Equals(key, "SuperStrengthMinSpeed", StringComparison.OrdinalIgnoreCase))
                    {
                        float parsed;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) && parsed > 0F && parsed <= 500F && !float.IsNaN(parsed) && !float.IsInfinity(parsed))
                            _superStrengthMinSpeed = parsed;
                    }
                    else if (string.Equals(key, "SuperStrengthMaxSpeed", StringComparison.OrdinalIgnoreCase))
                    {
                        float parsed;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) && parsed > 0F && parsed <= 500F && !float.IsNaN(parsed) && !float.IsInfinity(parsed))
                            _superStrengthMaxSpeed = parsed;
                    }
                    else if (string.Equals(key, "SuperStrengthAccelerationPerSecond", StringComparison.OrdinalIgnoreCase))
                    {
                        float parsed;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) && parsed >= 0F && parsed <= 500F && !float.IsNaN(parsed) && !float.IsInfinity(parsed))
                            _superStrengthAccelerationPerSecond = parsed;
                    }
                    else if (string.Equals(key, "SuperStrengthVerticalDamping", StringComparison.OrdinalIgnoreCase))
                    {
                        float parsed;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) && parsed >= 0F && parsed <= 1F && !float.IsNaN(parsed) && !float.IsInfinity(parsed))
                            _superStrengthVerticalDamping = parsed;
                    }
                    else if (string.Equals(key, "SuperStrengthVerticalClamp", StringComparison.OrdinalIgnoreCase))
                    {
                        float parsed;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) && parsed >= 0F && parsed <= 100F && !float.IsNaN(parsed) && !float.IsInfinity(parsed))
                            _superStrengthVerticalClamp = parsed;
                    }
                    else if (string.Equals(key, "JumpKey", StringComparison.OrdinalIgnoreCase))
                    {
                        int keyCode;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out keyCode) &&
                            keyCode > 0 &&
                            keyCode <= 0xFE)
                        {
                            _jumpVirtualKey = keyCode;
                        }
                    }
                    else if (string.Equals(key, "JumpStrength", StringComparison.OrdinalIgnoreCase))
                    {
                        float strength;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out strength) &&
                            !float.IsNaN(strength) &&
                            !float.IsInfinity(strength) &&
                            strength > 0F)
                        {
                            _jumpBoost = strength;
                        }
                    }
                    else if (string.Equals(key, "ChargeJumpKey", StringComparison.OrdinalIgnoreCase))
                    {
                        int keyCode;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out keyCode) &&
                            keyCode > 0 &&
                            keyCode <= 0xFE)
                        {
                            _chargeJumpVirtualKey = keyCode;
                        }
                    }
                    else if (string.Equals(key, "ChargeJumpMaxSeconds", StringComparison.OrdinalIgnoreCase))
                    {
                        float parsed;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                            _chargeJumpMaxSeconds = parsed;
                    }
                    else if (string.Equals(key, "ChargeJumpVerticalForce", StringComparison.OrdinalIgnoreCase))
                    {
                        float parsed;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                            _chargeJumpVerticalForce = parsed;
                    }
                    else if (string.Equals(key, "ChargeJumpForwardForce", StringComparison.OrdinalIgnoreCase))
                    {
                        float parsed;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                            _chargeJumpForwardForce = parsed;
                    }
                    else if (string.Equals(key, "WallJumperKey", StringComparison.OrdinalIgnoreCase))
                    {
                        int keyCode;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out keyCode) &&
                            keyCode > 0 &&
                            keyCode <= 0xFE)
                        {
                            _wallJumperVirtualKey = keyCode;
                        }
                    }
                    else if (string.Equals(key, "WallJumperStrength", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(key, "WallClimberStrength", StringComparison.OrdinalIgnoreCase))
                    {
                        float parsed;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                            _wallJumperStrength = parsed;
                    }
                    else if (string.Equals(key, "FreezeAIHotkey", StringComparison.OrdinalIgnoreCase))
                    {
                        int keyCode;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out keyCode) &&
                            keyCode > 0 &&
                            keyCode <= 0xFE)
                        {
                            _freezeAiHotkeyVirtualKey = keyCode;
                        }
                    }
                    else if (string.Equals(key, "MissionTimerHotkey", StringComparison.OrdinalIgnoreCase))
                    {
                        int keyCode;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out keyCode) &&
                            keyCode > 0 &&
                            keyCode <= 0xFE)
                        {
                            _missionTimerHotkeyVirtualKey = keyCode;
                        }
                    }
                    else if (string.Equals(key, "RaceTimerHotkey", StringComparison.OrdinalIgnoreCase))
                    {
                        int keyCode;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out keyCode) &&
                            keyCode > 0 &&
                            keyCode <= 0xFE)
                        {
                            _raceTimerHotkeyVirtualKey = keyCode;
                        }
                    }
                    else if (string.Equals(key, "FlyHotkey", StringComparison.OrdinalIgnoreCase))
                    {
                        int keyCode;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out keyCode) &&
                            keyCode > 0 &&
                            keyCode <= 0xFE)
                        {
                            _flyHotkeyVirtualKey = keyCode;
                        }
                    }
                    else if (string.Equals(key, "AccelerationHotkey", StringComparison.OrdinalIgnoreCase))
                    {
                        int keyCode;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out keyCode) &&
                            keyCode > 0 &&
                            keyCode <= 0xFE)
                        {
                            _accelerationHotkeyVirtualKey = keyCode;
                        }
                    }
                    else if (string.Equals(key, "CruiseControlHotkey", StringComparison.OrdinalIgnoreCase))
                    {
                        int keyCode;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out keyCode) &&
                            keyCode > 0 &&
                            keyCode <= 0xFE)
                        {
                            _cruiseControlHotkeyVirtualKey = keyCode;
                        }
                    }
                    else if (string.Equals(key, "AccelerationRampHotkey", StringComparison.OrdinalIgnoreCase))
                    {
                        int keyCode;
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out keyCode) &&
                            keyCode > 0 &&
                            keyCode <= 0xFE)
                        {
                            _accelRampHotkeyVirtualKey = keyCode;
                        }
                    }
                    else if (string.Equals(key, "FlySpeed", StringComparison.OrdinalIgnoreCase))
                    {
                        float speed;
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out speed) &&
                            speed >= 1F &&
                            speed <= 250F)
                        {
                            _flySpeed = speed;
                        }
                    }
                }
                NormalizeFovConfig();
                NormalizePhaseDashDistance();
                NormalizeRewindConfig();
                NormalizeGrapplingHookConfig();
                NormalizeChargeJumpConfig();
                NormalizeWallJumperConfig();
                NormalizeSuperStrengthConfig();
                SurfaceAlt = Blend(Surface, Color.White, 0.08F);
                Border = Blend(Surface, Color.White, 0.22F);
            }
            catch
            {
            }
        }

        private void SaveAppSettings()
        {
            SaveAppSettings(false);
        }

        private void SaveAppSettings(bool quiet)
        {
            try
            {
                NormalizeFovConfig();
                NormalizePhaseDashDistance();
                NormalizeRewindConfig();
                NormalizeGrapplingHookConfig();
                NormalizeChargeJumpConfig();
                NormalizeWallJumperConfig();
                var lines = new[]
                {
                    "Language=" + (_language ?? "English"),
                    "AutoMasteryRuns=" + _autoMasteryRuns.ToString(CultureInfo.InvariantCulture),
                    "TeleportKey=" + _teleportVirtualKey.ToString(CultureInfo.InvariantCulture),
                    "TeleportWaypointKey=" + _teleportWaypointVirtualKey.ToString(CultureInfo.InvariantCulture),
                    "TeleportCheckpointKey=" + _teleportCheckpointVirtualKey.ToString(CultureInfo.InvariantCulture),
                    "CheckpointRecoveryKey=" + _checkpointRecoveryVirtualKey.ToString(CultureInfo.InvariantCulture),
                    "AutoRaceDriveKey=" + _autoRaceDriveVirtualKey.ToString(CultureInfo.InvariantCulture),
                    "AutoRaceDriveCruiseKmh=" + _autoRaceDriveCruiseKmh.ToString(CultureInfo.InvariantCulture),
                    "AutoRaceDriveTopKmh=" + _autoRaceDriveTopSpeedKmh.ToString(CultureInfo.InvariantCulture),
                    "AutoRaceDriveAccel=" + _autoRaceDriveAccelStrength.ToString(CultureInfo.InvariantCulture),
                    "AutoRaceDriveSteer=" + FormatFloat(_autoRaceDriveSteerStrength),
                    "AutoRaceDriveAccelMult=" + FormatFloat(_autoRaceDriveAccelMultiplier),
                    "AutoRaceDriveSuperHandling=" + FormatFloat(_autoRaceDriveSuperHandling),
                    "AutoRaceDriveSlideCalmer=" + FormatFloat(_autoRaceDriveSlideCalmer),
                    "AutoRaceDriveLandingStabilizer=" + FormatFloat(_autoRaceDriveLandingStabilizer),
                    "AutoRaceDriveAdaptiveBrake=" + FormatFloat(_autoRaceDriveAdaptiveBrake),
                    "AutoRaceDriveScanMs=" + _autoRaceDriveScanIntervalMs.ToString(CultureInfo.InvariantCulture),
                    "AutoRaceDriveOffWaypoint=" + FormatFloat(_autoRaceDriveOffWaypointMeters),
                    "AutoRaceDriveNoWaterDrag=" + (_autoRaceDriveNoWaterDrag ? "1" : "0"),
                    "AutoRaceDriveScanOn=" + (_autoRaceDriveScanEnabled ? "1" : "0"),
                    "AutoRaceDriveOffWaypointOn=" + (_autoRaceDriveOffWaypointEnabled ? "1" : "0"),
                    "AutoRaceDriveCruiseOn=" + (_autoRaceDriveCruiseEnabled ? "1" : "0"),
                    "AutoRaceDriveTopOn=" + (_autoRaceDriveTopSpeedEnabled ? "1" : "0"),
                    "AutoRaceDriveAccelOn=" + (_autoRaceDriveAccelStrengthEnabled ? "1" : "0"),
                    "AutoRaceDriveSteerOn=" + (_autoRaceDriveSteerEnabled ? "1" : "0"),
                    "AutoRaceDriveAccelMultOn=" + (_autoRaceDriveAccelMultEnabled ? "1" : "0"),
                    "AutoRaceDriveSuperHandlingOn=" + (_autoRaceDriveSuperHandlingEnabled ? "1" : "0"),
                    "AutoRaceDriveSlideCalmerOn=" + (_autoRaceDriveSlideCalmerEnabled ? "1" : "0"),
                    "AutoRaceDriveLandingStabilizerOn=" + (_autoRaceDriveLandingStabilizerEnabled ? "1" : "0"),
                    "AutoRaceDriveAdaptiveBrakeOn=" + (_autoRaceDriveAdaptiveBrakeEnabled ? "1" : "0"),
                    "TeleportSaveKey=" + _teleportSaveVirtualKey.ToString(CultureInfo.InvariantCulture),
                    "TeleportPlaylistKey=" + _teleportPlaylistVirtualKey.ToString(CultureInfo.InvariantCulture),
                    "TeleportPlaylistSeconds=" + FormatFloat(_teleportPlaylistSeconds),
                    "NoClipKey=" + _noClipVirtualKey.ToString(CultureInfo.InvariantCulture),
                    "SuperStrengthKey=" + _superStrengthVirtualKey.ToString(CultureInfo.InvariantCulture),
                    "PhaseDashKey=" + _phaseDashVirtualKey.ToString(CultureInfo.InvariantCulture),
                    "RewindKey=" + _rewindVirtualKey.ToString(CultureInfo.InvariantCulture),
                    "GrapplingHookKey=" + _grapplingHookVirtualKey.ToString(CultureInfo.InvariantCulture),
                    "PhaseDashDistanceMeters=" + FormatFloat(_phaseDashDistanceMeters),
                    "RewindBufferSeconds=" + FormatFloat(_rewindBufferSeconds),
                    "RewindScrubSpeed=" + FormatFloat(_rewindScrubSpeed),
                    "GrapplingHookTargetDistance=" + FormatFloat(_grapplingHookTargetDistance),
                    "GrapplingHookMinimumSpeed=" + FormatFloat(_grapplingHookMinimumSpeed),
                    "GrapplingHookMaximumSpeed=" + FormatFloat(_grapplingHookMaximumSpeed),
                    "GrapplingHookRampPerSecond=" + FormatFloat(_grapplingHookRampPerSecond),
                    "GrapplingHookVerticalScale=" + FormatFloat(_grapplingHookVerticalScale),
                    "ChargeJumpKey=" + _chargeJumpVirtualKey.ToString(CultureInfo.InvariantCulture),
                    "ChargeJumpMaxSeconds=" + FormatFloat(_chargeJumpMaxSeconds),
                    "ChargeJumpVerticalForce=" + FormatFloat(_chargeJumpVerticalForce),
                    "ChargeJumpForwardForce=" + FormatFloat(_chargeJumpForwardForce),
                    "WallJumperKey=" + _wallJumperVirtualKey.ToString(CultureInfo.InvariantCulture),
                    "WallJumperStrength=" + FormatFloat(_wallJumperStrength),
                    "FovControlAll=" + (_fovControlAll ? "True" : "False"),
                    "FovAllDegrees=" + FormatFloat(_fovAllDegrees),
                    "FovDashboardDegrees=" + FormatFloat(_fovDashboardDegrees),
                    "FovChaseDegrees=" + FormatFloat(_fovChaseDegrees),
                    "FovFarChaseDegrees=" + FormatFloat(_fovFarChaseDegrees),
                    "FovHoodDegrees=" + FormatFloat(_fovHoodDegrees),
                    "FovBumperDegrees=" + FormatFloat(_fovBumperDegrees),
                    "SuperStrengthShoveSpeed=" + FormatFloat(_superStrengthShoveSpeed),
                    "SuperStrengthMinSpeed=" + FormatFloat(_superStrengthMinSpeed),
                    "SuperStrengthMaxSpeed=" + FormatFloat(_superStrengthMaxSpeed),
                    "SuperStrengthAccelerationPerSecond=" + FormatFloat(_superStrengthAccelerationPerSecond),
                    "SuperStrengthVerticalDamping=" + FormatFloat(_superStrengthVerticalDamping),
                    "SuperStrengthVerticalClamp=" + FormatFloat(_superStrengthVerticalClamp),
                    "JumpKey=" + _jumpVirtualKey.ToString(CultureInfo.InvariantCulture),
                    "JumpStrength=" + FormatFloat(_jumpBoost),
                    "FreezeAIHotkey=" + _freezeAiHotkeyVirtualKey.ToString(CultureInfo.InvariantCulture),
                    "MissionTimerHotkey=" + _missionTimerHotkeyVirtualKey.ToString(CultureInfo.InvariantCulture),
                    "RaceTimerHotkey=" + _raceTimerHotkeyVirtualKey.ToString(CultureInfo.InvariantCulture),
                    "FlyHotkey=" + _flyHotkeyVirtualKey.ToString(CultureInfo.InvariantCulture),
                    "AccelerationHotkey=" + _accelerationHotkeyVirtualKey.ToString(CultureInfo.InvariantCulture),
                    "CruiseControlHotkey=" + _cruiseControlHotkeyVirtualKey.ToString(CultureInfo.InvariantCulture),
                    "AccelerationRampHotkey=" + _accelRampHotkeyVirtualKey.ToString(CultureInfo.InvariantCulture),
                    "AccelerationMode=" + (_accelerationDefaultUsePercentage ? "Percentage" : "Custom"),
                    "AccelerationCustomMultiplier=" + FormatFloat(_accelerationDefaultMultiplier),
                    "AccelerationPercent=" + _accelerationDefaultPercentage.ToString(CultureInfo.InvariantCulture),
                    "FlySpeed=" + FormatFloat(_flySpeed),
                    "FeaturesAutosave=" + (_featuresAutosaveEnabled ? "True" : "False"),
                    "LoadCurrentOnAttach=" + (_loadCurrentOnAttachEnabled ? "True" : "False"),
                    "FeatureOverlayEnabled=" + (_featureOverlayEnabled ? "True" : "False"),
                    "FeatureOverlayKey=" + _featureOverlayVirtualKey.ToString(CultureInfo.InvariantCulture),
                    "FeatureOverlayBounds=" + FormatFeatureOverlayBoundsForSettings(),
                    "ConsoleOverlayEnabled=" + (_consoleOverlayEnabled ? "True" : "False"),
                    "ConsoleOverlayBounds=" + FormatConsoleOverlayBoundsForSettings(),
                    "RolePlayGasMode=" + (_rolePlayGasDrainByDistance ? "Distance" : "Minutes"),
                    "RolePlayGasAmount=" + FormatFloat(_rolePlayGasAmount),
                    "GasOverlayBounds=" + FormatGasOverlayBoundsForSettings(),
                    "LightMode=" + (_lightMode ? "1" : "0"),
                    "AutoRaceIntervalMs=" + _autoRaceIntervalMs.ToString(CultureInfo.InvariantCulture),
                    "AutoRaceHoldMs=" + _autoRaceHoldMs.ToString(CultureInfo.InvariantCulture),
                    "AutoRaceResetSpeed=" + (_autoRaceResetSpeed ? "True" : "False")
                };
                File.WriteAllLines(_settingsPath, lines);
                if (!quiet)
                    Log("Settings saved.");
            }
            catch (Exception ex)
            {
                Log("Settings could not be saved: " + ex.Message);
            }
        }

        private string FormatFeatureOverlayBoundsForSettings()
        {
            var bounds = _featureOverlayForm != null && !_featureOverlayForm.IsDisposed
                ? _featureOverlayForm.Bounds
                : _featureOverlayBounds;
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return string.Empty;
            return bounds.X.ToString(CultureInfo.InvariantCulture) + "," +
                   bounds.Y.ToString(CultureInfo.InvariantCulture) + "," +
                   bounds.Width.ToString(CultureInfo.InvariantCulture) + "," +
                   bounds.Height.ToString(CultureInfo.InvariantCulture);
        }

        private string FormatConsoleOverlayBoundsForSettings()
        {
            var bounds = _consoleOverlayForm != null && !_consoleOverlayForm.IsDisposed
                ? _consoleOverlayForm.Bounds
                : _consoleOverlayBounds;
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return string.Empty;
            return bounds.X.ToString(CultureInfo.InvariantCulture) + "," +
                   bounds.Y.ToString(CultureInfo.InvariantCulture) + "," +
                   bounds.Width.ToString(CultureInfo.InvariantCulture) + "," +
                   bounds.Height.ToString(CultureInfo.InvariantCulture);
        }

        private void UpdateConsoleOverlayButton()
        {
            if (_consoleOverlayButton == null || _consoleOverlayButton.IsDisposed)
                return;

            _consoleOverlayButton.Text = _consoleOverlayEnabled ? "Overlay On" : "Overlay Off";
            MakeAccentButton(_consoleOverlayButton, _consoleOverlayEnabled ? AccentGreen : AccentRed);
            _consoleOverlayButton.Invalidate();
        }

        private void SetConsoleOverlayEnabled(bool enabled, bool save)
        {
            if (_consoleOverlayEnabled == enabled)
            {
                if (enabled)
                    ShowConsoleOverlay();
                else
                    HideConsoleOverlay(save);
                UpdateConsoleOverlayButton();
                return;
            }

            _consoleOverlayEnabled = enabled;
            UpdateConsoleOverlayButton();

            if (enabled)
                ShowConsoleOverlay();
            else
                HideConsoleOverlay(save);

            if (save)
                SaveAppSettings();
            Log("Console Overlay " + (enabled ? "ON" : "OFF") + ".");
        }

        private void ShowConsoleOverlay()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowConsoleOverlay);
                return;
            }

            if (_consoleOverlayForm == null || _consoleOverlayForm.IsDisposed)
            {
                _consoleOverlayForm = new ConsoleOverlayForm(
                    AppBackground,
                    Surface,
                    SurfaceAlt,
                    Border,
                    TextPrimary,
                    TextMuted,
                    AccentBlue,
                    AccentGreen,
                    AccentRed,
                    delegate { SetConsoleOverlayEnabled(false, true); },
                    delegate(Rectangle bounds)
                    {
                        _consoleOverlayBounds = bounds;
                        if (_consoleOverlayEnabled)
                            SaveAppSettings(true);
                    });
                _consoleOverlayForm.Bounds = NormalizeConsoleOverlayBounds(_consoleOverlayBounds);
            }

            if (!_consoleOverlayForm.Visible)
                _consoleOverlayForm.Show(this);
            _consoleOverlayForm.TopMost = true;
            _consoleOverlayForm.ApplyPalette(AppBackground, Surface, SurfaceAlt, Border, TextPrimary, TextMuted, AccentBlue, AccentGreen, AccentRed);
            RefreshConsoleOverlay();
        }

        private void HideConsoleOverlay(bool saveBounds)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(delegate { HideConsoleOverlay(saveBounds); }));
                return;
            }

            if (_consoleOverlayForm != null && !_consoleOverlayForm.IsDisposed)
            {
                _consoleOverlayBounds = _consoleOverlayForm.Bounds;
                _consoleOverlayForm.Close();
                _consoleOverlayForm.Dispose();
                _consoleOverlayForm = null;
            }

            if (saveBounds)
                SaveAppSettings();
        }

        private Rectangle NormalizeConsoleOverlayBounds(Rectangle requested)
        {
            var workingArea = Screen.PrimaryScreen == null ? new Rectangle(80, 80, 1280, 720) : Screen.PrimaryScreen.WorkingArea;
            var bounds = requested.Width > 0 && requested.Height > 0
                ? requested
                : new Rectangle(workingArea.Right - 640, workingArea.Top + 96, 580, 360);

            bounds.Width = Math.Max(360, Math.Min(1200, bounds.Width));
            bounds.Height = Math.Max(220, Math.Min(900, bounds.Height));

            var visible = false;
            foreach (var screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.IntersectsWith(bounds))
                {
                    visible = true;
                    break;
                }
            }

            if (!visible)
                bounds.Location = new Point(Math.Max(workingArea.Left, workingArea.Right - bounds.Width - 40), workingArea.Top + 96);

            return bounds;
        }

        private void RefreshConsoleOverlay()
        {
            if (_consoleOverlayForm == null || _consoleOverlayForm.IsDisposed)
                return;

            var text = _log == null || _log.IsDisposed ? string.Empty : _log.Text;
            _consoleOverlayForm.SetLogText(TrimConsoleOverlayText(text));
        }

        private void AppendConsoleOverlayLine(string line, Color messageColor)
        {
            if (_consoleOverlayForm == null || _consoleOverlayForm.IsDisposed)
                return;

            _consoleOverlayForm.AppendLine(line, messageColor);
        }

        private static string TrimConsoleOverlayText(string text)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= ConsoleOverlayMaxCharacters)
                return text ?? string.Empty;

            return text.Substring(text.Length - ConsoleOverlayMaxCharacters);
        }

        private void UpdateFeatureOverlayHotkeyButton()
        {
            if (_featureOverlayHotkeyButton != null && !_featureOverlayHotkeyButton.IsDisposed)
                _featureOverlayHotkeyButton.Text = FormatVirtualKeyText(_featureOverlayVirtualKey);
            if (_featureOverlayForm != null && !_featureOverlayForm.IsDisposed)
                _featureOverlayForm.SetHotkeyText(FormatVirtualKeyText(_featureOverlayVirtualKey));
        }

        private void RegisterFeatureOverlayHotkey()
        {
            if (_featureOverlayHotkeyRegistered || IsDisposed)
                return;
            if (_featureOverlayVirtualKey <= 0 || _featureOverlayVirtualKey > 0xFE)
            {
                Log("ERROR: Feature Overlay hotkey has no valid key.");
                return;
            }

            if (Native.RegisterHotKey(Handle, FeatureOverlayHotkeyId, 0, _featureOverlayVirtualKey))
            {
                _featureOverlayHotkeyRegistered = true;
                Log("Feature Overlay hotkey ready: press " + FormatVirtualKeyText(_featureOverlayVirtualKey) + " to show or hide it.");
                return;
            }

            Log("ERROR: Feature Overlay hotkey " + FormatVirtualKeyText(_featureOverlayVirtualKey) + " could not be registered. The Settings toggle still works.");
        }

        private void UnregisterFeatureOverlayHotkey()
        {
            if (!_featureOverlayHotkeyRegistered)
                return;

            Native.UnregisterHotKey(Handle, FeatureOverlayHotkeyId);
            _featureOverlayHotkeyRegistered = false;
        }

        private void ReRegisterFeatureOverlayHotkey()
        {
            UnregisterFeatureOverlayHotkey();
            RegisterFeatureOverlayHotkey();
        }

        private void ToggleFeatureOverlayFromHotkey()
        {
            SetFeatureOverlayEnabled(!_featureOverlayEnabled, true);
        }

        private void SetFeatureOverlayEnabled(bool enabled, bool save)
        {
            if (_featureOverlayEnabled == enabled)
            {
                if (enabled)
                    ShowFeatureOverlay();
                else
                    HideFeatureOverlay(save);
                return;
            }

            _featureOverlayEnabled = enabled;
            if (_featureOverlayToggle != null && !_featureOverlayToggle.IsDisposed && _featureOverlayToggle.Checked != enabled)
                _featureOverlayToggle.Checked = enabled;

            if (enabled)
                ShowFeatureOverlay();
            else
                HideFeatureOverlay(save);

            if (save)
                SaveAppSettings();
            Log("Feature Overlay " + (enabled ? "ON" : "OFF") + ".");
        }

        private void ShowFeatureOverlay()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowFeatureOverlay);
                return;
            }

            if (_featureOverlayForm == null || _featureOverlayForm.IsDisposed)
            {
                _featureOverlayForm = new FeatureOverlayForm(
                    AppBackground,
                    Surface,
                    SurfaceAlt,
                    Border,
                    TextPrimary,
                    TextMuted,
                    AccentGreen,
                    AccentBlue,
                    AccentRed,
                    FormatVirtualKeyText(_featureOverlayVirtualKey),
                    delegate { SetFeatureOverlayEnabled(false, true); },
                    delegate(Rectangle bounds)
                    {
                        _featureOverlayBounds = bounds;
                        if (_featureOverlayEnabled)
                            SaveAppSettings(true);
                    });
                _featureOverlayForm.Bounds = NormalizeFeatureOverlayBounds(_featureOverlayBounds);
            }

            if (!_featureOverlayForm.Visible)
                _featureOverlayForm.Show(this);
            _featureOverlayForm.TopMost = true;
            _featureOverlayForm.ApplyPalette(AppBackground, Surface, SurfaceAlt, Border, TextPrimary, TextMuted, AccentGreen, AccentBlue, AccentRed);
            _featureOverlayForm.SetHotkeyText(FormatVirtualKeyText(_featureOverlayVirtualKey));
            RefreshFeatureOverlay();
            BeginInvoke((Action)RefreshFeatureOverlay);
            if (_featureOverlayTimer != null && !_featureOverlayTimer.Enabled)
                _featureOverlayTimer.Start();
        }

        private void HideFeatureOverlay(bool saveBounds)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(delegate { HideFeatureOverlay(saveBounds); }));
                return;
            }

            if (_featureOverlayTimer != null)
                _featureOverlayTimer.Stop();

            if (_featureOverlayForm != null && !_featureOverlayForm.IsDisposed)
            {
                _featureOverlayBounds = _featureOverlayForm.Bounds;
                _featureOverlayForm.Close();
                _featureOverlayForm.Dispose();
                _featureOverlayForm = null;
            }

            if (saveBounds)
                SaveAppSettings();
        }

        private Rectangle NormalizeFeatureOverlayBounds(Rectangle requested)
        {
            var workingArea = Screen.PrimaryScreen == null ? new Rectangle(80, 80, 1280, 720) : Screen.PrimaryScreen.WorkingArea;
            var bounds = requested.Width > 0 && requested.Height > 0
                ? requested
                : new Rectangle(workingArea.Right - 380, workingArea.Top + 86, 340, 260);

            bounds.Width = Math.Max(260, Math.Min(900, bounds.Width));
            bounds.Height = Math.Max(150, Math.Min(900, bounds.Height));

            var visible = false;
            foreach (var screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.IntersectsWith(bounds))
                {
                    visible = true;
                    break;
                }
            }

            if (!visible)
                bounds.Location = new Point(Math.Max(workingArea.Left, workingArea.Right - bounds.Width - 40), workingArea.Top + 86);

            return bounds;
        }

        private void RefreshFeatureOverlay()
        {
            if (_featureOverlayForm == null || _featureOverlayForm.IsDisposed)
                return;

            _featureOverlayForm.UpdateItems(GetEnabledFeatureOverlayItems());
        }

        private void RequestFeatureOverlayRefresh()
        {
            if (!_featureOverlayEnabled || _featureOverlayForm == null || _featureOverlayForm.IsDisposed)
                return;

            if (InvokeRequired)
            {
                BeginInvoke((Action)RequestFeatureOverlayRefresh);
                return;
            }

            RefreshFeatureOverlay();
            BeginInvoke((Action)RefreshFeatureOverlay);
        }

        private List<FeatureOverlayItem> GetEnabledFeatureOverlayItems()
        {
            var items = new List<FeatureOverlayItem>();
            foreach (var item in _runtimeFeatureToggles.ToArray())
            {
                if (item.Key == RuntimeProfileFeature.Acceleration || item.Key == RuntimeProfileFeature.AccelerationRamp)
                    continue;

                var toggle = item.Value;
                if (toggle != null && !toggle.IsDisposed && toggle.Checked)
                    items.Add(new FeatureOverlayItem(
                        "runtime:" + item.Key.ToString(),
                        TranslateDynamicUi(GetRuntimeFeatureLabel(item.Key)),
                        GetFeatureOverlayValue(item.Key)));
            }

            AddOverlayRuntimeFeatureItem(items, RuntimeProfileFeature.Acceleration, _accelerationFeatureToggle);
            AddOverlayRuntimeFeatureItem(items, RuntimeProfileFeature.AccelerationRamp, _accelerationRampFeatureToggle);
            AddOverlayToggleItem(items, _profileBoostPackToggle, "special:profile-pack", "Profile Boost Pack", BuildProfilePackOverlayValue());
            AddOverlayToggleItem(items, _teleportSavedToggle, "special:saved-teleport", "Saved Teleport", "Key " + GetTeleportKeyText());
            AddOverlayToggleItem(items, _teleportWaypointToggle, "special:waypoint", "Teleport To Waypoint", "Key " + GetTeleportWaypointKeyText());
            AddOverlayToggleItem(items, _teleportCheckpointToggle, "special:auto-race-teleport", "Auto Race (Teleport)", "Key " + GetTeleportCheckpointKeyText());
            AddOverlayToggleItem(items, _autoRaceDriveToggle, "special:auto-race-drive", "Auto Race (Drive)", _autoRaceDriveCruiseKmh.ToString(CultureInfo.InvariantCulture) + " km/h");
            AddOverlayToggleItem(items, _checkpointRecoveryToggle, "special:recovery-path", "Recovery Path", "Key " + GetCheckpointRecoveryKeyText());
            return items.OrderBy(item => item.Label, StringComparer.OrdinalIgnoreCase).ThenBy(item => item.Key, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private void AddOverlayToggleItem(List<FeatureOverlayItem> items, StatusDotToggle toggle, string key, string label, string value)
        {
            if (toggle != null && !toggle.IsDisposed && toggle.Checked)
                items.Add(new FeatureOverlayItem(key, TranslateDynamicUi(label), value));
        }

        private void AddOverlayRuntimeFeatureItem(List<FeatureOverlayItem> items, RuntimeProfileFeature feature, StatusDotToggle toggle)
        {
            if (toggle != null && !toggle.IsDisposed && toggle.Checked)
                items.Add(new FeatureOverlayItem(
                    "runtime:" + feature.ToString(),
                    TranslateDynamicUi(GetRuntimeFeatureLabel(feature)),
                    GetFeatureOverlayValue(feature)));
        }

        private string GetFeatureOverlayValue(RuntimeProfileFeature feature)
        {
            switch (feature)
            {
                case RuntimeProfileFeature.Acceleration:
                    return "x" + FormatFloat(GetAccelerationRequestedMultiplier()) + (_accelerationUsePercentage ? " (" + _accelerationPercentage.ToString(CultureInfo.InvariantCulture) + "%)" : string.Empty);
                case RuntimeProfileFeature.Hover:
                    return "Speed " + FormatFloat(_flySpeed);
                case RuntimeProfileFeature.Jump:
                    return "Strength " + FormatFloat(_jumpBoost);
                case RuntimeProfileFeature.WallClimber:
                    return "Key " + GetWallJumperKeyText() + "; Strength " + FormatFloat(_wallJumperStrength);
                case RuntimeProfileFeature.ChargeJump:
                    return "Charge " + System.Threading.Volatile.Read(ref _chargeJumpOverlayPercent).ToString(CultureInfo.InvariantCulture) + "%; " + BuildChargeJumpConfigSummary();
                case RuntimeProfileFeature.Boost:
                    return "Force " + FormatFloat(_boostForce);
                case RuntimeProfileFeature.PhaseDash:
                    return FormatFloat(_phaseDashDistanceMeters) + " m";
                case RuntimeProfileFeature.SuperStrength:
                    return "Config";
                case RuntimeProfileFeature.FovSlider:
                    return BuildFovConfigSummary();
                case RuntimeProfileFeature.CruiseControl:
                    return GetFeatureOverlayTextBoxValue(feature, "km/h");
                case RuntimeProfileFeature.AccelerationRamp:
                    return GetFeatureOverlayTextBoxValue(feature, "m/s^2");
                case RuntimeProfileFeature.TopSpeedCap:
                    return GetFeatureOverlayTextBoxValue(feature, "km/h");
                case RuntimeProfileFeature.XpGain:
                    return BuildXpGainOverlayValue(feature);
                default:
                    return GetFeatureOverlayTextBoxValue(feature, string.Empty);
            }
        }

        private string GetFeatureOverlayTextBoxValue(RuntimeProfileFeature feature, string suffix)
        {
            TextBox box;
            if (!_profileValueBoxes.TryGetValue(feature, out box) || box == null || box.IsDisposed)
                return string.Empty;

            var value = (box.Text ?? string.Empty).Trim();
            if (value.Length == 0)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(suffix) && value.IndexOf(suffix, StringComparison.OrdinalIgnoreCase) < 0)
                value += " " + suffix;
            return value;
        }

        private string BuildXpGainOverlayValue(RuntimeProfileFeature feature)
        {
            TextBox box;
            if (!_profileValueBoxes.TryGetValue(feature, out box) || box == null || box.IsDisposed)
                return string.Empty;

            int amount;
            int speedMs;
            int fireCount;
            int xpAmount;
            bool whilePlaying;
            ParseXpGainConfig(box.Text, out amount, out speedMs, out whilePlaying, out fireCount, out xpAmount);
            return xpAmount.ToString(CultureInfo.InvariantCulture) + " XP / " +
                   amount.ToString(CultureInfo.InvariantCulture) + " SP" +
                   (fireCount > 0 ? " x" + fireCount.ToString(CultureInfo.InvariantCulture) : " continuous");
        }

        private string BuildProfilePackOverlayValue()
        {
            return _profilePackXpGainXp.ToString(CultureInfo.InvariantCulture) + " XP / " +
                   _profilePackSkillPoints.ToString(CultureInfo.InvariantCulture) + " SP";
        }

        private sealed class FeatureOverlayItem
        {
            public readonly string Key;
            public readonly string Label;
            public readonly string Value;

            public FeatureOverlayItem(string key, string label, string value)
            {
                Key = key ?? string.Empty;
                Label = label ?? string.Empty;
                Value = value ?? string.Empty;
            }
        }

        private void ApplyThemeToExistingControls()
        {
            ApplyThemeRecursive(this);
            UpdateWelcomeTitleColor();
            UpdateHubConnectionState(_currentConnectionState);
            if (_featureOverlayForm != null && !_featureOverlayForm.IsDisposed)
                _featureOverlayForm.ApplyPalette(AppBackground, Surface, SurfaceAlt, Border, TextPrimary, TextMuted, AccentGreen, AccentBlue, AccentRed);
            if (_consoleOverlayForm != null && !_consoleOverlayForm.IsDisposed)
                _consoleOverlayForm.ApplyPalette(AppBackground, Surface, SurfaceAlt, Border, TextPrimary, TextMuted, AccentBlue, AccentGreen, AccentRed);
            if (_gasOverlayForm != null && !_gasOverlayForm.IsDisposed)
                _gasOverlayForm.ApplyPalette(AppBackground, Surface, SurfaceAlt, Border, TextPrimary, TextMuted, AccentGreen, AccentBlue, AccentRed);
            if (_chargeJumpMeterForm != null && !_chargeJumpMeterForm.IsDisposed)
                _chargeJumpMeterForm.ApplyPalette(AppBackground, Surface, SurfaceAlt, Border, TextPrimary, TextMuted, AccentBlue, AccentPurple, AccentGreen);
            UpdateConsoleOverlayButton();
            Invalidate(true);
        }

        private void ApplyThemeRecursive(Control control)
        {
            if (control == null)
                return;

            if (control == this || control == _contentHost || control == _connectPage || control == _mainPage || control == _featuresPage ||
                control == _profilePage || control == _rarityPage || control == _skillPage || control == _carEditorPage || control == _drivingPage ||
                control == _rolePlayPage ||
                control == _consolePage || control == _settingsPage || control == _donatePage)
                control.BackColor = AppBackground;

            var modernPanel = control as ModernPanel;
            if (modernPanel != null)
            {
                if (object.Equals(modernPanel.Tag, "PageHeaderSubtitlePanel"))
                {
                    modernPanel.FillColor = Surface;
                    modernPanel.BorderColor = Border;
                    modernPanel.BackColor = Surface;
                }
                else if (object.Equals(modernPanel.Tag, "SearchField"))
                {
                    modernPanel.FillColor = SurfaceAlt;
                    modernPanel.BorderColor = Blend(Border, AccentBlue, AppBackground.GetBrightness() > 0.62F ? 0.16F : 0.22F);
                    modernPanel.BackColor = modernPanel.Parent == null ? Surface : modernPanel.Parent.BackColor;
                }
                else if (object.Equals(modernPanel.Tag, "NoteRow"))
                {
                    modernPanel.FillColor = SurfaceAlt;
                    modernPanel.BorderColor = Blend(SurfaceAlt, Border, 0.35F);
                    modernPanel.BackColor = SurfaceAlt;
                }
                else if (object.Equals(modernPanel.Tag, "CreditCard"))
                {
                    modernPanel.FillColor = SurfaceAlt;
                    modernPanel.BorderColor = Blend(Border, TextMuted, 0.26F);
                    modernPanel.BackColor = Surface;
                }
                else if (object.Equals(modernPanel.Tag, "DonatePanel"))
                {
                    modernPanel.FillColor = SurfaceAlt;
                    modernPanel.BorderColor = Blend(Border, TextMuted, 0.22F);
                    modernPanel.BackColor = SurfaceAlt;
                }
                else
                {
                    modernPanel.FillColor = Surface;
                    modernPanel.BorderColor = Border;
                    modernPanel.BackColor = Surface;
                }
            }
            else if (control is Panel || control is PictureBox)
            {
                if (object.Equals(control.Tag, "ThemeSwatch"))
                    return;
                if (object.Equals(control.Tag, "HubIcon"))
                    control.BackColor = Color.Transparent;
                else if (object.Equals(control.Tag, "DonateLogo") && control.Parent is ModernPanel)
                    control.BackColor = ((ModernPanel)control.Parent).FillColor;
                else if (control is PictureBox && control.Tag is Color)
                    control.BackColor = Blend(control.Parent == null ? Surface : control.Parent.BackColor, (Color)control.Tag, 0.22F);
                else if (object.Equals(control.Tag, "PageBg") || control.Parent == _contentHost)
                    control.BackColor = AppBackground;
                else if (control.Parent != null && control.Parent.Controls.Count > 0)
                    control.BackColor = control.Parent is ModernPanel ? Surface : control.BackColor;
            }

            var modernButton = control as ModernButton;
            if (modernButton != null)
                SkinSecondaryButton(modernButton);

            var noteBadge = control as NoteBadge;
            if (noteBadge != null)
            {
                var badgeColor = noteBadge.Tag is Color ? (Color)noteBadge.Tag : AccentBlue;
                var parentColor = noteBadge.Parent == null ? SurfaceAlt : noteBadge.Parent.BackColor;
                noteBadge.FillColor = Blend(parentColor, badgeColor, 0.28F);
                noteBadge.GlyphColor = badgeColor;
                noteBadge.BackColor = parentColor;
            }

            var pageInfoIcon = control as PageInfoIcon;
            if (pageInfoIcon != null)
            {
                pageInfoIcon.GlyphColor = TextMuted;
                pageInfoIcon.BackColor = pageInfoIcon.Parent == null ? Surface : pageInfoIcon.Parent.BackColor;
            }

            var label = control as Label;
            if (label != null)
            {
                if (object.Equals(label.Tag, "WarningLabel"))
                    label.ForeColor = AccentRed;
                else if (object.Equals(label.Tag, "HubConnectionDot"))
                {
                }
                else if (object.Equals(label.Tag, "PageHeaderSubtitle"))
                    label.ForeColor = TextMuted;
                else if (label.Tag is Color)
                    label.ForeColor = (Color)label.Tag;
                else
                    label.ForeColor = TextPrimary;

                if (label.BackColor == Color.Transparent ||
                    object.Equals(label.Tag, "HubText") ||
                    object.Equals(label.Tag, "HubConnectionDot") ||
                    object.Equals(label.Tag, "PageHeaderSubtitle") ||
                    object.Equals(label.Tag, "TutorialText"))
                    label.BackColor = Color.Transparent;
                else if (label.Parent is ModernPanel || label.Parent is Panel)
                    label.BackColor = label.Parent.BackColor;
                if (label.Tag is Color)
                    label.BackColor = Blend(label.Parent == null ? SurfaceAlt : label.Parent.BackColor, (Color)label.Tag, 0.28F);
            }

            var textBox = control as TextBox;
            if (textBox != null)
            {
                textBox.BackColor = SurfaceAlt;
                textBox.ForeColor = TextPrimary;
            }

            var richText = control as RichTextBox;
            if (richText != null && object.Equals(richText.Tag, "ConsoleLog"))
            {
                var lightTheme = AppBackground.GetBrightness() > 0.62F;
                richText.BackColor = lightTheme ? Color.FromArgb(248, 250, 252) : Color.FromArgb(17, 20, 26);
                richText.ForeColor = lightTheme ? Color.FromArgb(30, 41, 59) : Color.FromArgb(226, 232, 240);
            }

            var combo = control as ComboBox;
            if (combo != null)
            {
                combo.BackColor = SurfaceAlt;
                combo.ForeColor = TextPrimary;
            }

            var check = control as CheckBox;
            if (check != null)
            {
                check.ForeColor = TextPrimary;
                check.BackColor = check.Parent == null ? Surface : check.Parent.BackColor;
            }

            var grid = control as DataGridView;
            if (grid != null)
                ApplyThemeToGrid(grid);

            foreach (Control child in control.Controls)
                ApplyThemeRecursive(child);
        }

        private static void SkinSecondaryButton(ModernButton modern)
        {
            if (modern == null || !modern.TracksTheme)
                return;

            var lightTheme = AppBackground.GetBrightness() > 0.62F;
            modern.FillColor = lightTheme ? Color.FromArgb(248, 250, 252) : SurfaceAlt;
            modern.HoverColor = lightTheme
                ? Blend(Color.FromArgb(232, 240, 252), AccentBlue, 0.12F)
                : Blend(SurfaceAlt, TextPrimary, 0.06F);
            modern.PressedColor = lightTheme
                ? Blend(Color.FromArgb(219, 234, 254), AccentBlue, 0.16F)
                : Blend(SurfaceAlt, TextPrimary, 0.12F);
            modern.BorderColor = lightTheme ? Color.FromArgb(163, 174, 192) : Border;
            modern.SelectedFillColor = AccentBlueSoft;
            modern.SelectedBorderColor = AccentBlue;
            modern.GlyphColor = AccentBlue;
            modern.ForeColor = TextPrimary;
            modern.BackColor = modern.Parent != null ? modern.Parent.BackColor : Surface;
        }

        private static void SkinLunaDialogButtons(Control parent)
        {
            if (parent == null)
                return;

            foreach (Control child in parent.Controls)
            {
                var modern = child as ModernButton;
                if (modern != null)
                    SkinSecondaryButton(modern);
                if (child.HasChildren)
                    SkinLunaDialogButtons(child);
            }
        }

        private sealed class ModernGridState
        {
            public int HoverRow = -1;
            public ModernGridScrollBar ScrollBar;
        }

        private static void ApplyThemeToGrid(DataGridView grid)
        {
            if (grid == null)
                return;

            EnsureModernGridBehavior(grid);
            grid.BackgroundColor = Surface;
            grid.GridColor = Surface;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.None;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = Math.Max(42, grid.ColumnHeadersHeight);
            grid.ColumnHeadersDefaultCellStyle.BackColor = Blend(SurfaceAlt, AccentBlue, 0.06F);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Blend(SurfaceAlt, AccentBlue, 0.06F);
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextPrimary;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.25F);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 7, 0);
            grid.DefaultCellStyle.BackColor = Surface;
            grid.DefaultCellStyle.ForeColor = TextPrimary;
            grid.DefaultCellStyle.SelectionBackColor = Blend(Surface, AccentBlue, 0.14F);
            grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
            grid.DefaultCellStyle.Padding = new Padding(9, 3, 6, 3);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Blend(
                Surface,
                TextPrimary,
                AppBackground.GetBrightness() > 0.62F ? 0.025F : 0.035F);
            grid.AlternatingRowsDefaultCellStyle.ForeColor = TextPrimary;
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = Blend(Surface, AccentBlue, 0.14F);
            grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = TextPrimary;
            grid.RowTemplate.Height = Math.Max(40, grid.RowTemplate.Height);
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (!row.IsNewRow)
                    row.Height = Math.Max(40, row.Height);
            }
            grid.Font = new Font("Segoe UI", Math.Max(8.75F, grid.Font.Size));
            grid.ScrollBars = ScrollBars.None;
            if ((grid.AutoSizeColumnsMode == DataGridViewAutoSizeColumnsMode.Fill || grid.AutoSizeColumnsMode == DataGridViewAutoSizeColumnsMode.None) &&
                grid.Columns.Count > 0)
                grid.Columns[grid.Columns.Count - 1].MinimumWidth = Math.Max(grid.Columns[grid.Columns.Count - 1].MinimumWidth, 44);
            if (grid.Tag is ModernGridState)
            {
                var state = (ModernGridState)grid.Tag;
                if (state.ScrollBar != null)
                {
                    state.ScrollBar.TrackColor = Blend(Surface, Border, 0.28F);
                    state.ScrollBar.ThumbColor = Blend(Border, AccentBlue, 0.38F);
                    state.ScrollBar.HoverColor = AccentBlue;
                    state.ScrollBar.RefreshMetrics();
                }
            }
            grid.BackgroundColor = Surface;
            grid.Invalidate();
        }

        private static void EnsureModernGridBehavior(DataGridView grid)
        {
            if (grid.Tag is ModernGridState)
                return;

            var state = new ModernGridState();
            grid.Tag = state;
            state.ScrollBar = new ModernGridScrollBar(grid);
            state.ScrollBar.Width = 10;
            state.ScrollBar.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            grid.Controls.Add(state.ScrollBar);
            state.ScrollBar.BringToFront();
            grid.CellMouseEnter += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex == state.HoverRow)
                    return;
                var oldRow = state.HoverRow;
                state.HoverRow = e.RowIndex;
                if (oldRow >= 0 && oldRow < grid.RowCount)
                    grid.InvalidateRow(oldRow);
                if (state.HoverRow >= 0 && state.HoverRow < grid.RowCount)
                    grid.InvalidateRow(state.HoverRow);
            };
            grid.CellMouseLeave += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (grid.ClientRectangle.Contains(grid.PointToClient(Cursor.Position)))
                    return;
                var oldRow = state.HoverRow;
                state.HoverRow = -1;
                if (oldRow >= 0 && oldRow < grid.RowCount)
                    grid.InvalidateRow(oldRow);
            };
            grid.MouseLeave += delegate
            {
                var oldRow = state.HoverRow;
                state.HoverRow = -1;
                if (oldRow >= 0 && oldRow < grid.RowCount)
                    grid.InvalidateRow(oldRow);
            };
            grid.MouseWheel += delegate(object sender, MouseEventArgs e)
            {
                ScrollModernGrid(grid, e);
            };
            grid.CellPainting += delegate(object sender, DataGridViewCellPaintingEventArgs e)
            {
                PaintModernGridCell(grid, state, e);
            };
            grid.RowPostPaint += delegate(object sender, DataGridViewRowPostPaintEventArgs e)
            {
                if (e.RowIndex < 0 || e.RowIndex >= grid.RowCount || !grid.Rows[e.RowIndex].Selected)
                    return;
                using (var rail = new SolidBrush(AccentBlue))
                    e.Graphics.FillRectangle(rail, e.RowBounds.Left, e.RowBounds.Top + 4, 3, Math.Max(1, e.RowBounds.Height - 8));
            };
            grid.Paint += delegate(object sender, PaintEventArgs e)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = CreateRoundedPath(
                    new Rectangle(1, 1, Math.Max(1, grid.ClientSize.Width - 3), Math.Max(1, grid.ClientSize.Height - 3)),
                    6))
                using (var border = new Pen(Blend(AccentBlue, Color.White, 0.18F), 1.5F))
                    e.Graphics.DrawPath(border, path);
            };
            Action updateRegion = delegate
            {
                if (grid.Width <= 0 || grid.Height <= 0)
                    return;
                using (var path = CreateRoundedPath(
                    new Rectangle(0, 0, Math.Max(1, grid.Width - 1), Math.Max(1, grid.Height - 1)),
                    7))
                {
                    var previous = grid.Region;
                    grid.Region = new Region(path);
                    if (previous != null)
                        previous.Dispose();
                }
            };
            grid.SizeChanged += delegate { updateRegion(); };
            updateRegion();
        }

        private static void ScrollModernGrid(DataGridView grid, MouseEventArgs e)
        {
            if (grid == null || grid.RowCount == 0 || e.Delta == 0)
                return;

            var visibleRows = Math.Max(1, grid.DisplayedRowCount(false));
            var maxFirst = Math.Max(0, grid.RowCount - visibleRows);
            var first = 0;
            try
            {
                first = Math.Max(0, grid.FirstDisplayedScrollingRowIndex);
            }
            catch
            {
            }

            var step = Math.Max(1, Math.Min(4, Math.Abs(e.Delta) / 120 * 3));
            var target = e.Delta > 0
                ? Math.Max(0, first - step)
                : Math.Min(maxFirst, first + step);
            if (target != first)
            {
                try
                {
                    grid.FirstDisplayedScrollingRowIndex = target;
                    var handled = e as HandledMouseEventArgs;
                    if (handled != null)
                        handled.Handled = true;
                    return;
                }
                catch
                {
                }
            }

            ScrollContainingPage(grid, e.Delta);
        }

        private static void ScrollContainingPage(Control child, int delta)
        {
            Control current = child == null ? null : child.Parent;
            while (current != null)
            {
                var scrollable = current as ScrollableControl;
                if (scrollable != null && scrollable.AutoScroll)
                {
                    var currentX = Math.Max(0, -scrollable.AutoScrollPosition.X);
                    var currentY = Math.Max(0, -scrollable.AutoScrollPosition.Y);
                    var maximumY = Math.Max(0, scrollable.DisplayRectangle.Height - scrollable.ClientSize.Height);
                    var nextY = Math.Max(0, Math.Min(maximumY, currentY - (delta / 3)));
                    scrollable.AutoScrollPosition = new Point(currentX, nextY);
                    return;
                }
                current = current.Parent;
            }
        }

        private static void PaintModernGridCell(DataGridView grid, ModernGridState state, DataGridViewCellPaintingEventArgs e)
        {
            if (e.Handled || e.ColumnIndex < 0)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            if (e.RowIndex == -1)
            {
                e.Handled = true;
                var top = Blend(SurfaceAlt, AccentBlue, 0.13F);
                var bottom = Blend(SurfaceAlt, Color.Black, AppBackground.GetBrightness() > 0.62F ? 0F : 0.06F);
                using (var background = new LinearGradientBrush(e.CellBounds, top, bottom, LinearGradientMode.Vertical))
                    e.Graphics.FillRectangle(background, e.CellBounds);
                using (var line = new Pen(Blend(AccentBlue, Border, 0.34F), 1.2F))
                    e.Graphics.DrawLine(line, e.CellBounds.Left, e.CellBounds.Bottom - 2, e.CellBounds.Right, e.CellBounds.Bottom - 2);

                var textRect = Rectangle.Inflate(e.CellBounds, -10, -2);
                var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine;
                var alignment = e.CellStyle.Alignment;
                if (alignment == DataGridViewContentAlignment.MiddleCenter)
                    flags |= TextFormatFlags.HorizontalCenter;
                else if (alignment == DataGridViewContentAlignment.MiddleRight)
                    flags |= TextFormatFlags.Right;
                else
                    flags |= TextFormatFlags.Left;
                TextRenderer.DrawText(
                    e.Graphics,
                    Convert.ToString(e.FormattedValue, CultureInfo.CurrentCulture),
                    e.CellStyle.Font ?? grid.ColumnHeadersDefaultCellStyle.Font,
                    textRect,
                    TextPrimary,
                    flags);

                if (grid.SortedColumn != null && grid.SortedColumn.Index == e.ColumnIndex)
                {
                    var cx = e.CellBounds.Right - 12;
                    var cy = e.CellBounds.Top + (e.CellBounds.Height / 2);
                    var direction = grid.SortOrder == SortOrder.Descending ? -1 : 1;
                    var points = direction > 0
                        ? new[] { new Point(cx - 4, cy + 2), new Point(cx + 4, cy + 2), new Point(cx, cy - 2) }
                        : new[] { new Point(cx - 4, cy - 2), new Point(cx + 4, cy - 2), new Point(cx, cy + 2) };
                    using (var sortBrush = new SolidBrush(AccentBlue))
                        e.Graphics.FillPolygon(sortBrush, points);
                }
                return;
            }

            if (grid.IsCurrentCellInEditMode && grid.CurrentCell != null &&
                grid.CurrentCell.RowIndex == e.RowIndex && grid.CurrentCell.ColumnIndex == e.ColumnIndex)
                return;

            e.Handled = true;
            var selected = (e.State & DataGridViewElementStates.Selected) != 0;
            var baseColor = e.RowIndex % 2 == 0
                ? Surface
                : Blend(Surface, TextPrimary, AppBackground.GetBrightness() > 0.62F ? 0.025F : 0.035F);
            var backgroundColor = selected
                ? Blend(Surface, AccentBlue, 0.14F)
                : state.HoverRow == e.RowIndex
                    ? Blend(baseColor, AccentBlue, 0.045F)
                    : baseColor;
            using (var background = new SolidBrush(backgroundColor))
                e.Graphics.FillRectangle(background, e.CellBounds);

            var cell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            if (cell is DataGridViewCheckBoxCell)
            {
                PaintModernGridCheckBox(e, cell);
            }
            else
            {
                e.PaintContent(e.ClipBounds);
            }

            using (var separator = new Pen(Blend(backgroundColor, Border, 0.34F), 1F))
                e.Graphics.DrawLine(separator, e.CellBounds.Left + 12, e.CellBounds.Bottom - 1, e.CellBounds.Right - 6, e.CellBounds.Bottom - 1);
        }

        private static void PaintModernGridCheckBox(DataGridViewCellPaintingEventArgs e, DataGridViewCell cell)
        {
            var isChecked = false;
            if (cell.Value is bool)
                isChecked = (bool)cell.Value;
            else if (cell.Value != null)
                bool.TryParse(Convert.ToString(cell.Value, CultureInfo.InvariantCulture), out isChecked);

            const int size = 16;
            var rect = new Rectangle(
                e.CellBounds.Left + ((e.CellBounds.Width - size) / 2),
                e.CellBounds.Top + ((e.CellBounds.Height - size) / 2),
                size,
                size);
            var fillColor = isChecked ? AccentBlue : Blend(SurfaceAlt, Border, 0.28F);
            var borderColor = isChecked ? Blend(AccentBlue, Color.White, 0.20F) : Border;
            using (var path = CreateRoundedPath(rect, 4))
            using (var fill = new SolidBrush(fillColor))
            using (var border = new Pen(borderColor, 1.2F))
            {
                e.Graphics.FillPath(fill, path);
                e.Graphics.DrawPath(border, path);
            }
            if (!isChecked)
                return;

            using (var check = new Pen(Color.White, 1.8F))
            {
                check.StartCap = LineCap.Round;
                check.EndCap = LineCap.Round;
                e.Graphics.DrawLines(check, new[]
                {
                    new Point(rect.Left + 4, rect.Top + 8),
                    new Point(rect.Left + 7, rect.Top + 11),
                    new Point(rect.Right - 3, rect.Top + 5)
                });
            }
        }

        private void RememberEnglishText(Control parent)
        {
            if (parent == null || parent == _log)
                return;

            var grid = parent as DataGridView;
            if (grid != null)
                RememberEnglishGridText(grid);

            if ((parent is Label || parent is Button || parent is CheckBox) && !string.IsNullOrWhiteSpace(parent.Text) && !_englishText.ContainsKey(parent))
                _englishText[parent] = parent.Text;

            foreach (Control child in parent.Controls)
                RememberEnglishText(child);
        }

        private void RememberEnglishGridText(DataGridView grid)
        {
            foreach (DataGridViewColumn column in grid.Columns)
            {
                if (!string.IsNullOrWhiteSpace(column.HeaderText) && !_englishGridHeaders.ContainsKey(column))
                    _englishGridHeaders[column] = column.HeaderText;
            }

            foreach (DataGridViewRow row in grid.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.OwningColumn != null && !cell.OwningColumn.ReadOnly)
                        continue;

                    var text = cell.Value as string;
                    if (!string.IsNullOrWhiteSpace(text) && ShouldTranslateUiKey(text) && !_englishGridCells.ContainsKey(cell))
                        _englishGridCells[cell] = text;
                }
            }
        }

        private void ApplyLanguage()
        {
            if (!string.IsNullOrWhiteSpace(_language))
                _selectedLanguageChoice = _language;
            ApplyLanguageRecursive(this);
            if (_languageFieldLabel != null && !string.IsNullOrWhiteSpace(_selectedLanguageChoice))
                _languageFieldLabel.Text = _selectedLanguageChoice;
            SetStatus("Ready");
        }

        private void ApplyLanguageRecursive(Control parent)
        {
            if (parent == null || parent == _log || parent == _consolePage)
                return;

            string english;
            if (_englishText.TryGetValue(parent, out english))
                parent.Text = TranslateUi(english);

            string englishTip;
            if (_tips != null && _englishToolTips.TryGetValue(parent, out englishTip))
                _tips.SetToolTip(parent, TranslateUi(englishTip));

            var grid = parent as DataGridView;
            if (grid != null)
                ApplyGridLanguage(grid);

            foreach (Control child in parent.Controls)
                ApplyLanguageRecursive(child);
        }

        private void ApplyGridLanguage(DataGridView grid)
        {
            foreach (DataGridViewColumn column in grid.Columns)
            {
                string englishHeader;
                if (_englishGridHeaders.TryGetValue(column, out englishHeader))
                    column.HeaderText = TranslateUi(englishHeader);
            }

            foreach (DataGridViewRow row in grid.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    string englishCell;
                    if (_englishGridCells.TryGetValue(cell, out englishCell))
                        cell.Value = TranslateUi(englishCell);
                }
            }
        }

        private string TranslateUi(string english)
        {
            if (string.Equals(_language, "English", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(english))
                return english;

            Dictionary<string, string> map;
            if (!UiTranslations.TryGetValue(_language ?? "English", out map))
                return english;

            string translated;
            if (map.TryGetValue(english, out translated) && !IsBrokenTranslationValue(translated))
                return translated;

            return english;
        }

        private string TranslateDynamicUi(string english)
        {
            return TranslateUi(english);
        }

        private void SetUiText(Control control, string english)
        {
            if (control == null)
                return;

            if (!string.IsNullOrWhiteSpace(english))
                _englishText[control] = english;
            control.Text = TranslateUi(english);
        }

        private string TranslateStatusText(string english)
        {
            if (string.IsNullOrWhiteSpace(english))
                return english;

            return TranslateUi(english);
        }

        private void SetTranslatedToolTip(Control control, string english)
        {
            if (control == null || _tips == null)
                return;

            if (!string.IsNullOrWhiteSpace(english) && !_englishToolTips.ContainsKey(control))
                _englishToolTips[control] = english;

            _tips.SetToolTip(control, TranslateUi(english));
        }

        private string TranslateMessageText(string english)
        {
            if (string.IsNullOrWhiteSpace(english))
                return english;

            var exact = TranslateUi(english);
            return string.Equals(exact, english, StringComparison.Ordinal) ? english : exact;
        }

        private void PrepareDialogForLanguage(Form dialog)
        {
            if (dialog == null)
                return;

            RememberEnglishText(dialog);
            ApplyLanguageRecursive(dialog);
            if (!string.IsNullOrWhiteSpace(dialog.Text))
                dialog.Text = TranslateUi(dialog.Text);
            ApplyLunaDialogShell(dialog);
        }

        private void PrepareDialogForLanguage(FileDialog dialog)
        {
            if (dialog == null)
                return;

            if (!string.IsNullOrWhiteSpace(dialog.Title))
                dialog.Title = TranslateUi(dialog.Title);
        }

        private void PrepareDialogForLanguage(CommonDialog dialog)
        {
        }

        private DialogResult ShowTranslatedMessageBox(IWin32Window owner, string message, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return ShowLunaMessageDialog(owner, TranslateMessageText(message), buttons, icon);
        }

        private DialogResult ShowTranslatedMessageBox(string message, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return ShowLunaMessageDialog(this, TranslateMessageText(message), buttons, icon);
        }

        private void ApplyLunaDialogShell(Form dialog)
        {
            if (dialog == null || object.Equals(dialog.Tag, "LunaDialogShell"))
                return;

            var isMessageDialog = object.Equals(dialog.Tag, "LunaMessageDialog");
            var titleBarHeight = isMessageDialog ? 42 : 50;
            var contentTopPadding = isMessageDialog ? 8 : 18;
            var originalControls = dialog.Controls.Cast<Control>().ToList();
            HideDialogChromePictures(originalControls);
            var originalHeight = dialog.ClientSize.Height;
            var contentTitle = FindDialogContentTitle(originalControls, dialog.Text) ?? originalControls
                .OfType<Label>()
                .Where(label =>
                    label.Dock == DockStyle.None &&
                    label.Top <= 32 &&
                    !string.IsNullOrWhiteSpace(label.Text) &&
                    !object.Equals(label.Tag, "DialogHeading") &&
                    (label.Font.Bold || label.Font.Size >= 10F))
                .OrderBy(label => label.Top)
                .ThenBy(label => label.Left)
                .FirstOrDefault();
            var contentOffset = titleBarHeight + contentTopPadding;
            dialog.SuspendLayout();
            try
            {
                dialog.Tag = "LunaDialogShell";
                dialog.FormBorderStyle = FormBorderStyle.None;
                dialog.ControlBox = false;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ShowIcon = false;
                dialog.Padding = new Padding(4);
                dialog.BackColor = Surface;
                dialog.ClientSize = new Size(
                    dialog.ClientSize.Width,
                    originalHeight + contentOffset);

                if (contentTitle != null)
                    RemoveDialogContentTitle(contentTitle);

                foreach (var control in originalControls)
                {
                    if (control.Dock == DockStyle.None)
                        control.Top += contentOffset;

                    ApplyLunaDialogTypography(control, contentTitle);
                }
                ReflowLunaDialogContainer(dialog, contentTitle);

                var titleBar = new Panel();
                titleBar.SetBounds(4, 4, Math.Max(1, dialog.ClientSize.Width - 8), titleBarHeight - 4);
                titleBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                titleBar.BackColor = Surface;
                titleBar.Tag = "LunaDialogTitleBar";
                dialog.Controls.Add(titleBar);
                titleBar.BringToFront();

                var title = new Label();
                title.Text = isMessageDialog
                    ? string.Empty
                    : string.IsNullOrWhiteSpace(dialog.Text) ? Program.AppTitle : dialog.Text;
                title.UseMnemonic = false;
                title.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
                title.ForeColor = TextPrimary;
                title.BackColor = Color.Transparent;
                title.Location = new Point(16, 8);
                title.Size = new Size(Math.Max(80, titleBar.Width - 72), 30);
                title.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                title.TextAlign = ContentAlignment.MiddleCenter;
                titleBar.Controls.Add(title);

                var close = new LunaDialogCloseButton();
                close.SetBounds(titleBar.Width - 42, isMessageDialog ? 3 : 7, 32, 32);
                close.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                close.FillColor = AccentRed;
                close.HoverColor = ThemePaint.Darken(AccentRed, 0.16F);
                close.PressedColor = ThemePaint.Darken(AccentRed, 0.28F);
                close.GlyphColor = Color.White;
                close.Click += delegate
                {
                    dialog.DialogResult = DialogResult.Cancel;
                    dialog.Close();
                };
                titleBar.Controls.Add(close);

                Point dragCursor = Point.Empty;
                Point dragForm = Point.Empty;
                var dragging = false;
                MouseEventHandler beginDrag = delegate(object sender, MouseEventArgs e)
                {
                    if (e.Button != MouseButtons.Left)
                        return;
                    dragging = true;
                    dragCursor = Cursor.Position;
                    dragForm = dialog.Location;
                };
                MouseEventHandler moveDrag = delegate(object sender, MouseEventArgs e)
                {
                    if (!dragging || (Control.MouseButtons & MouseButtons.Left) == 0)
                        return;
                    var cursor = Cursor.Position;
                    dialog.Location = new Point(
                        dragForm.X + cursor.X - dragCursor.X,
                        dragForm.Y + cursor.Y - dragCursor.Y);
                };
                MouseEventHandler endDrag = delegate { dragging = false; };
                foreach (var draggable in new Control[] { titleBar, title })
                {
                    draggable.MouseDown += beginDrag;
                    draggable.MouseMove += moveDrag;
                    draggable.MouseUp += endDrag;
                }

                Action updateRegion = delegate
                {
                    if (dialog.Width <= 0 || dialog.Height <= 0)
                        return;
                    using (var path = CreateDialogRoundPath(
                        new Rectangle(0, 0, Math.Max(1, dialog.Width - 1), Math.Max(1, dialog.Height - 1)),
                        16))
                    {
                        var previous = dialog.Region;
                        dialog.Region = new Region(path);
                        if (previous != null)
                            previous.Dispose();
                    }
                };
                dialog.SizeChanged += delegate { updateRegion(); };
                dialog.Paint += delegate(object sender, PaintEventArgs e)
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    var rect = new Rectangle(
                        1,
                        1,
                        Math.Max(1, dialog.ClientSize.Width - 3),
                        Math.Max(1, dialog.ClientSize.Height - 3));
                    var borderColor = AppBackground.GetBrightness() > 0.62F
                        ? Color.FromArgb(176, 186, 200)
                        : Color.FromArgb(92, 96, 106);
                    using (var path = CreateDialogRoundPath(rect, 15))
                    using (var pen = new Pen(borderColor, 1.5F))
                        e.Graphics.DrawPath(pen, path);
                };
                SkinLunaDialogButtons(dialog);
                updateRegion();
            }
            finally
            {
                dialog.ResumeLayout(true);
            }
        }

        private static void HideDialogChromePictures(IEnumerable<Control> controls)
        {
            if (controls == null)
                return;

            foreach (var control in controls)
            {
                var picture = control as PictureBox;
                if (picture != null && IsDialogChromePicture(picture))
                {
                    picture.Visible = false;
                    picture.Enabled = false;
                    continue;
                }

                if (control.HasChildren)
                    HideDialogChromePictures(control.Controls.Cast<Control>());
            }
        }

        private static bool IsDialogChromePicture(PictureBox picture)
        {
            if (picture == null)
                return false;

            if (picture.Tag != null)
                return false;

            return picture.Left <= 48 &&
                picture.Top <= 48 &&
                picture.Width <= 56 &&
                picture.Height <= 56;
        }

        private static Label FindDialogContentTitle(IEnumerable<Control> controls, string dialogTitle)
        {
            if (controls == null)
                return null;

            var labels = new List<Label>();
            CollectDialogLabels(controls, labels);
            return labels
                .Where(label =>
                    label.Top <= 40 &&
                    !string.IsNullOrWhiteSpace(label.Text) &&
                    string.Equals(label.Text.Trim(), (dialogTitle ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase))
                .OrderBy(label => label.Top)
                .ThenBy(label => label.Left)
                .FirstOrDefault();
        }

        private static void CollectDialogLabels(IEnumerable<Control> controls, List<Label> labels)
        {
            foreach (var control in controls)
            {
                var label = control as Label;
                if (label != null)
                    labels.Add(label);
                if (control.HasChildren)
                    CollectDialogLabels(control.Controls.Cast<Control>(), labels);
            }
        }

        private static void RemoveDialogContentTitle(Label title)
        {
            if (title == null)
                return;

            var parent = title.Parent;
            var reclaimed = Math.Min(34, title.Height + 8);
            if (parent != null)
            {
                foreach (Control sibling in parent.Controls)
                {
                    if (sibling != title && sibling.Dock == DockStyle.None && sibling.Top > title.Top)
                        sibling.Top = Math.Max(6, sibling.Top - reclaimed);
                }
            }
            title.Visible = false;
        }

        private static void ApplyLunaDialogTypography(Control control, Label hiddenTitle)
        {
            if (control == null)
                return;

            var grid = control as DataGridView;
            if (grid != null)
                ApplyThemeToGrid(grid);

            var label = control as Label;
            if (label != null && label != hiddenTitle)
            {
                label.UseMnemonic = false;
                label.BackColor = Color.Transparent;
                if (object.Equals(label.Tag, "DialogDetail"))
                {
                    label.ForeColor = TextMuted;
                    label.Font = new Font("Segoe UI", Math.Max(8.75F, label.Font.Size), FontStyle.Regular);
                }
                else if (object.Equals(label.Tag, "DialogBody"))
                {
                    label.ForeColor = TextPrimary;
                    label.Font = new Font("Segoe UI", Math.Max(9.5F, label.Font.Size), FontStyle.Regular);
                }
                else
                {
                    label.ForeColor = TextPrimary;
                    label.Font = new Font(
                        "Segoe UI Semibold",
                        Math.Max(9F, label.Font.Size),
                        FontStyle.Bold);
                }
            }

            var save = control as ModernButton;
            if (save != null &&
                save.Text.StartsWith("Save", StringComparison.OrdinalIgnoreCase))
            {
                save.TracksTheme = false;
                save.FillColor = Color.FromArgb(82, 158, 246);
                save.HoverColor = AccentGreen;
                save.PressedColor = ThemePaint.Darken(AccentGreen, 0.20F);
                save.BorderColor = Blend(Color.FromArgb(82, 158, 246), Color.White, 0.16F);
                save.ForeColor = Color.White;
            }

            foreach (Control child in control.Controls)
                ApplyLunaDialogTypography(child, hiddenTitle);
        }

        private static void ReflowLunaDialogContainer(Control container, Label hiddenTitle)
        {
            if (container == null)
                return;

            var children = container.Controls.Cast<Control>().ToList();
            foreach (var child in children.Where(item => item.HasChildren).OrderBy(item => item.Top))
            {
                var oldHeight = child.Height;
                ReflowLunaDialogContainer(child, hiddenTitle);
                var growth = child.Height - oldHeight;
                if (growth > 0)
                    ShiftDialogControlsBelow(children, child, child.Top + oldHeight - 1, growth);
            }

            foreach (var label in children
                .OfType<Label>()
                .Where(item => item != hiddenTitle && item.Visible && !item.AutoSize && item.Width > 0 && !string.IsNullOrWhiteSpace(item.Text))
                .OrderBy(item => item.Top)
                .ThenBy(item => item.Left))
            {
                var availableWidth = Math.Max(20, label.Width);
                var measured = TextRenderer.MeasureText(
                    label.Text,
                    label.Font,
                    new Size(availableWidth, 2000),
                    TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
                var neededHeight = Math.Max(label.Height, measured.Height + 6);
                var growth = neededHeight - label.Height;
                if (growth <= 0)
                    continue;

                var oldBottom = label.Bottom;
                label.Height = neededHeight;
                ShiftDialogControlsBelow(children, label, oldBottom - 1, growth);
            }

            var visibleChildren = children.Where(item => item.Visible && item.Dock == DockStyle.None).ToList();
            if (visibleChildren.Count == 0)
                return;

            var requiredBottom = visibleChildren.Max(item => item.Bottom) + 12;
            var form = container as Form;
            if (form != null)
            {
                if (requiredBottom > form.ClientSize.Height)
                    form.ClientSize = new Size(form.ClientSize.Width, requiredBottom);
                return;
            }

            if (requiredBottom > container.ClientSize.Height)
                container.Height += requiredBottom - container.ClientSize.Height;
        }

        private static void ShiftDialogControlsBelow(List<Control> controls, Control source, int threshold, int amount)
        {
            if (controls == null || amount <= 0)
                return;

            foreach (var control in controls)
            {
                if (control == source || control.Dock != DockStyle.None || control.Top < threshold)
                    continue;
                control.Top += amount;
            }
        }

        private static GraphicsPath CreateDialogRoundPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            radius = Math.Max(1, Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2));
            var diameter = Math.Max(2, radius * 2);
            var arc = new Rectangle(rect.Left, rect.Top, diameter, diameter);
            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        private DialogResult ShowLunaMessageDialog(IWin32Window owner, string message, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            using (var dialog = new Form())
            {
                var isWarning = icon == MessageBoxIcon.Warning || icon == MessageBoxIcon.Error;
                var isQuestion = icon == MessageBoxIcon.Question;
                var accent = isWarning ? AccentRed : isQuestion ? AccentPurple : AccentBlue;
                var contentHeading = isWarning ? "Before you continue" : isQuestion ? "Please confirm" : "Update";
                dialog.Text = string.Empty;
                dialog.Tag = "LunaMessageDialog";
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.ShowInTaskbar = false;
                dialog.BackColor = Surface;
                dialog.ForeColor = TextPrimary;
                dialog.Font = new Font("Segoe UI", 9F);

                Size measured;
                using (var font = new Font("Segoe UI", 10F))
                    measured = TextRenderer.MeasureText(message ?? string.Empty, font, new Size(412, 1000), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);

                var bodyHeight = Math.Max(70, Math.Min(240, measured.Height + 16));
                var dialogHeight = bodyHeight + 156;
                dialog.ClientSize = new Size(540, dialogHeight);

                var headingLabel = new Label();
                headingLabel.Text = contentHeading;
                headingLabel.UseMnemonic = false;
                headingLabel.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
                headingLabel.ForeColor = TextPrimary;
                headingLabel.BackColor = Color.Transparent;
                headingLabel.Location = new Point(24, 18);
                headingLabel.Size = new Size(488, 26);
                headingLabel.Tag = "DialogHeading";
                dialog.Controls.Add(headingLabel);

                var detailLabel = new Label();
                detailLabel.Text = isWarning
                    ? "Review this recommendation before continuing."
                    : isQuestion
                        ? "This action needs your confirmation."
                        : "Review the details below.";
                detailLabel.UseMnemonic = false;
                detailLabel.Font = new Font("Segoe UI", 8.75F);
                detailLabel.ForeColor = TextMuted;
                detailLabel.BackColor = Color.Transparent;
                detailLabel.Location = new Point(24, 45);
                detailLabel.Size = new Size(488, 20);
                detailLabel.Tag = "DialogDetail";
                dialog.Controls.Add(detailLabel);

                var divider = new Panel();
                divider.BackColor = Blend(Border, accent, 0.20F);
                divider.SetBounds(24, 76, dialog.ClientSize.Width - 48, 1);
                dialog.Controls.Add(divider);

                var body = new Label();
                body.Text = message ?? string.Empty;
                body.UseMnemonic = false;
                body.UseCompatibleTextRendering = true;
                body.Font = new Font("Segoe UI", 10F);
                body.ForeColor = TextPrimary;
                body.BackColor = Color.Transparent;
                body.Location = new Point(24, 94);
                body.Size = new Size(dialog.ClientSize.Width - 48, bodyHeight);
                body.TextAlign = ContentAlignment.TopLeft;
                body.Tag = "DialogBody";
                dialog.Controls.Add(body);

                var buttonSpecs = buttons == MessageBoxButtons.OKCancel
                    ? new[]
                    {
                        new KeyValuePair<string, DialogResult>("Cancel", DialogResult.Cancel),
                        new KeyValuePair<string, DialogResult>("OK", DialogResult.OK)
                    }
                    : new[]
                    {
                        new KeyValuePair<string, DialogResult>("OK", DialogResult.OK)
                    };
                const int buttonWidth = 104;
                const int buttonGap = 12;
                var groupWidth = (buttonSpecs.Length * buttonWidth) + ((buttonSpecs.Length - 1) * buttonGap);
                var buttonX = dialog.ClientSize.Width - 24 - groupWidth;
                foreach (var spec in buttonSpecs)
                {
                    var button = MakeButton(spec.Key, buttonX, dialog.ClientSize.Height - 50, buttonWidth, 34);
                    if (spec.Value == DialogResult.OK)
                        MakeAccentButton(button, accent);
                    button.Click += delegate(object sender, EventArgs e)
                    {
                        var clicked = sender as Button;
                        dialog.DialogResult = clicked == null ? DialogResult.Cancel : clicked.DialogResult;
                        dialog.Close();
                    };
                    button.DialogResult = spec.Value;
                    dialog.Controls.Add(button);
                    if (spec.Value == DialogResult.OK)
                        dialog.AcceptButton = button;
                    else if (spec.Value == DialogResult.Cancel)
                        dialog.CancelButton = button;
                    buttonX += buttonWidth + buttonGap;
                }

                PrepareDialogForLanguage(dialog);
                return dialog.ShowDialog(owner);
            }
        }

        private static Color ParseThemeColor(string text, Color fallback)
        {
            if (string.IsNullOrWhiteSpace(text))
                return fallback;
            try
            {
                var value = text.Trim();
                if (!value.StartsWith("#", StringComparison.Ordinal))
                    value = "#" + value;
                return ColorTranslator.FromHtml(value);
            }
            catch
            {
                return fallback;
            }
        }

        private static bool TryParseThemeColor(string text, out Color color)
        {
            color = Color.Empty;
            if (string.IsNullOrWhiteSpace(text))
                return false;
            try
            {
                var value = text.Trim();
                if (!value.StartsWith("#", StringComparison.Ordinal))
                    value = "#" + value;
                color = ColorTranslator.FromHtml(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string ColorToHex(Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

        private static Color Blend(Color a, Color b, float amountB)
        {
            amountB = Math.Max(0F, Math.Min(1F, amountB));
            var amountA = 1F - amountB;
            return Color.FromArgb(
                (int)Math.Round(a.R * amountA + b.R * amountB),
                (int)Math.Round(a.G * amountA + b.G * amountB),
                (int)Math.Round(a.B * amountA + b.B * amountB));
        }

        private static Color ColorFromHsv(double hue, double saturation, double value)
        {
            hue = ((hue % 360) + 360) % 360;
            saturation = Math.Max(0, Math.Min(1, saturation));
            value = Math.Max(0, Math.Min(1, value));

            var chroma = value * saturation;
            var x = chroma * (1 - Math.Abs((hue / 60.0) % 2 - 1));
            var m = value - chroma;
            double r, g, b;

            if (hue < 60) { r = chroma; g = x; b = 0; }
            else if (hue < 120) { r = x; g = chroma; b = 0; }
            else if (hue < 180) { r = 0; g = chroma; b = x; }
            else if (hue < 240) { r = 0; g = x; b = chroma; }
            else if (hue < 300) { r = x; g = 0; b = chroma; }
            else { r = chroma; g = 0; b = x; }

            return Color.FromArgb(
                (int)Math.Round((r + m) * 255),
                (int)Math.Round((g + m) * 255),
                (int)Math.Round((b + m) * 255));
        }

        private static void SeedKnownUiKeys()
        {
            var keys = new[]
            {
                "Teleport", "Console", "Tutorial", "Credits", "Open Folder", "Runtime Hooks", "Quick Tutorials", "Feature How To Use",
                "Project Credits", "Setting", "What it does", "Strength", "Toggle", "Ready", "Use Value", "Cancel",
                "Save Preset", "Load Preset", "Apply Live Hooks", "Turn All Off", "Load", "Edit", "Green", "Red",
                "Attached", "Not attached", "running...", "success", "failed", "Back", "Add Selected", "Add All",
                "Remove Selected", "Confirm", "Close", "OK", "Save", "Browse", "Pick", "Display language",
                "Language", "Color Theme", "Connection Status", "Start Here", "Database Actions", "Quick Actions",
                "Internal Actions", "Database Tuning", "Database Tuning Actions", "Selected Car Editor", "Pick Part",
                "New Value", "Section", "Source", "Decal Unlocker",
                "Editors", "Tables & Unlocks", "Car ID", "Year", "Make", "Model", "Class", "Copies", "State",
                "Status", "Value", "Current", "Default", "Description", "Name", "File", "Select", "Owned",
                "Price", "Quantity", "Restore All Car Classes", "Class Restrictions: OFF", "Class Restrictions: ON",
                "Disable Damage: OFF", "Disable Damage: ON", "Garage Favorites", "Traffic Editor", "Wheelspin Odds",
                "Photo Capture", "Routes", "Races", "Free Prices", "DLC Gates", "Install Flags", "New Tags",
                "Unlock Presets", "Unobtainable Gate", "Vehicle Tuner", "Tuning", "Driving Tuning", "Tuning Actions",
                "Driving Tuning Actions", "Live Tuning Table", "Driving Tuning Table", "Scan Tuning", "Start Live Scan",
                "Stop Live Scan", "Apply Selected", "Restore Snapshot", "Tuning Reference", "Car Class Panel", "Stats Editor",
                "Speed Zone", "Build Limit", "Camera", "Brakes", "Skill Chain", "Clothing",
                "No Build Limit", "Freeze AI", "FOV Slider", "No Clip", "Speed Zone Multiplier",
                "Mission Timer Scale", "Race Timer Scale", "Time of Day", "No Water Drag", "Super Handling",
                "Landing Stabilizer", "Slide Calmer", "Road Magnet", "Speed Tamer", "Air Lift", "Bounce Cushion",
                "Momentum Control", "Left Right Calm", "Forward Back Calm", "Side Push", "Forward Push",
                "Vertical Trim", "Side Lock", "Forward Lock", "Vertical Hold", "Motion Freeze", "Wheelie Boost",
                "Drift Kick", "Hover Glide", "Air Brake", "Corner Stabilizer", "Planted Boost", "Forward Launch",
                "Straight Launch", "Tire Bite", "Grip Lock", "Forward Grip", "Rail Grip", "High Speed Stabilizer",
                "Ground Clamp", "Stability Rail", "Corner Bite", "Spin Recovery", "Boost",
                "Drift Mode", "Both Wheelspins", "Skill Points", "Super Strength", "Phase Dash", "Config",
                "Super Strength Configuration", "Save Config", "Reset Defaults", "Shove speed", "Minimum speed",
                "Maximum speed", "Acceleration ramp", "Vertical damping", "Vertical clamp"
            };

            foreach (var map in UiTranslations.Values)
            {
                foreach (var key in keys)
                {
                    if (!map.ContainsKey(key))
                        map[key] = key;
                }
            }

            AddUiTranslations("Japanese",
                "Attached", "接続済み", "Not attached", "未接続", "running...", "実行中...", "success", "成功", "failed", "失敗",
                "Back", "戻る", "Add Selected", "選択を追加", "Add All", "すべて追加", "Remove Selected", "選択を削除",
                "Confirm", "確認", "Close", "閉じる", "OK", "OK", "Save", "保存", "Browse", "参照", "Pick", "選択",
                "Display language", "表示言語", "Language", "言語", "Color Theme", "カラーテーマ", "Connection Status", "接続状態",
                "Start Here", "ここから開始", "Database Actions", "データ操作", "Quick Actions", "クイック操作",
                "Editors", "エディター", "Tables & Unlocks", "テーブルと解除", "Car ID", "車ID", "Year", "年式",
                "Make", "メーカー", "Model", "モデル", "Class", "クラス", "Copies", "台数", "State", "状態",
                "Status", "状態", "Value", "値", "Current", "現在", "Default", "初期値", "Description", "説明",
                "Name", "名前", "File", "ファイル", "Select", "選択", "Owned", "所有済み", "Price", "価格", "Quantity", "数量");

            AddUiTranslations("Chinese",
                "Attached", "已附加", "Not attached", "未附加", "running...", "运行中...", "success", "成功", "failed", "失败",
                "Back", "返回", "Add Selected", "添加所选", "Add All", "全部添加", "Remove Selected", "移除所选",
                "Confirm", "确认", "Close", "关闭", "OK", "确定", "Save", "保存", "Browse", "浏览", "Pick", "选择",
                "Display language", "显示语言", "Language", "语言", "Color Theme", "颜色主题", "Connection Status", "连接状态",
                "Start Here", "从这里开始", "Database Actions", "数据库操作", "Quick Actions", "快捷操作",
                "Editors", "编辑器", "Tables & Unlocks", "表格与解锁", "Car ID", "车辆ID", "Year", "年份",
                "Make", "品牌", "Model", "型号", "Class", "等级", "Copies", "数量", "State", "状态",
                "Status", "状态", "Value", "数值", "Current", "当前", "Default", "默认", "Description", "说明",
                "Name", "名称", "File", "文件", "Select", "选择", "Owned", "已拥有", "Price", "价格", "Quantity", "数量");

            AddUiTranslations("Arabic",
                "Attached", "متصل", "Not attached", "غير متصل", "running...", "قيد التشغيل...", "success", "نجاح", "failed", "فشل",
                "Back", "رجوع", "Add Selected", "إضافة المحدد", "Add All", "إضافة الكل", "Remove Selected", "حذف المحدد",
                "Confirm", "تأكيد", "Close", "إغلاق", "OK", "حسنًا", "Save", "حفظ", "Pick", "اختيار",
                "Display language", "لغة العرض", "Language", "اللغة", "Color Theme", "سمة الألوان", "Connection Status", "حالة الاتصال",
                "Start Here", "ابدأ هنا", "Database Actions", "إجراءات قاعدة البيانات", "Quick Actions", "إجراءات سريعة",
                "Editors", "المحررات", "Tables & Unlocks", "الجداول وفتح القفل", "Car ID", "معرف السيارة", "Year", "السنة",
                "Make", "الشركة", "Model", "الطراز", "Class", "الفئة", "Copies", "النسخ", "State", "الحالة",
                "Status", "الحالة", "Value", "القيمة", "Current", "الحالي", "Default", "الافتراضي", "Description", "الوصف",
                "Name", "الاسم", "File", "الملف", "Select", "تحديد", "Owned", "مملوك", "Price", "السعر", "Quantity", "الكمية");

            AddUiTranslations("Turkish",
                "Attached", "Bağlı", "Not attached", "Bağlı değil", "running...", "çalışıyor...", "success", "başarılı", "failed", "başarısız",
                "Back", "Geri", "Add Selected", "Seçileni ekle", "Add All", "Tümünü ekle", "Remove Selected", "Seçileni kaldır",
                "Confirm", "Onayla", "Close", "Kapat", "OK", "Tamam", "Save", "Kaydet", "Browse", "Gözat", "Pick", "Seç",
                "Display language", "Görüntü dili", "Language", "Dil", "Color Theme", "Renk teması", "Connection Status", "Bağlantı durumu",
                "Start Here", "Buradan başla", "Database Actions", "Veritabanı işlemleri", "Quick Actions", "Hızlı işlemler",
                "Editors", "Düzenleyiciler", "Tables & Unlocks", "Tablolar ve kilitler", "Car ID", "Araç ID", "Year", "Yıl",
                "Make", "Marka", "Model", "Model", "Class", "Sınıf", "Copies", "Kopya", "State", "Durum",
                "Status", "Durum", "Value", "Değer", "Current", "Geçerli", "Default", "Varsayılan", "Description", "Açıklama",
                "Name", "Ad", "File", "Dosya", "Select", "Seç", "Owned", "Sahip", "Price", "Fiyat", "Quantity", "Adet");

            AddUiTranslations("Polish",
                "Attached", "Połączono", "Not attached", "Nie połączono", "running...", "działa...", "success", "sukces", "failed", "błąd",
                "Back", "Wstecz", "Add Selected", "Dodaj wybrane", "Add All", "Dodaj wszystko", "Remove Selected", "Usuń wybrane",
                "Confirm", "Potwierdź", "Close", "Zamknij", "OK", "OK", "Save", "Zapisz", "Browse", "Przeglądaj", "Pick", "Wybierz",
                "Display language", "Język interfejsu", "Language", "Język", "Color Theme", "Motyw kolorów", "Connection Status", "Stan połączenia",
                "Start Here", "Zacznij tutaj", "Database Actions", "Akcje bazy", "Quick Actions", "Szybkie akcje",
                "Editors", "Edytory", "Tables & Unlocks", "Tabele i odblokowania", "Car ID", "ID auta", "Year", "Rok",
                "Make", "Marka", "Model", "Model", "Class", "Klasa", "Copies", "Kopie", "State", "Stan",
                "Status", "Status", "Value", "Wartość", "Current", "Obecne", "Default", "Domyślne", "Description", "Opis",
                "Name", "Nazwa", "File", "Plik", "Select", "Wybierz", "Owned", "Posiadane", "Price", "Cena", "Quantity", "Ilość");

            AddUiTranslations("Swedish",
                "Attached", "Ansluten", "Not attached", "Inte ansluten", "running...", "kör...", "success", "klart", "failed", "misslyckades",
                "Back", "Tillbaka", "Add Selected", "Lägg till valda", "Add All", "Lägg till alla", "Remove Selected", "Ta bort valda",
                "Confirm", "Bekräfta", "Close", "Stäng", "OK", "OK", "Save", "Spara", "Browse", "Bläddra", "Pick", "Välj",
                "Display language", "Visningsspråk", "Language", "Språk", "Color Theme", "Färgtema", "Connection Status", "Anslutningsstatus",
                "Start Here", "Börja här", "Database Actions", "Databasåtgärder", "Quick Actions", "Snabbåtgärder",
                "Editors", "Redigerare", "Tables & Unlocks", "Tabeller och upplåsning", "Car ID", "Bil-ID", "Year", "År",
                "Make", "Märke", "Model", "Modell", "Class", "Klass", "Copies", "Kopior", "State", "Tillstånd",
                "Status", "Status", "Value", "Värde", "Current", "Nuvarande", "Default", "Standard", "Description", "Beskrivning",
                "Name", "Namn", "File", "Fil", "Select", "Välj", "Owned", "Ägd", "Price", "Pris", "Quantity", "Antal");

            AddUiTranslations("Vietnamese",
                "Runtime Hooks", "Hook trực tiếp", "Quick Tutorials", "Hướng dẫn nhanh", "Feature How To Use", "Cách dùng tính năng",
                "Project Credits", "Ghi công dự án", "Setting", "Cài đặt", "Use Value", "Dùng giá trị", "Save Preset", "Lưu mẫu",
                "Load Preset", "Tải mẫu", "Apply Live Hooks", "Áp dụng hook", "Turn All Off", "Tắt tất cả", "Edit", "Sửa",
                "Green", "Xanh", "Red", "Đỏ", "No Build Limit", "Không giới hạn build",
                "Freeze AI", "Đóng băng AI", "FOV Slider", "Thanh FOV", "No Clip", "Xuyên vật thể", "Speed Zone Multiplier", "Nhân điểm Speed Zone",
                "Mission Timer Scale", "Tỉ lệ giờ nhiệm vụ", "Race Timer Scale", "Tỉ lệ giờ đua", "Time of Day", "Giờ trong ngày",
                "No Water Drag", "Không cản nước", "Super Handling", "Siêu bám lái", "Landing Stabilizer", "Ổn định tiếp đất",
                "Slide Calmer", "Giảm trượt", "Road Magnet", "Bám mặt đường", "Speed Tamer", "Hãm tốc", "Air Lift", "Nâng trên không",
                "Bounce Cushion", "Đệm nảy", "Momentum Control", "Kiểm soát đà", "Left Right Calm", "Ổn định trái/phải",
                "Forward Back Calm", "Ổn định trước/sau", "Side Push", "Đẩy ngang", "Forward Push", "Đẩy tới",
                "Vertical Trim", "Chỉnh dọc", "Side Lock", "Khóa ngang", "Forward Lock", "Khóa tới", "Vertical Hold", "Giữ dọc",
                "Motion Freeze", "Đóng băng chuyển động", "Wheelie Boost", "Tăng bốc đầu", "Drift Kick", "Đá drift",
                "Hover Glide", "Lướt nổi", "Air Brake", "Phanh trên không", "Corner Stabilizer", "Ổn định cua",
                "Planted Boost", "Tăng bám đường", "Forward Launch", "Phóng tới", "Straight Launch", "Phóng thẳng",
                "Tire Bite", "Độ bám lốp", "Grip Lock", "Khóa bám", "Forward Grip", "Bám tiến", "Rail Grip", "Bám ray",
                "High Speed Stabilizer", "Ổn định tốc cao", "Ground Clamp", "Ép xuống đất", "Stability Rail", "Ray ổn định",
                "Corner Bite", "Bám cua", "Spin Recovery", "Phục hồi xoay",
                "Boost", "Tăng tốc", "Drift Mode", "Chế độ drift", "Both Wheelspins", "Cả hai wheelspin",
                "Skill Points", "Điểm kỹ năng", "Wheelspin Odds", "Tỉ lệ wheelspin", "Garage Favorites", "Yêu thích garage",
                "Traffic Editor", "Chỉnh xe giao thông", "Photo Capture", "Chụp ảnh", "Free Prices", "Giá miễn phí",
                "DLC Gates", "Cổng DLC", "Install Flags", "Cờ cài đặt", "New Tags", "Thẻ mới",
                "Unlock Presets", "Mở khóa preset", "Unobtainable Gate", "Cổng xe ẩn", "Vehicle Tuner", "Chỉnh xe",
                "Car Class Panel", "Bảng hạng xe", "Stats Editor", "Chỉnh thống kê", "Restore All Car Classes", "Khôi phục mọi hạng xe",
                "Class Restrictions: OFF", "Giới hạn hạng: TẮT", "Class Restrictions: ON", "Giới hạn hạng: BẬT",
                "Disable Damage: OFF", "Tắt hư hại: TẮT", "Disable Damage: ON", "Tắt hư hại: BẬT");

            AddUiTranslations("Dutch",
                "Runtime Hooks", "Live-hooks", "Quick Tutorials", "Snelle handleidingen", "Feature How To Use", "Functies gebruiken",
                "Project Credits", "Projectcredits", "Setting", "Instelling", "Use Value", "Waarde gebruiken", "Save Preset", "Preset opslaan",
                "Load Preset", "Preset laden", "Apply Live Hooks", "Live-hooks toepassen", "Turn All Off", "Alles uitzetten", "Edit", "Bewerken",
                "Green", "Groen", "Red", "Rood", "No Build Limit", "Geen buildlimiet",
                "Freeze AI", "AI bevriezen", "FOV Slider", "FOV-schuif", "No Clip", "No Clip", "Speed Zone Multiplier", "Speedzone-multiplier",
                "Mission Timer Scale", "Missietimer-schaal", "Race Timer Scale", "Racetimer-schaal", "Time of Day", "Tijdstip",
                "No Water Drag", "Geen waterweerstand", "Super Handling", "Superbesturing", "Landing Stabilizer", "Landingsstabilisator",
                "Slide Calmer", "Slide-kalmering", "Road Magnet", "Wegmagneet", "Speed Tamer", "Snelheidsdemper", "Air Lift", "Luchtlift",
                "Bounce Cushion", "Stuiterkussen", "Momentum Control", "Momentumcontrole", "Left Right Calm", "Links/rechts rust",
                "Forward Back Calm", "Voor/achter rust", "Side Push", "Zijwaartse duw", "Forward Push", "Voorwaartse duw",
                "Vertical Trim", "Verticale trim", "Side Lock", "Zijslot", "Forward Lock", "Voorwaarts slot", "Vertical Hold", "Verticale hold",
                "Motion Freeze", "Beweging bevriezen", "Wheelie Boost", "Wheelie-boost", "Drift Kick", "Drift-kick",
                "Hover Glide", "Zweefglij", "Air Brake", "Luchtrem", "Corner Stabilizer", "Bochtstabilisator",
                "Planted Boost", "Gripboost", "Forward Launch", "Voorwaartse launch", "Straight Launch", "Rechte launch",
                "Tire Bite", "Bandengrip", "Grip Lock", "Grip-slot", "Forward Grip", "Voorwaartse grip", "Rail Grip", "Rail-grip",
                "High Speed Stabilizer", "Hogesnelheidsstabilisator", "Ground Clamp", "Grondklem", "Stability Rail", "Stabiliteitsrail",
                "Corner Bite", "Bochtgrip", "Spin Recovery", "Spinherstel",
                "Boost", "Boost", "Drift Mode", "Driftmodus", "Both Wheelspins", "Beide wheelspins",
                "Skill Points", "Skillpunten", "Wheelspin Odds", "Wheelspin-kansen", "Garage Favorites", "Garagefavorieten",
                "Traffic Editor", "Verkeerseditor", "Photo Capture", "Fotocapture", "Free Prices", "Gratis prijzen",
                "DLC Gates", "DLC-poorten", "Install Flags", "Installatievlaggen", "New Tags", "Nieuwe labels",
                "Unlock Presets", "Presets ontgrendelen", "Unobtainable Gate", "Verborgen poort", "Vehicle Tuner", "Voertuigtuner",
                "Car Class Panel", "Autoklassepaneel", "Stats Editor", "Statistiekeditor", "Restore All Car Classes", "Alle autoklassen herstellen",
                "Class Restrictions: OFF", "Klassebeperkingen: UIT", "Class Restrictions: ON", "Klassebeperkingen: AAN",
                "Disable Damage: OFF", "Schade uitschakelen: UIT", "Disable Damage: ON", "Schade uitschakelen: AAN");

            const string superStrengthTooltip = "Green arms Super Strength. Hold the selected key in-game to stabilize the car and shove forward through anything in the way.";
            const string superStrengthKeyTooltip = "Default is G. Hold this key to apply controlled unstoppable shove.";
            AddUiTranslations("Japanese", "Super Strength", "スーパー強化", superStrengthTooltip, "緑でスーパー強化を待機します。ゲーム内で選択したキーを押し続けると、車を安定させて前方へ強く押し出します。", superStrengthKeyTooltip, "初期値は G です。このキーを押し続けると制御された強力な押し出しを適用します。");
            AddUiTranslations("Chinese", "Super Strength", "超级力量", superStrengthTooltip, "绿色表示超级力量已待命。在游戏中按住所选按键，可稳定车辆并强力向前推进。", superStrengthKeyTooltip, "默认是 G。按住此键可应用受控的强力推进。");
            AddUiTranslations("Spanish", "Super Strength", "Super fuerza", superStrengthTooltip, "Verde arma Super fuerza. Mantén la tecla elegida en el juego para estabilizar el coche y empujarlo hacia delante con fuerza.", superStrengthKeyTooltip, "Predeterminado: G. Mantén esta tecla para aplicar un empuje imparable controlado.");
            AddUiTranslations("Arabic", "Super Strength", "قوة خارقة", superStrengthTooltip, "اللون الأخضر يجهز القوة الخارقة. اضغط مطولًا على المفتاح المحدد داخل اللعبة لتثبيت السيارة ودفعها بقوة للأمام.", superStrengthKeyTooltip, "الافتراضي هو G. اضغط مطولًا على هذا المفتاح لتطبيق دفع قوي ومتحكم به.");
            AddUiTranslations("Turkish", "Super Strength", "Süper güç", superStrengthTooltip, "Yeşil Süper gücü hazırlar. Arabayı sabitlemek ve güçlü biçimde ileri itmek için oyunda seçili tuşu basılı tut.", superStrengthKeyTooltip, "Varsayılan G. Kontrollü durdurulamaz itiş için bu tuşu basılı tut.");
            AddUiTranslations("Polish", "Super Strength", "Super siła", superStrengthTooltip, "Zielony uzbraja Super siłę. Przytrzymaj wybrany klawisz w grze, aby ustabilizować auto i mocno pchnąć je do przodu.", superStrengthKeyTooltip, "Domyślnie G. Przytrzymaj ten klawisz, aby użyć kontrolowanego, potężnego pchnięcia.");
            AddUiTranslations("German", "Super Strength", "Superkraft", superStrengthTooltip, "Grün aktiviert Superkraft. Halte die gewählte Taste im Spiel, um das Auto zu stabilisieren und kräftig nach vorn zu schieben.", superStrengthKeyTooltip, "Standard ist G. Halte diese Taste für einen kontrollierten, starken Vorwärtsschub.");
            AddUiTranslations("Swedish", "Super Strength", "Superstyrka", superStrengthTooltip, "Grön aktiverar Superstyrka. Håll den valda tangenten i spelet för att stabilisera bilen och skjuta den kraftigt framåt.", superStrengthKeyTooltip, "Standard är G. Håll tangenten för en kontrollerad, ostoppbar knuff.");
            AddUiTranslations("Farsi", "Super Strength", "قدرت فوق‌العاده", superStrengthTooltip, "رنگ سبز قدرت فوق‌العاده را آماده می‌کند. کلید انتخاب‌شده را در بازی نگه دارید تا خودرو پایدار شود و با قدرت به جلو رانده شود.", superStrengthKeyTooltip, "پیش‌فرض G است. این کلید را نگه دارید تا فشار قدرتمند و کنترل‌شده اعمال شود.");
            AddUiTranslations("French", "Super Strength", "Super force", superStrengthTooltip, "Le vert arme Super force. Maintiens la touche choisie en jeu pour stabiliser la voiture et la pousser fortement vers l'avant.", superStrengthKeyTooltip, "Par défaut : G. Maintiens cette touche pour appliquer une poussée puissante et contrôlée.");
            AddUiTranslations("Lithuanian", "Super Strength", "Super jėga", superStrengthTooltip, "Žalia įjungia Super jėgą. Laikyk pasirinktą klavišą žaidime, kad automobilis būtų stabilus ir stipriai stumiamas pirmyn.", superStrengthKeyTooltip, "Numatyta G. Laikyk šį klavišą, kad pritaikytum valdomą galingą stūmimą.");
            AddUiTranslations("Portuguese", "Super Strength", "Super força", superStrengthTooltip, "Verde arma a Super força. Segure a tecla escolhida no jogo para estabilizar o carro e empurrar forte para frente.", superStrengthKeyTooltip, "Padrao: G. Segure esta tecla para aplicar um empurrao controlado e poderoso.");
            AddUiTranslations("Indonesian", "Super Strength", "Kekuatan super", superStrengthTooltip, "Hijau mengaktifkan Kekuatan super. Tahan tombol pilihan di game untuk menstabilkan mobil dan mendorong kuat ke depan.", superStrengthKeyTooltip, "Default G. Tahan tombol ini untuk dorongan kuat yang tetap terkontrol.");
            AddUiTranslations("Georgian", "Super Strength", "სუპერ ძალა", superStrengthTooltip, "მწვანე რთავს სუპერ ძალას. თამაშში არჩეული ღილაკის დაჭერით მანქანა დასტაბილურდება და ძლიერად წავა წინ.", superStrengthKeyTooltip, "ნაგულისხმებია G. დააჭირე ამ ღილაკს კონტროლირებადი ძლიერი ბიძგისთვის.");
            AddUiTranslations("Vietnamese", "Super Strength", "Siêu sức mạnh", superStrengthTooltip, "Màu xanh bật Siêu sức mạnh. Giữ phím đã chọn trong game để ổn định xe và đẩy mạnh về phía trước.", superStrengthKeyTooltip, "Mặc định là G. Giữ phím này để tạo lực đẩy mạnh nhưng có kiểm soát.");
            AddUiTranslations("Dutch", "Super Strength", "Superkracht", superStrengthTooltip, "Groen zet Superkracht klaar. Houd de gekozen toets in de game ingedrukt om de auto te stabiliseren en hard vooruit te duwen.", superStrengthKeyTooltip, "Standaard is G. Houd deze toets vast voor een gecontroleerde, krachtige duw.");

            const string phaseDashTooltip = "Green arms Phase Dash. Press the selected key to teleport the car forward by the configured distance.";
            const string phaseDashKeyTooltip = "Default is H. Press this key to dash forward by the configured distance.";
            AddUiTranslations("Japanese", "Phase Dash", "フェーズダッシュ", phaseDashTooltip, "緑でフェーズダッシュを待機します。選択したキーを押すと、設定した距離だけ車を前方へテレポートします。", phaseDashKeyTooltip, "初期値は H です。このキーを押すと、設定した距離だけ前方へダッシュします。");
            AddUiTranslations("Chinese", "Phase Dash", "相位冲刺", phaseDashTooltip, "绿色表示相位冲刺已待命。按下所选按键会按设置距离将车辆向前传送。", phaseDashKeyTooltip, "默认是 H。按下此键会按设置距离向前冲刺。");
            AddUiTranslations("Spanish", "Phase Dash", "Impulso fase", phaseDashTooltip, "Verde arma Impulso fase. Pulsa la tecla elegida para teletransportar el coche hacia delante la distancia configurada.", phaseDashKeyTooltip, "Predeterminado: H. Pulsa esta tecla para avanzar la distancia configurada.");
            AddUiTranslations("Arabic", "Phase Dash", "اندفاع الطور", phaseDashTooltip, "اللون الأخضر يجهز اندفاع الطور. اضغط المفتاح المحدد لنقل السيارة للأمام حسب المسافة المضبوطة.", phaseDashKeyTooltip, "الافتراضي هو H. اضغط هذا المفتاح للاندفاع للأمام حسب المسافة المضبوطة.");
            AddUiTranslations("Turkish", "Phase Dash", "Faz atılışı", phaseDashTooltip, "Yeşil Faz atılışını hazırlar. Seçili tuşa basınca araç ayarlanan mesafe kadar ileri ışınlanır.", phaseDashKeyTooltip, "Varsayılan H. Ayarlanan mesafe kadar ileri atılmak için bu tuşa bas.");
            AddUiTranslations("Polish", "Phase Dash", "Przeskok fazowy", phaseDashTooltip, "Zielony uzbraja Przeskok fazowy. Naciśnij wybrany klawisz, aby przenieść auto do przodu o ustawioną odległość.", phaseDashKeyTooltip, "Domyślnie H. Naciśnij ten klawisz, aby skoczyć do przodu o ustawioną odległość.");
            AddUiTranslations("German", "Phase Dash", "Phasen-Dash", phaseDashTooltip, "Grün aktiviert Phasen-Dash. Drücke die gewählte Taste, um das Auto um die eingestellte Distanz nach vorn zu teleportieren.", phaseDashKeyTooltip, "Standard ist H. Drücke diese Taste, um um die eingestellte Distanz nach vorn zu dashen.");
            AddUiTranslations("Swedish", "Phase Dash", "Fasdash", phaseDashTooltip, "Grön aktiverar Fasdash. Tryck vald tangent för att teleportera bilen framåt med den inställda distansen.", phaseDashKeyTooltip, "Standard är H. Tryck tangenten för att dasha framåt med den inställda distansen.");
            AddUiTranslations("Farsi", "Phase Dash", "داش فازی", phaseDashTooltip, "رنگ سبز داش فازی را آماده می‌کند. کلید انتخاب‌شده را بزنید تا خودرو به اندازه فاصله تنظیم‌شده به جلو تلپورت شود.", phaseDashKeyTooltip, "پیش‌فرض H است. این کلید را بزنید تا به اندازه فاصله تنظیم‌شده به جلو حرکت کنید.");
            AddUiTranslations("French", "Phase Dash", "Dash phase", phaseDashTooltip, "Le vert arme Dash phase. Appuie sur la touche choisie pour téléporter la voiture vers l'avant selon la distance réglée.", phaseDashKeyTooltip, "Par défaut : H. Appuie sur cette touche pour avancer selon la distance réglée.");
            AddUiTranslations("Lithuanian", "Phase Dash", "Fazinis šuolis", phaseDashTooltip, "Žalia įjungia Fazinį šuolį. Paspausk pasirinktą klavišą, kad automobilis būtų perkeltas pirmyn nustatytu atstumu.", phaseDashKeyTooltip, "Numatyta H. Paspausk šį klavišą, kad šoktum pirmyn nustatytu atstumu.");
            AddUiTranslations("Portuguese", "Phase Dash", "Dash de fase", phaseDashTooltip, "Verde arma o Dash de fase. Pressione a tecla escolhida para teleportar o carro para frente pela distancia configurada.", phaseDashKeyTooltip, "Padrao: H. Pressione esta tecla para avancar pela distancia configurada.");
            AddUiTranslations("Indonesian", "Phase Dash", "Dash fase", phaseDashTooltip, "Hijau mengaktifkan Dash fase. Tekan tombol pilihan untuk memindahkan mobil maju sesuai jarak yang diatur.", phaseDashKeyTooltip, "Default H. Tekan tombol ini untuk dash maju sesuai jarak yang diatur.");
            AddUiTranslations("Georgian", "Phase Dash", "ფაზური დაში", phaseDashTooltip, "მწვანე ამზადებს ფაზურ დაშს. დააჭირე არჩეულ ღილაკს, რომ მანქანა წინ გადავიდეს დაყენებული მანძილით.", phaseDashKeyTooltip, "ნაგულისხმებია H. დააჭირე ამ ღილაკს წინ გადასასვლელად დაყენებული მანძილით.");
            AddUiTranslations("Vietnamese", "Phase Dash", "Lướt xuyên pha", phaseDashTooltip, "Màu xanh bật Lướt xuyên pha. Nhấn phím đã chọn để dịch chuyển xe về phía trước theo khoảng cách đã đặt.", phaseDashKeyTooltip, "Mặc định là H. Nhấn phím này để lao tới theo khoảng cách đã đặt.");
            AddUiTranslations("Dutch", "Phase Dash", "Fasedash", phaseDashTooltip, "Groen zet Fasedash klaar. Druk op de gekozen toets om de auto de ingestelde afstand vooruit te teleporteren.", phaseDashKeyTooltip, "Standaard is H. Druk op deze toets om de ingestelde afstand vooruit te dashen.");

            AddUiTranslations("Spanish",
                "Teleport", "Teletransporte", "Console", "Consola", "Tutorial", "Tutorial", "Runtime Hooks", "Hooks en vivo",
                "Quick Tutorials", "Tutoriales rapidos", "Feature How To Use", "Como usar funciones", "Project Credits", "Creditos del proyecto",
                "Setting", "Ajuste", "What it does", "Que hace", "Strength", "Fuerza", "Toggle", "Activar", "Ready", "Listo",
                "Use Value", "Usar valor", "Cancel", "Cancelar", "Save Preset", "Guardar preset", "Load Preset", "Cargar preset",
                "Apply Live Hooks", "Aplicar hooks", "Turn All Off", "Apagar todo",
                "No Build Limit", "Sin limite build", "Speed Zone Multiplier", "Multiplicador zona", "FOV Slider", "FOV",
                "No Clip", "No Clip", "Super Handling", "Super manejo", "Landing Stabilizer", "Estabilizador aterrizaje",
                "Slide Calmer", "Calmar derrape", "Road Magnet", "Iman de carretera", "Air Brake", "Freno de aire",
                "Forward Launch", "Impulso frontal", "Tire Bite", "Mordida de llanta", "Corner Stabilizer", "Estabilizador curva");

            AddUiTranslations("Portuguese",
                "Teleport", "Teleporte", "Console", "Console", "Runtime Hooks", "Hooks ao vivo", "Quick Tutorials", "Tutoriais rapidos",
                "Feature How To Use", "Como usar recursos", "Setting", "Opcao", "What it does", "O que faz", "Strength", "Forca",
                "Toggle", "Liga", "Ready", "Pronto", "Use Value", "Usar valor", "Cancel", "Cancelar", "Save Preset", "Salvar preset",
                "Load Preset", "Carregar preset", "Apply Live Hooks", "Aplicar hooks", "Turn All Off", "Desligar tudo",
                "No Build Limit", "Sem limite build", "Super Handling", "Super controle",
                "Landing Stabilizer", "Estabilizador pouso", "Slide Calmer", "Reduz deslize", "Road Magnet", "Ima de estrada",
                "Forward Launch", "Arranque frontal", "Air Brake", "Freio aereo");

            AddUiTranslations("French",
                "Teleport", "Teleportation", "Console", "Console", "Runtime Hooks", "Hooks actifs", "Quick Tutorials", "Tutoriels rapides",
                "Feature How To Use", "Utiliser les fonctions", "Setting", "Reglage", "What it does", "Effet", "Strength", "Force",
                "Toggle", "Actif", "Ready", "Pret", "Use Value", "Utiliser", "Cancel", "Annuler", "Save Preset", "Sauver preset",
                "Load Preset", "Charger preset", "Apply Live Hooks", "Appliquer hooks", "Turn All Off", "Tout eteindre",
                "No Build Limit", "Sans limite build", "Forward Launch", "Lancement avant",
                "Air Brake", "Frein aerien");

            AddUiTranslations("German",
                "Teleport", "Teleport", "Console", "Konsole", "Runtime Hooks", "Live-Hooks", "Quick Tutorials", "Kurzanleitungen",
                "Feature How To Use", "Funktionen nutzen", "Setting", "Einstellung", "What it does", "Wirkung", "Strength", "Staerke",
                "Toggle", "Schalter", "Ready", "Bereit", "Use Value", "Wert nutzen", "Cancel", "Abbrechen", "Save Preset", "Preset speichern",
                "Load Preset", "Preset laden", "Apply Live Hooks", "Live anwenden", "Turn All Off", "Alles aus",
                "No Build Limit", "Kein Buildlimit", "Forward Launch", "Vorwaertsstart",
                "Air Brake", "Luftbremse");

            AddUiTranslations("Indonesian",
                "Runtime Hooks", "Hook live", "Quick Tutorials", "Tutorial cepat", "Feature How To Use", "Cara pakai fitur",
                "Setting", "Pengaturan", "What it does", "Fungsi", "Strength", "Kekuatan", "Toggle", "Aktif",
                "Ready", "Siap", "Use Value", "Pakai nilai", "Cancel", "Batal", "Apply Live Hooks", "Terapkan hook",
                "Forward Launch", "Dorongan maju", "Air Brake", "Rem udara");

            AddUiTranslations("Spanish",
                "Attached", "Conectado", "Not attached", "No conectado", "running...", "ejecutando...", "success", "correcto", "failed", "falló",
                "Back", "Atrás", "Add Selected", "Añadir seleccionados", "Add All", "Añadir todo", "Remove Selected", "Quitar seleccionados",
                "Confirm", "Confirmar", "Close", "Cerrar", "OK", "Aceptar", "Save", "Guardar", "Browse", "Examinar", "Pick", "Elegir",
                "Display language", "Idioma de pantalla", "Language", "Idioma", "Color Theme", "Tema de color", "Connection Status", "Estado de conexión",
                "Database Actions", "Acciones de base de datos", "Quick Actions", "Acciones rápidas", "Editors", "Editores", "Tables & Unlocks", "Tablas y desbloqueos",
                "Car ID", "ID de coche", "Year", "Año", "Make", "Marca", "Model", "Modelo", "Class", "Clase", "Copies", "Copias",
                "State", "Estado", "Status", "Estado", "Value", "Valor", "Current", "Actual", "Default", "Predeterminado", "Description", "Descripción",
                "Name", "Nombre", "File", "Archivo", "Select", "Seleccionar", "Owned", "Propio", "Price", "Precio", "Quantity", "Cantidad",
                "Restore All Car Classes", "Restaurar clases", "Class Restrictions: OFF", "Restricciones: OFF", "Class Restrictions: ON", "Restricciones: ON");

            AddUiTranslations("Portuguese",
                "Attached", "Conectado", "Not attached", "Nao conectado", "running...", "executando...", "success", "sucesso", "failed", "falhou",
                "Back", "Voltar", "Add Selected", "Adicionar selecionados", "Add All", "Adicionar tudo", "Remove Selected", "Remover selecionados",
                "Confirm", "Confirmar", "Close", "Fechar", "OK", "OK", "Save", "Salvar", "Browse", "Procurar", "Pick", "Escolher",
                "Display language", "Idioma da tela", "Language", "Idioma", "Color Theme", "Tema de cores", "Connection Status", "Status da conexao",
                "Database Actions", "Acoes de banco", "Quick Actions", "Acoes rapidas", "Editors", "Editores", "Tables & Unlocks", "Tabelas e desbloqueios",
                "Car ID", "ID do carro", "Year", "Ano", "Make", "Marca", "Model", "Modelo", "Class", "Classe", "Copies", "Copias",
                "State", "Estado", "Status", "Status", "Value", "Valor", "Current", "Atual", "Default", "Padrao", "Description", "Descricao",
                "Name", "Nome", "File", "Arquivo", "Select", "Selecionar", "Owned", "Possuido", "Price", "Preco", "Quantity", "Quantidade",
                "Restore All Car Classes", "Restaurar classes", "Class Restrictions: OFF", "Restricoes: OFF", "Class Restrictions: ON", "Restricoes: ON");

            AddUiTranslations("French",
                "Attached", "Connecté", "Not attached", "Non connecté", "running...", "en cours...", "success", "réussi", "failed", "échoué",
                "Back", "Retour", "Add Selected", "Ajouter la sélection", "Add All", "Tout ajouter", "Remove Selected", "Retirer la sélection",
                "Confirm", "Confirmer", "Close", "Fermer", "OK", "OK", "Save", "Enregistrer", "Browse", "Parcourir", "Pick", "Choisir",
                "Display language", "Langue d'affichage", "Language", "Langue", "Color Theme", "Thème de couleur", "Connection Status", "État de connexion",
                "Database Actions", "Actions base de données", "Quick Actions", "Actions rapides", "Editors", "Éditeurs", "Tables & Unlocks", "Tables et déblocages",
                "Car ID", "ID voiture", "Year", "Année", "Make", "Marque", "Model", "Modèle", "Class", "Classe", "Copies", "Copies",
                "State", "État", "Status", "Statut", "Value", "Valeur", "Current", "Actuel", "Default", "Par défaut", "Description", "Description",
                "Name", "Nom", "File", "Fichier", "Select", "Sélectionner", "Owned", "Possédé", "Price", "Prix", "Quantity", "Quantité",
                "Restore All Car Classes", "Restaurer les classes", "Class Restrictions: OFF", "Restrictions: OFF", "Class Restrictions: ON", "Restrictions: ON");

            AddUiTranslations("German",
                "Attached", "Verbunden", "Not attached", "Nicht verbunden", "running...", "läuft...", "success", "erfolgreich", "failed", "fehlgeschlagen",
                "Back", "Zurück", "Add Selected", "Ausgewählte hinzufügen", "Add All", "Alle hinzufügen", "Remove Selected", "Ausgewählte entfernen",
                "Confirm", "Bestätigen", "Close", "Schließen", "OK", "OK", "Save", "Speichern", "Browse", "Durchsuchen", "Pick", "Wählen",
                "Display language", "Anzeigesprache", "Language", "Sprache", "Color Theme", "Farbdesign", "Connection Status", "Verbindungsstatus",
                "Database Actions", "Datenbankaktionen", "Quick Actions", "Schnellaktionen", "Editors", "Editoren", "Tables & Unlocks", "Tabellen und Freischaltungen",
                "Car ID", "Auto-ID", "Year", "Jahr", "Make", "Marke", "Model", "Modell", "Class", "Klasse", "Copies", "Kopien",
                "State", "Status", "Status", "Status", "Value", "Wert", "Current", "Aktuell", "Default", "Standard", "Description", "Beschreibung",
                "Name", "Name", "File", "Datei", "Select", "Auswählen", "Owned", "Besessen", "Price", "Preis", "Quantity", "Anzahl",
                "Restore All Car Classes", "Alle Klassen wiederherstellen", "Class Restrictions: OFF", "Klassenbeschränkungen: AUS", "Class Restrictions: ON", "Klassenbeschränkungen: EIN");

            AddUiTranslations("Farsi",
                "Attached", "متصل", "Not attached", "متصل نیست", "running...", "در حال اجرا...", "success", "موفق", "failed", "ناموفق",
                "Back", "بازگشت", "Add Selected", "افزودن انتخاب‌شده", "Add All", "افزودن همه", "Remove Selected", "حذف انتخاب‌شده",
                "Confirm", "تأیید", "Close", "بستن", "OK", "باشه", "Save", "ذخیره", "Pick", "انتخاب",
                "Display language", "زبان نمایش", "Language", "زبان", "Color Theme", "رنگ‌بندی", "Connection Status", "وضعیت اتصال",
                "Car ID", "شناسه خودرو", "Year", "سال", "Make", "سازنده", "Model", "مدل", "Class", "کلاس", "Copies", "کپی‌ها",
                "State", "وضعیت", "Status", "وضعیت", "Value", "مقدار", "Current", "فعلی", "Default", "پیش‌فرض", "Description", "توضیح",
                "Name", "نام", "File", "فایل", "Select", "انتخاب", "Owned", "دارایی", "Price", "قیمت", "Quantity", "تعداد");

            AddUiTranslations("Lithuanian",
                "Attached", "Prisijungta", "Not attached", "Neprisijungta", "running...", "vykdoma...", "success", "pavyko", "failed", "nepavyko",
                "Back", "Atgal", "Add Selected", "Pridėti pasirinktus", "Add All", "Pridėti viską", "Remove Selected", "Pašalinti pasirinktus",
                "Confirm", "Patvirtinti", "Close", "Uždaryti", "OK", "Gerai", "Save", "Išsaugoti", "Browse", "Naršyti", "Pick", "Pasirinkti",
                "Display language", "Rodymo kalba", "Language", "Kalba", "Color Theme", "Spalvų tema", "Connection Status", "Prisijungimo būsena",
                "Database Actions", "Duomenų veiksmai", "Quick Actions", "Greiti veiksmai", "Editors", "Redaktoriai", "Tables & Unlocks", "Lentelės ir atrakinimai",
                "Car ID", "Auto ID", "Year", "Metai", "Make", "Gamintojas", "Model", "Modelis", "Class", "Klasė", "Copies", "Kopijos",
                "State", "Būsena", "Status", "Būsena", "Value", "Reikšmė", "Current", "Dabartinė", "Default", "Numatyta", "Description", "Aprašas",
                "Name", "Pavadinimas", "File", "Failas", "Select", "Pasirinkti", "Owned", "Turima", "Price", "Kaina", "Quantity", "Kiekis");

            AddUiTranslations("Indonesian",
                "Attached", "Terhubung", "Not attached", "Belum terhubung", "running...", "berjalan...", "success", "berhasil", "failed", "gagal",
                "Back", "Kembali", "Add Selected", "Tambah pilihan", "Add All", "Tambah semua", "Remove Selected", "Hapus pilihan",
                "Confirm", "Konfirmasi", "Close", "Tutup", "OK", "OK", "Save", "Simpan", "Browse", "Telusuri", "Pick", "Pilih",
                "Display language", "Bahasa tampilan", "Language", "Bahasa", "Color Theme", "Tema warna", "Connection Status", "Status koneksi",
                "Database Actions", "Aksi database", "Quick Actions", "Aksi cepat", "Editors", "Editor", "Tables & Unlocks", "Tabel & buka kunci",
                "Car ID", "ID mobil", "Year", "Tahun", "Make", "Merek", "Model", "Model", "Class", "Kelas", "Copies", "Salinan",
                "State", "Status", "Status", "Status", "Value", "Nilai", "Current", "Saat ini", "Default", "Bawaan", "Description", "Deskripsi",
                "Name", "Nama", "File", "File", "Select", "Pilih", "Owned", "Dimiliki", "Price", "Harga", "Quantity", "Jumlah");

            AddUiTranslations("Georgian",
                "Attached", "დაკავშირებულია", "Not attached", "არ არის დაკავშირებული", "running...", "მუშაობს...", "success", "წარმატებულია", "failed", "ვერ შესრულდა",
                "Back", "უკან", "Add Selected", "არჩეულის დამატება", "Add All", "ყველას დამატება", "Remove Selected", "არჩეულის წაშლა",
                "Confirm", "დადასტურება", "Close", "დახურვა", "OK", "OK", "Save", "შენახვა", "Browse", "დათვალიერება", "Pick", "არჩევა",
                "Display language", "ჩვენების ენა", "Language", "ენა", "Color Theme", "ფერის თემა", "Connection Status", "კავშირის სტატუსი",
                "Car ID", "მანქანის ID", "Year", "წელი", "Make", "მწარმოებელი", "Model", "მოდელი", "Class", "კლასი", "Copies", "ასლები",
                "State", "მდგომარეობა", "Status", "სტატუსი", "Value", "მნიშვნელობა", "Current", "მიმდინარე", "Default", "ნაგულისხმები", "Description", "აღწერა",
                "Name", "სახელი", "File", "ფაილი", "Select", "არჩევა", "Owned", "ფლობაშია", "Price", "ფასი", "Quantity", "რაოდენობა");

            AddUiTranslations("Chinese", "Credits", "鸣谢");
            AddUiTranslations("Arabic", "Credits", "الشكر");
            AddUiTranslations("Turkish", "Credits", "Katkılar");
            AddUiTranslations("Polish", "Credits", "Podziękowania");
            AddUiTranslations("Farsi", "Credits", "قدردانی");
            AddUiTranslations("Lithuanian", "Credits", "Padėkos");
            AddUiTranslations("Indonesian", "Credits", "Ucapan Terima Kasih");
            AddUiTranslations("Georgian", "Credits", "მადლობები");

            const string patchCreditSubtitle = "A special thanks for Patch crafting teleport areas for the community!";
            AddUiTranslations("Japanese", patchCreditSubtitle, "コミュニティのためにテレポート エリアを作成した Patch に心より感謝いたします。");
            AddUiTranslations("Chinese", patchCreditSubtitle, "特别感谢 Patch 为社区制作传送区域！");
            AddUiTranslations("Spanish", patchCreditSubtitle, "¡Un agradecimiento especial a Patch por crear áreas de teletransporte para la comunidad!");
            AddUiTranslations("Arabic", patchCreditSubtitle, "شكر خاص لـ Patch على إنشاء مناطق انتقال فوري للمجتمع!");
            AddUiTranslations("Turkish", patchCreditSubtitle, "Topluluk için ışınlanma alanları hazırlayan Patch'e özellikle teşekkür ederiz!");
            AddUiTranslations("Polish", patchCreditSubtitle, "Specjalne podziękowania dla Patcha za stworzenie obszarów teleportacji dla społeczności!");
            AddUiTranslations("German", patchCreditSubtitle, "Ein besonderer Dank geht an Patch, der Teleportbereiche für die Community erstellt hat!");
            AddUiTranslations("Swedish", patchCreditSubtitle, "Ett särskilt tack till Patch för att ha skapat teleporteringsområden för communityn!");
            AddUiTranslations("Farsi", patchCreditSubtitle, "یک تشکر ویژه از Patch برای ساخت مناطق تلپورت برای جامعه!");
            AddUiTranslations("French", patchCreditSubtitle, "Un merci tout spécial à Patch pour avoir créé des zones de téléportation pour la communauté !");
            AddUiTranslations("Lithuanian", patchCreditSubtitle, "Ypatingas ačiū Patch, kuris bendruomenei sukūrė teleportavimo sritis!");
            AddUiTranslations("Portuguese", patchCreditSubtitle, "Um agradecimento especial a Patch por criar áreas de teletransporte para a comunidade!");
            AddUiTranslations("Indonesian", patchCreditSubtitle, "Terima kasih khusus kepada Patch yang membuat area teleportasi untuk komunitas!");
            AddUiTranslations("Georgian", patchCreditSubtitle, "განსაკუთრებული მადლობა Patch-ს საზოგადოებისთვის ტელეპორტის ზონების შექმნისთვის!");
            AddUiTranslations("Vietnamese", patchCreditSubtitle, "Xin gửi lời cảm ơn đặc biệt đến Patch đã tạo các khu vực dịch chuyển tức thời cho cộng đồng!");
            AddUiTranslations("Dutch", patchCreditSubtitle, "Een speciale dank aan Patch voor het maken van teleportgebieden voor de community!");

            const string merikaCreditSubtitle = "Tuning Reference";
            AddUiTranslations("Japanese", merikaCreditSubtitle, "チューニング参考");
            AddUiTranslations("Chinese", merikaCreditSubtitle, "调校参考");
            AddUiTranslations("Spanish", merikaCreditSubtitle, "Referencia de ajuste");
            AddUiTranslations("Arabic", merikaCreditSubtitle, "مرجع الضبط");
            AddUiTranslations("Turkish", merikaCreditSubtitle, "Ayar referansı");
            AddUiTranslations("Polish", merikaCreditSubtitle, "Referencja tuningu");
            AddUiTranslations("German", merikaCreditSubtitle, "Tuning-Referenz");
            AddUiTranslations("Swedish", merikaCreditSubtitle, "Tuningreferens");
            AddUiTranslations("Farsi", merikaCreditSubtitle, "مرجع تیونینگ");
            AddUiTranslations("French", merikaCreditSubtitle, "Référence de réglage");
            AddUiTranslations("Lithuanian", merikaCreditSubtitle, "Derinimo nuoroda");
            AddUiTranslations("Portuguese", merikaCreditSubtitle, "Referencia de ajuste");
            AddUiTranslations("Indonesian", merikaCreditSubtitle, "Referensi tuning");
            AddUiTranslations("Georgian", merikaCreditSubtitle, "ტიუნინგის მინიშნება");
            AddUiTranslations("Vietnamese", merikaCreditSubtitle, "Tham chiếu tinh chỉnh");
            AddUiTranslations("Dutch", merikaCreditSubtitle, "Tuningreferentie");

            AddUiTranslations("Japanese", "Internal Actions", "内部アクション");
            AddUiTranslations("Chinese", "Internal Actions", "内部操作");
            AddUiTranslations("Spanish", "Internal Actions", "Acciones internas");
            AddUiTranslations("Arabic", "Internal Actions", "إجراءات داخلية");
            AddUiTranslations("Turkish", "Internal Actions", "Dahili işlemler");
            AddUiTranslations("Polish", "Internal Actions", "Akcje wewnętrzne");
            AddUiTranslations("German", "Internal Actions", "Interne Aktionen");
            AddUiTranslations("Swedish", "Internal Actions", "Interna åtgärder");
            AddUiTranslations("Farsi", "Internal Actions", "اقدامات داخلی");
            AddUiTranslations("French", "Internal Actions", "Actions internes");
            AddUiTranslations("Lithuanian", "Internal Actions", "Vidiniai veiksmai");
            AddUiTranslations("Portuguese", "Internal Actions", "Acoes internas");
            AddUiTranslations("Indonesian", "Internal Actions", "Aksi internal");
            AddUiTranslations("Georgian", "Internal Actions", "შიდა მოქმედებები");
            AddUiTranslations("Vietnamese", "Internal Actions", "Hành động nội bộ");
            AddUiTranslations("Dutch", "Internal Actions", "Interne acties");

            AddUiTranslations("Japanese", "Tuning", "チューニング", "Driving Tuning", "ドライビング調整", "Tuning Actions", "チューニング操作", "Driving Tuning Actions", "ドライビング調整操作", "Live Tuning Table", "ライブチューニング表", "Driving Tuning Table", "ドライビング調整表", "Scan Tuning", "チューニング検索", "Start Live Scan", "ライブスキャン開始", "Stop Live Scan", "ライブスキャン停止", "Restore Snapshot", "スナップ復元");
            AddUiTranslations("Chinese", "Tuning", "调校", "Driving Tuning", "驾驶调校", "Tuning Actions", "调校操作", "Driving Tuning Actions", "驾驶调校操作", "Live Tuning Table", "实时调校表", "Driving Tuning Table", "驾驶调校表", "Scan Tuning", "扫描调校", "Start Live Scan", "开始实时扫描", "Stop Live Scan", "停止实时扫描", "Restore Snapshot", "恢复快照");
            AddUiTranslations("Spanish", "Tuning", "Ajuste", "Driving Tuning", "Ajuste de conducción", "Tuning Actions", "Acciones de ajuste", "Driving Tuning Actions", "Acciones de conducción", "Live Tuning Table", "Tabla de ajuste en vivo", "Driving Tuning Table", "Tabla de conducción", "Scan Tuning", "Escanear ajuste", "Start Live Scan", "Iniciar escaneo", "Stop Live Scan", "Detener escaneo", "Restore Snapshot", "Restaurar copia");
            AddUiTranslations("Arabic", "Tuning", "الضبط", "Driving Tuning", "ضبط القيادة", "Tuning Actions", "إجراءات الضبط", "Driving Tuning Actions", "إجراءات ضبط القيادة", "Live Tuning Table", "جدول الضبط المباشر", "Driving Tuning Table", "جدول ضبط القيادة", "Scan Tuning", "فحص الضبط", "Start Live Scan", "بدء الفحص المباشر", "Stop Live Scan", "إيقاف الفحص المباشر", "Restore Snapshot", "استعادة اللقطة");
            AddUiTranslations("Turkish", "Tuning", "Ayar", "Driving Tuning", "Sürüş ayarı", "Tuning Actions", "Ayar işlemleri", "Driving Tuning Actions", "Sürüş ayarı işlemleri", "Live Tuning Table", "Canlı ayar tablosu", "Driving Tuning Table", "Sürüş ayarı tablosu", "Scan Tuning", "Ayar tara", "Start Live Scan", "Canlı taramayı başlat", "Stop Live Scan", "Canlı taramayı durdur", "Restore Snapshot", "Anlık yedek");
            AddUiTranslations("Polish", "Tuning", "Tuning", "Driving Tuning", "Tuning jazdy", "Tuning Actions", "Akcje tuningu", "Driving Tuning Actions", "Akcje tuningu jazdy", "Live Tuning Table", "Tabela tuningu live", "Driving Tuning Table", "Tabela tuningu jazdy", "Scan Tuning", "Skanuj tuning", "Start Live Scan", "Start skanu live", "Stop Live Scan", "Stop skanu live", "Restore Snapshot", "Przywróć zapis");
            AddUiTranslations("German", "Tuning", "Tuning", "Driving Tuning", "Fahr-Tuning", "Tuning Actions", "Tuning-Aktionen", "Driving Tuning Actions", "Fahr-Tuning-Aktionen", "Live Tuning Table", "Live-Tuning-Tabelle", "Driving Tuning Table", "Fahr-Tuning-Tabelle", "Scan Tuning", "Tuning suchen", "Start Live Scan", "Live-Scan starten", "Stop Live Scan", "Live-Scan stoppen", "Restore Snapshot", "Snapshot wiederherstellen");
            AddUiTranslations("Swedish", "Tuning", "Tuning", "Driving Tuning", "Körtuning", "Tuning Actions", "Tuningåtgärder", "Driving Tuning Actions", "Körtuningåtgärder", "Live Tuning Table", "Live tuningtabell", "Driving Tuning Table", "Körtuningtabell", "Scan Tuning", "Skanna tuning", "Start Live Scan", "Starta liveskanning", "Stop Live Scan", "Stoppa liveskanning", "Restore Snapshot", "Återställ snapshot");
            AddUiTranslations("Farsi", "Tuning", "تیونینگ", "Driving Tuning", "تیونینگ رانندگی", "Tuning Actions", "اقدام‌های تیونینگ", "Driving Tuning Actions", "اقدام‌های تیونینگ رانندگی", "Live Tuning Table", "جدول تیونینگ زنده", "Driving Tuning Table", "جدول تیونینگ رانندگی", "Scan Tuning", "اسکن تیونینگ", "Start Live Scan", "شروع اسکن زنده", "Stop Live Scan", "توقف اسکن زنده", "Restore Snapshot", "بازیابی اسنپ‌شات");
            AddUiTranslations("French", "Tuning", "Réglage", "Driving Tuning", "Réglage conduite", "Tuning Actions", "Actions de réglage", "Driving Tuning Actions", "Actions de conduite", "Live Tuning Table", "Table de réglage live", "Driving Tuning Table", "Table de conduite", "Scan Tuning", "Scanner réglage", "Start Live Scan", "Démarrer le scan live", "Stop Live Scan", "Arrêter le scan live", "Restore Snapshot", "Restaurer capture");
            AddUiTranslations("Lithuanian", "Tuning", "Derinimas", "Driving Tuning", "Vairavimo derinimas", "Tuning Actions", "Derinimo veiksmai", "Driving Tuning Actions", "Vairavimo derinimo veiksmai", "Live Tuning Table", "Tiesioginio derinimo lentelė", "Driving Tuning Table", "Vairavimo derinimo lentelė", "Scan Tuning", "Skenuoti derinimą", "Start Live Scan", "Pradėti tiesioginį skenavimą", "Stop Live Scan", "Stabdyti tiesioginį skenavimą", "Restore Snapshot", "Atkurti kopiją");
            AddUiTranslations("Portuguese", "Tuning", "Ajuste", "Driving Tuning", "Ajuste de condução", "Tuning Actions", "Acoes de ajuste", "Driving Tuning Actions", "Acoes de condução", "Live Tuning Table", "Tabela de ajuste ao vivo", "Driving Tuning Table", "Tabela de condução", "Scan Tuning", "Escanear ajuste", "Start Live Scan", "Iniciar scan ao vivo", "Stop Live Scan", "Parar scan ao vivo", "Restore Snapshot", "Restaurar snapshot");
            AddUiTranslations("Indonesian", "Tuning", "Tuning", "Driving Tuning", "Tuning mengemudi", "Tuning Actions", "Aksi tuning", "Driving Tuning Actions", "Aksi tuning mengemudi", "Live Tuning Table", "Tabel tuning live", "Driving Tuning Table", "Tabel tuning mengemudi", "Scan Tuning", "Pindai tuning", "Start Live Scan", "Mulai pindai live", "Stop Live Scan", "Hentikan pindai live", "Restore Snapshot", "Pulihkan snapshot");
            AddUiTranslations("Georgian", "Tuning", "ტიუნინგი", "Driving Tuning", "მართვის ტიუნინგი", "Tuning Actions", "ტიუნინგის მოქმედებები", "Driving Tuning Actions", "მართვის ტიუნინგის მოქმედებები", "Live Tuning Table", "ცოცხალი ტიუნინგის ცხრილი", "Driving Tuning Table", "მართვის ტიუნინგის ცხრილი", "Scan Tuning", "ტიუნინგის სკანირება", "Start Live Scan", "ცოცხალი სკანის დაწყება", "Stop Live Scan", "ცოცხალი სკანის გაჩერება", "Restore Snapshot", "სნეპშოტის აღდგენა");
            AddUiTranslations("Vietnamese", "Tuning", "Tinh chỉnh", "Driving Tuning", "Tinh chỉnh lái xe", "Tuning Actions", "Thao tác tinh chỉnh", "Driving Tuning Actions", "Thao tác tinh chỉnh lái xe", "Live Tuning Table", "Bảng tinh chỉnh trực tiếp", "Driving Tuning Table", "Bảng tinh chỉnh lái xe", "Scan Tuning", "Quét tinh chỉnh", "Start Live Scan", "Bắt đầu quét trực tiếp", "Stop Live Scan", "Dừng quét trực tiếp", "Restore Snapshot", "Khôi phục ảnh chụp");
            AddUiTranslations("Dutch", "Tuning", "Tuning", "Driving Tuning", "Rijtuning", "Tuning Actions", "Tuningacties", "Driving Tuning Actions", "Rijtuningacties", "Live Tuning Table", "Live tuningtabel", "Driving Tuning Table", "Rijtuningtabel", "Scan Tuning", "Tuning scannen", "Start Live Scan", "Live scan starten", "Stop Live Scan", "Live scan stoppen", "Restore Snapshot", "Snapshot herstellen");
        }

        private static void SeedCurrentUiTranslations()
        {
            AddUiTranslations("Japanese",
                "Welcome to Luna", "Lunaへようこそ", "Unleash the Horizon.", "ホライゾンを解き放て。",
                "Connect", "接続", "Features", "機能", "Autoshow", "オートショー", "Database Tools", "データベースツール",
                "Database Tuning", "データベース調整", "Driving Editor", "走行エディター", "Driving Tuning", "走行チューニング",
                "Photo Mode", "フォトモード", "Feedback", "フィードバック", "Settings", "設定", "Donate", "寄付", "Credits", "クレジット",
                "Disconnected", "切断", "Connecting", "接続中", "Connected", "接続済み",
                "Configure runtime tools, rewards, and gameplay options.", "ランタイムツール、報酬、ゲームプレイ設定を調整します。",
                "Manage garage entries and vehicle unlocks.", "ガレージの車両項目とアンロックを管理します。",
                "Repair saves and edit database records.", "セーブを修復し、データベース記録を編集します。",
                "Edit vehicle parts and database values.", "車両パーツとデータベース値を編集します。",
                "Control live grip, motion, and handling.", "走行中のグリップ、挙動、ハンドリングを制御します。",
                "Edit live tires, gearing, and suspension.", "タイヤ、ギア比、サスペンションをリアルタイム編集します。",
                "Adjust camera and environment settings.", "カメラと環境設定を調整します。",
                "Save locations and replay custom routes.", "位置を保存し、カスタムルートを再生します。",
                "Review logs and diagnostic output.", "ログと診断出力を確認します。",
                "Open Luna configs, logs, and exports.", "Lunaの設定、ログ、エクスポートを開きます。",
                "Read setup instructions and usage guides.", "セットアップ手順と使用ガイドを読みます。",
                "Report issues and suggest improvements.", "問題を報告し、改善案を送ります。",
                "Configure Luna preferences and appearance.", "Lunaの設定と外観を変更します。",
                "Support Luna's continued development.", "Lunaの継続開発を支援します。",
                "View contributors and acknowledgements.", "貢献者と謝辞を表示します。",
                "Attach Luna to the active game process.", "Lunaを実行中のゲームプロセスに接続します.");

            AddUiTranslations("Chinese",
                "Welcome to Luna", "欢迎使用 Luna", "Unleash the Horizon.", "释放地平线。",
                "Connect", "连接", "Features", "功能", "Autoshow", "车展", "Database Tools", "数据库工具",
                "Database Tuning", "数据库调校", "Driving Editor", "驾驶编辑器", "Driving Tuning", "驾驶调校",
                "Photo Mode", "拍照模式", "Feedback", "反馈", "Settings", "设置", "Donate", "捐赠", "Credits", "鸣谢",
                "Disconnected", "已断开", "Connecting", "连接中", "Connected", "已连接",
                "Configure runtime tools, rewards, and gameplay options.", "配置运行时工具、奖励和游戏选项。",
                "Manage garage entries and vehicle unlocks.", "管理车库条目和车辆解锁。",
                "Repair saves and edit database records.", "修复存档并编辑数据库记录。",
                "Edit vehicle parts and database values.", "编辑车辆部件和数据库数值。",
                "Control live grip, motion, and handling.", "实时控制抓地、运动和操控。",
                "Edit live tires, gearing, and suspension.", "实时编辑轮胎、齿比和悬挂。",
                "Adjust camera and environment settings.", "调整相机和环境设置。",
                "Save locations and replay custom routes.", "保存位置并重放自定义路线。",
                "Review logs and diagnostic output.", "查看日志和诊断输出。",
                "Open Luna configs, logs, and exports.", "打开 Luna 配置、日志和导出文件。",
                "Read setup instructions and usage guides.", "阅读设置说明和使用指南。",
                "Report issues and suggest improvements.", "报告问题并提出改进建议。",
                "Configure Luna preferences and appearance.", "配置 Luna 偏好和外观。",
                "Support Luna's continued development.", "支持 Luna 持续开发。",
                "View contributors and acknowledgements.", "查看贡献者和致谢。",
                "Attach Luna to the active game process.", "将 Luna 连接到当前游戏进程。");

            AddUiTranslations("Spanish",
                "Welcome to Luna", "Bienvenido a Luna", "Unleash the Horizon.", "Desata el Horizon.",
                "Photo Mode", "Modo Foto", "Feedback", "Comentarios", "Disconnected", "Desconectado", "Connecting", "Conectando", "Connected", "Conectado",
                "Configure runtime tools, rewards, and gameplay options.", "Configura herramientas en vivo, recompensas y opciones de juego.",
                "Manage garage entries and vehicle unlocks.", "Gestiona entradas del garaje y desbloqueos de vehículos.",
                "Repair saves and edit database records.", "Repara partidas guardadas y edita registros de la base de datos.",
                "Edit vehicle parts and database values.", "Edita piezas del vehículo y valores de la base de datos.",
                "Control live grip, motion, and handling.", "Controla agarre, movimiento y manejo en tiempo real.",
                "Edit live tires, gearing, and suspension.", "Edita neumáticos, marchas y suspensión en vivo.",
                "Adjust camera and environment settings.", "Ajusta la cámara y el entorno.",
                "Save locations and replay custom routes.", "Guarda ubicaciones y reproduce rutas personalizadas.",
                "Review logs and diagnostic output.", "Revisa registros y salida de diagnóstico.",
                "Open Luna configs, logs, and exports.", "Abre configuraciones, registros y exportaciones de Luna.",
                "Read setup instructions and usage guides.", "Lee instrucciones de configuración y guías de uso.",
                "Report issues and suggest improvements.", "Reporta problemas y sugiere mejoras.",
                "Configure Luna preferences and appearance.", "Configura preferencias y apariencia de Luna.",
                "Support Luna's continued development.", "Apoya el desarrollo continuo de Luna.",
                "View contributors and acknowledgements.", "Ve contribuyentes y agradecimientos.",
                "Attach Luna to the active game process.", "Conecta Luna al proceso activo del juego.");

            AddUiTranslations("Arabic",
                "Welcome to Luna", "مرحبًا بك في Luna", "Unleash the Horizon.", "أطلق العنان للأفق.",
                "Connect", "اتصال", "Features", "الميزات", "Autoshow", "معرض السيارات", "Database Tools", "أدوات قاعدة البيانات",
                "Database Tuning", "ضبط قاعدة البيانات", "Driving Editor", "محرر القيادة", "Driving Tuning", "ضبط القيادة",
                "Photo Mode", "وضع التصوير", "Feedback", "الملاحظات", "Settings", "الإعدادات", "Donate", "تبرع", "Credits", "الشكر",
                "Disconnected", "غير متصل", "Connecting", "جارٍ الاتصال", "Connected", "متصل",
                "Configure runtime tools, rewards, and gameplay options.", "اضبط أدوات التشغيل المباشر والمكافآت وخيارات اللعب.",
                "Manage garage entries and vehicle unlocks.", "أدر عناصر المرآب وفتح المركبات.",
                "Repair saves and edit database records.", "أصلح ملفات الحفظ وعدل سجلات قاعدة البيانات.",
                "Edit vehicle parts and database values.", "عدل أجزاء المركبة وقيم قاعدة البيانات.",
                "Control live grip, motion, and handling.", "تحكم بالتماسك والحركة والتحكم أثناء اللعب.",
                "Edit live tires, gearing, and suspension.", "عدل الإطارات ونسب القير والتعليق مباشرة.",
                "Adjust camera and environment settings.", "اضبط إعدادات الكاميرا والبيئة.",
                "Save locations and replay custom routes.", "احفظ المواقع وأعد تشغيل المسارات المخصصة.",
                "Review logs and diagnostic output.", "راجع السجلات ومخرجات التشخيص.",
                "Open Luna configs, logs, and exports.", "افتح إعدادات Luna وسجلاتها وتصديراتها.",
                "Read setup instructions and usage guides.", "اقرأ تعليمات الإعداد وأدلة الاستخدام.",
                "Report issues and suggest improvements.", "أبلغ عن المشاكل واقترح التحسينات.",
                "Configure Luna preferences and appearance.", "اضبط تفضيلات Luna ومظهرها.",
                "Support Luna's continued development.", "ادعم استمرار تطوير Luna.",
                "View contributors and acknowledgements.", "اعرض المساهمين والشكر.",
                "Attach Luna to the active game process.", "صل Luna بعملية اللعبة النشطة.");

            AddUiTranslations("Turkish",
                "Welcome to Luna", "Luna'ya Hoş Geldin", "Unleash the Horizon.", "Horizon'u serbest bırak.",
                "Photo Mode", "Fotoğraf Modu", "Feedback", "Geri Bildirim", "Disconnected", "Bağlantı yok", "Connecting", "Bağlanıyor", "Connected", "Bağlandı",
                "Configure runtime tools, rewards, and gameplay options.", "Canlı araçları, ödülleri ve oynanış seçeneklerini ayarla.",
                "Manage garage entries and vehicle unlocks.", "Garaj kayıtlarını ve araç kilitlerini yönet.",
                "Repair saves and edit database records.", "Kayıt dosyalarını onar ve veritabanı kayıtlarını düzenle.",
                "Edit vehicle parts and database values.", "Araç parçalarını ve veritabanı değerlerini düzenle.",
                "Control live grip, motion, and handling.", "Canlı yol tutuşu, hareketi ve sürüşü kontrol et.",
                "Edit live tires, gearing, and suspension.", "Canlı lastik, vites oranı ve süspansiyonu düzenle.",
                "Adjust camera and environment settings.", "Kamera ve ortam ayarlarını değiştir.",
                "Save locations and replay custom routes.", "Konumları kaydet ve özel rotaları tekrar oynat.",
                "Review logs and diagnostic output.", "Günlükleri ve tanılama çıktısını incele.",
                "Open Luna configs, logs, and exports.", "Luna ayarlarını, günlüklerini ve dışa aktarımlarını aç.",
                "Read setup instructions and usage guides.", "Kurulum yönergelerini ve kullanım kılavuzlarını oku.",
                "Report issues and suggest improvements.", "Sorun bildir ve iyileştirme öner.",
                "Configure Luna preferences and appearance.", "Luna tercihlerini ve görünümünü ayarla.",
                "Support Luna's continued development.", "Luna'nın devam eden gelişimini destekle.",
                "View contributors and acknowledgements.", "Katkıda bulunanları ve teşekkürleri görüntüle.",
                "Attach Luna to the active game process.", "Luna'yı etkin oyun işlemine bağla.");

            AddUiTranslations("Polish",
                "Welcome to Luna", "Witaj w Luna", "Unleash the Horizon.", "Uwolnij Horizon.",
                "Photo Mode", "Tryb zdjęć", "Feedback", "Opinie", "Disconnected", "Rozłączono", "Connecting", "Łączenie", "Connected", "Połączono",
                "Configure runtime tools, rewards, and gameplay options.", "Konfiguruj narzędzia runtime, nagrody i opcje rozgrywki.",
                "Manage garage entries and vehicle unlocks.", "Zarządzaj garażem i odblokowaniami pojazdów.",
                "Repair saves and edit database records.", "Naprawiaj zapisy i edytuj rekordy bazy danych.",
                "Edit vehicle parts and database values.", "Edytuj części pojazdów i wartości bazy danych.",
                "Control live grip, motion, and handling.", "Steruj przyczepnością, ruchem i prowadzeniem na żywo.",
                "Edit live tires, gearing, and suspension.", "Edytuj opony, przełożenia i zawieszenie na żywo.",
                "Adjust camera and environment settings.", "Dostosuj kamerę i ustawienia otoczenia.",
                "Save locations and replay custom routes.", "Zapisuj lokalizacje i odtwarzaj własne trasy.",
                "Review logs and diagnostic output.", "Przeglądaj logi i dane diagnostyczne.",
                "Open Luna configs, logs, and exports.", "Otwórz konfiguracje, logi i eksporty Luna.",
                "Read setup instructions and usage guides.", "Czytaj instrukcje konfiguracji i poradniki.",
                "Report issues and suggest improvements.", "Zgłaszaj problemy i sugeruj ulepszenia.",
                "Configure Luna preferences and appearance.", "Konfiguruj preferencje i wygląd Luna.",
                "Support Luna's continued development.", "Wesprzyj dalszy rozwój Luna.",
                "View contributors and acknowledgements.", "Zobacz autorów i podziękowania.",
                "Attach Luna to the active game process.", "Połącz Luna z aktywnym procesem gry.");

            AddUiTranslations("German",
                "Welcome to Luna", "Willkommen bei Luna", "Unleash the Horizon.", "Entfessle den Horizon.",
                "Photo Mode", "Fotomodus", "Feedback", "Feedback", "Disconnected", "Getrennt", "Connecting", "Verbindet", "Connected", "Verbunden",
                "Configure runtime tools, rewards, and gameplay options.", "Konfiguriere Laufzeittools, Belohnungen und Gameplay-Optionen.",
                "Manage garage entries and vehicle unlocks.", "Verwalte Garageneinträge und Fahrzeugfreischaltungen.",
                "Repair saves and edit database records.", "Repariere Speicherstände und bearbeite Datenbankeinträge.",
                "Edit vehicle parts and database values.", "Bearbeite Fahrzeugteile und Datenbankwerte.",
                "Control live grip, motion, and handling.", "Steuere Grip, Bewegung und Handling live.",
                "Edit live tires, gearing, and suspension.", "Bearbeite Reifen, Übersetzung und Fahrwerk live.",
                "Adjust camera and environment settings.", "Passe Kamera- und Umgebungseinstellungen an.",
                "Save locations and replay custom routes.", "Speichere Orte und spiele eigene Routen ab.",
                "Review logs and diagnostic output.", "Prüfe Logs und Diagnoseausgaben.",
                "Open Luna configs, logs, and exports.", "Öffne Luna-Konfigurationen, Logs und Exporte.",
                "Read setup instructions and usage guides.", "Lies Einrichtungsanweisungen und Nutzungshilfen.",
                "Report issues and suggest improvements.", "Melde Probleme und schlage Verbesserungen vor.",
                "Configure Luna preferences and appearance.", "Konfiguriere Luna-Einstellungen und Darstellung.",
                "Support Luna's continued development.", "Unterstütze die weitere Entwicklung von Luna.",
                "View contributors and acknowledgements.", "Zeige Mitwirkende und Danksagungen.",
                "Attach Luna to the active game process.", "Verbinde Luna mit dem aktiven Spielprozess.");

            AddUiTranslations("Swedish",
                "Welcome to Luna", "Välkommen till Luna", "Unleash the Horizon.", "Släpp loss Horizon.",
                "Photo Mode", "Fotoläge", "Feedback", "Feedback", "Disconnected", "Frånkopplad", "Connecting", "Ansluter", "Connected", "Ansluten",
                "Configure runtime tools, rewards, and gameplay options.", "Konfigurera runtime-verktyg, belöningar och spelalternativ.",
                "Manage garage entries and vehicle unlocks.", "Hantera garageposter och fordonsupplåsningar.",
                "Repair saves and edit database records.", "Reparera sparfiler och redigera databasposter.",
                "Edit vehicle parts and database values.", "Redigera fordonsdelar och databasvärden.",
                "Control live grip, motion, and handling.", "Styr grepp, rörelse och väghållning live.",
                "Edit live tires, gearing, and suspension.", "Redigera däck, utväxling och fjädring live.",
                "Adjust camera and environment settings.", "Justera kamera- och miljöinställningar.",
                "Save locations and replay custom routes.", "Spara platser och spela upp egna rutter.",
                "Review logs and diagnostic output.", "Granska loggar och diagnostik.",
                "Open Luna configs, logs, and exports.", "Öppna Lunas konfigurationer, loggar och exporter.",
                "Read setup instructions and usage guides.", "Läs installationsinstruktioner och guider.",
                "Report issues and suggest improvements.", "Rapportera problem och föreslå förbättringar.",
                "Configure Luna preferences and appearance.", "Konfigurera Lunas inställningar och utseende.",
                "Support Luna's continued development.", "Stöd Lunas fortsatta utveckling.",
                "View contributors and acknowledgements.", "Visa bidragsgivare och tack.",
                "Attach Luna to the active game process.", "Anslut Luna till den aktiva spelprocessen.");

            AddUiTranslations("Farsi",
                "Welcome to Luna", "به Luna خوش آمدید", "Unleash the Horizon.", "افق را آزاد کن.",
                "Connect", "اتصال", "Features", "امکانات", "Autoshow", "اتوشو", "Database Tools", "ابزارهای پایگاه داده",
                "Database Tuning", "تنظیم پایگاه داده", "Driving Editor", "ویرایشگر رانندگی", "Driving Tuning", "تنظیم رانندگی",
                "Photo Mode", "حالت عکس", "Feedback", "بازخورد", "Settings", "تنظیمات", "Donate", "حمایت", "Credits", "قدردانی",
                "Disconnected", "قطع شده", "Connecting", "در حال اتصال", "Connected", "متصل",
                "Configure runtime tools, rewards, and gameplay options.", "ابزارهای زنده، پاداش‌ها و گزینه‌های بازی را تنظیم کنید.",
                "Manage garage entries and vehicle unlocks.", "ورودی‌های گاراژ و باز شدن خودروها را مدیریت کنید.",
                "Repair saves and edit database records.", "ذخیره‌ها را تعمیر و رکوردهای پایگاه داده را ویرایش کنید.",
                "Edit vehicle parts and database values.", "قطعات خودرو و مقادیر پایگاه داده را ویرایش کنید.",
                "Control live grip, motion, and handling.", "چسبندگی، حرکت و هندلینگ زنده را کنترل کنید.",
                "Edit live tires, gearing, and suspension.", "لاستیک، دنده و تعلیق را زنده ویرایش کنید.",
                "Adjust camera and environment settings.", "تنظیمات دوربین و محیط را تغییر دهید.",
                "Save locations and replay custom routes.", "مکان‌ها را ذخیره و مسیرهای سفارشی را پخش کنید.",
                "Review logs and diagnostic output.", "گزارش‌ها و خروجی تشخیص را بررسی کنید.",
                "Open Luna configs, logs, and exports.", "تنظیمات، گزارش‌ها و خروجی‌های Luna را باز کنید.",
                "Read setup instructions and usage guides.", "راهنمای نصب و استفاده را بخوانید.",
                "Report issues and suggest improvements.", "مشکلات را گزارش و پیشنهاد بهبود دهید.",
                "Configure Luna preferences and appearance.", "ترجیحات و ظاهر Luna را تنظیم کنید.",
                "Support Luna's continued development.", "از توسعه مداوم Luna حمایت کنید.",
                "View contributors and acknowledgements.", "مشارکت‌کنندگان و قدردانی‌ها را ببینید.",
                "Attach Luna to the active game process.", "Luna را به فرایند فعال بازی وصل کنید.");

            AddUiTranslations("French",
                "Welcome to Luna", "Bienvenue dans Luna", "Unleash the Horizon.", "Libère l'Horizon.",
                "Photo Mode", "Mode Photo", "Feedback", "Retour", "Disconnected", "Déconnecté", "Connecting", "Connexion", "Connected", "Connecté",
                "Configure runtime tools, rewards, and gameplay options.", "Configure les outils runtime, les récompenses et les options de jeu.",
                "Manage garage entries and vehicle unlocks.", "Gère les entrées du garage et les déblocages de véhicules.",
                "Repair saves and edit database records.", "Répare les sauvegardes et modifie les enregistrements de base de données.",
                "Edit vehicle parts and database values.", "Modifie les pièces du véhicule et les valeurs de base de données.",
                "Control live grip, motion, and handling.", "Contrôle l'adhérence, le mouvement et la conduite en direct.",
                "Edit live tires, gearing, and suspension.", "Modifie les pneus, rapports et suspensions en direct.",
                "Adjust camera and environment settings.", "Ajuste les paramètres de caméra et d'environnement.",
                "Save locations and replay custom routes.", "Enregistre des positions et rejoue des itinéraires personnalisés.",
                "Review logs and diagnostic output.", "Consulte les journaux et les diagnostics.",
                "Open Luna configs, logs, and exports.", "Ouvre les configs, journaux et exports de Luna.",
                "Read setup instructions and usage guides.", "Lis les instructions de configuration et les guides.",
                "Report issues and suggest improvements.", "Signale des problèmes et propose des améliorations.",
                "Configure Luna preferences and appearance.", "Configure les préférences et l'apparence de Luna.",
                "Support Luna's continued development.", "Soutiens le développement continu de Luna.",
                "View contributors and acknowledgements.", "Affiche les contributeurs et remerciements.",
                "Attach Luna to the active game process.", "Connecte Luna au processus de jeu actif.");

            AddUiTranslations("Lithuanian",
                "Welcome to Luna", "Sveiki atvykę į Luna", "Unleash the Horizon.", "Išlaisvink Horizon.",
                "Photo Mode", "Foto režimas", "Feedback", "Atsiliepimai", "Disconnected", "Atsijungta", "Connecting", "Jungiamasi", "Connected", "Prisijungta",
                "Configure runtime tools, rewards, and gameplay options.", "Konfigūruok veikiančius įrankius, apdovanojimus ir žaidimo parinktis.",
                "Manage garage entries and vehicle unlocks.", "Tvarkyk garažo įrašus ir automobilių atrakinimus.",
                "Repair saves and edit database records.", "Taisyk išsaugojimus ir redaguok duomenų bazės įrašus.",
                "Edit vehicle parts and database values.", "Redaguok automobilio dalis ir duomenų bazės reikšmes.",
                "Control live grip, motion, and handling.", "Valdyk sukibimą, judėjimą ir valdymą gyvai.",
                "Edit live tires, gearing, and suspension.", "Redaguok padangas, pavaras ir pakabą gyvai.",
                "Adjust camera and environment settings.", "Keisk kameros ir aplinkos nustatymus.",
                "Save locations and replay custom routes.", "Išsaugok vietas ir atkurk pasirinktinius maršrutus.",
                "Review logs and diagnostic output.", "Peržiūrėk žurnalus ir diagnostikos išvestį.",
                "Open Luna configs, logs, and exports.", "Atidaryk Luna konfigūracijas, žurnalus ir eksportus.",
                "Read setup instructions and usage guides.", "Skaityk nustatymo instrukcijas ir naudojimo vadovus.",
                "Report issues and suggest improvements.", "Pranešk apie problemas ir siūlyk patobulinimus.",
                "Configure Luna preferences and appearance.", "Konfigūruok Luna nuostatas ir išvaizdą.",
                "Support Luna's continued development.", "Paremk tolesnį Luna kūrimą.",
                "View contributors and acknowledgements.", "Peržiūrėk prisidėjusius ir padėkas.",
                "Attach Luna to the active game process.", "Prijunk Luna prie aktyvaus žaidimo proceso.");

            AddUiTranslations("Portuguese",
                "Welcome to Luna", "Bem-vindo ao Luna", "Unleash the Horizon.", "Liberte o Horizon.",
                "Photo Mode", "Modo Foto", "Feedback", "Feedback", "Disconnected", "Desconectado", "Connecting", "Conectando", "Connected", "Conectado",
                "Configure runtime tools, rewards, and gameplay options.", "Configure ferramentas em tempo real, recompensas e opções de jogo.",
                "Manage garage entries and vehicle unlocks.", "Gerencie entradas da garagem e desbloqueios de veículos.",
                "Repair saves and edit database records.", "Repare saves e edite registros do banco de dados.",
                "Edit vehicle parts and database values.", "Edite peças do veículo e valores do banco de dados.",
                "Control live grip, motion, and handling.", "Controle aderência, movimento e dirigibilidade ao vivo.",
                "Edit live tires, gearing, and suspension.", "Edite pneus, marchas e suspensão ao vivo.",
                "Adjust camera and environment settings.", "Ajuste câmera e configurações de ambiente.",
                "Save locations and replay custom routes.", "Salve locais e reproduza rotas personalizadas.",
                "Review logs and diagnostic output.", "Revise logs e saída de diagnóstico.",
                "Open Luna configs, logs, and exports.", "Abra configurações, logs e exportações do Luna.",
                "Read setup instructions and usage guides.", "Leia instruções de configuração e guias de uso.",
                "Report issues and suggest improvements.", "Reporte problemas e sugira melhorias.",
                "Configure Luna preferences and appearance.", "Configure preferências e aparência do Luna.",
                "Support Luna's continued development.", "Apoie o desenvolvimento contínuo do Luna.",
                "View contributors and acknowledgements.", "Veja contribuidores e agradecimentos.",
                "Attach Luna to the active game process.", "Conecte Luna ao processo ativo do jogo.");

            AddUiTranslations("Indonesian",
                "Welcome to Luna", "Selamat datang di Luna", "Unleash the Horizon.", "Bebaskan Horizon.",
                "Photo Mode", "Mode Foto", "Feedback", "Masukan", "Disconnected", "Terputus", "Connecting", "Menghubungkan", "Connected", "Terhubung",
                "Configure runtime tools, rewards, and gameplay options.", "Atur alat runtime, hadiah, dan opsi gameplay.",
                "Manage garage entries and vehicle unlocks.", "Kelola entri garasi dan pembuka kendaraan.",
                "Repair saves and edit database records.", "Perbaiki save dan edit catatan database.",
                "Edit vehicle parts and database values.", "Edit komponen kendaraan dan nilai database.",
                "Control live grip, motion, and handling.", "Kontrol grip, gerak, dan handling secara langsung.",
                "Edit live tires, gearing, and suspension.", "Edit ban, rasio gigi, dan suspensi secara langsung.",
                "Adjust camera and environment settings.", "Atur kamera dan lingkungan.",
                "Save locations and replay custom routes.", "Simpan lokasi dan putar ulang rute khusus.",
                "Review logs and diagnostic output.", "Lihat log dan output diagnostik.",
                "Open Luna configs, logs, and exports.", "Buka konfigurasi, log, dan ekspor Luna.",
                "Read setup instructions and usage guides.", "Baca instruksi setup dan panduan penggunaan.",
                "Report issues and suggest improvements.", "Laporkan masalah dan sarankan peningkatan.",
                "Configure Luna preferences and appearance.", "Atur preferensi dan tampilan Luna.",
                "Support Luna's continued development.", "Dukung pengembangan Luna berkelanjutan.",
                "View contributors and acknowledgements.", "Lihat kontributor dan ucapan terima kasih.",
                "Attach Luna to the active game process.", "Hubungkan Luna ke proses game aktif.");

            AddUiTranslations("Georgian",
                "Welcome to Luna", "კეთილი იყოს შენი მობრძანება Luna-ში", "Unleash the Horizon.", "გაათავისუფლე Horizon.",
                "Connect", "დაკავშირება", "Features", "ფუნქციები", "Autoshow", "Autoshow", "Database Tools", "ბაზის ხელსაწყოები",
                "Database Tuning", "ბაზის ტიუნინგი", "Driving Editor", "მართვის რედაქტორი", "Driving Tuning", "მართვის ტიუნინგი",
                "Photo Mode", "ფოტო რეჟიმი", "Feedback", "უკუკავშირი", "Settings", "პარამეტრები", "Donate", "დონაცია", "Credits", "მადლობები",
                "Disconnected", "გათიშულია", "Connecting", "ერთდება", "Connected", "დაკავშირებულია",
                "Configure runtime tools, rewards, and gameplay options.", "დააყენე ცოცხალი ხელსაწყოები, ჯილდოები და თამაშის პარამეტრები.",
                "Manage garage entries and vehicle unlocks.", "მართე გარაჟის ჩანაწერები და მანქანების გახსნა.",
                "Repair saves and edit database records.", "შეაკეთე შენახვები და შეცვალე ბაზის ჩანაწერები.",
                "Edit vehicle parts and database values.", "შეცვალე მანქანის ნაწილები და ბაზის მნიშვნელობები.",
                "Control live grip, motion, and handling.", "აკონტროლე მოჭიდება, მოძრაობა და მართვა ცოცხლად.",
                "Edit live tires, gearing, and suspension.", "ცოცხლად შეცვალე საბურავები, გადაცემები და დაკიდება.",
                "Adjust camera and environment settings.", "შეცვალე კამერისა და გარემოს პარამეტრები.",
                "Save locations and replay custom routes.", "შეინახე მდებარეობები და გაიმეორე მორგებული მარშრუტები.",
                "Review logs and diagnostic output.", "ნახე ლოგები და დიაგნოსტიკური ინფორმაცია.",
                "Open Luna configs, logs, and exports.", "გახსენი Luna-ს კონფიგები, ლოგები და ექსპორტები.",
                "Read setup instructions and usage guides.", "წაიკითხე დაყენებისა და გამოყენების გზამკვლევები.",
                "Report issues and suggest improvements.", "შეატყობინე პრობლემა და შესთავაზე გაუმჯობესება.",
                "Configure Luna preferences and appearance.", "დააყენე Luna-ს პარამეტრები და გარეგნობა.",
                "Support Luna's continued development.", "დაუჭირე მხარი Luna-ს განვითარებას.",
                "View contributors and acknowledgements.", "ნახე მონაწილეები და მადლობები.",
                "Attach Luna to the active game process.", "დააკავშირე Luna აქტიურ თამაშის პროცესთან.");

            AddUiTranslations("Vietnamese",
                "Welcome to Luna", "Chào mừng đến với Luna", "Unleash the Horizon.", "Giải phóng Horizon.",
                "Photo Mode", "Chế độ ảnh", "Feedback", "Phản hồi", "Disconnected", "Đã ngắt", "Connecting", "Đang kết nối", "Connected", "Đã kết nối",
                "Configure runtime tools, rewards, and gameplay options.", "Cấu hình công cụ runtime, phần thưởng và tùy chọn gameplay.",
                "Manage garage entries and vehicle unlocks.", "Quản lý mục garage và mở khóa xe.",
                "Repair saves and edit database records.", "Sửa save và chỉnh bản ghi cơ sở dữ liệu.",
                "Edit vehicle parts and database values.", "Chỉnh phụ tùng xe và giá trị cơ sở dữ liệu.",
                "Control live grip, motion, and handling.", "Điều khiển độ bám, chuyển động và xử lý trực tiếp.",
                "Edit live tires, gearing, and suspension.", "Chỉnh lốp, tỷ số truyền và treo trực tiếp.",
                "Adjust camera and environment settings.", "Điều chỉnh camera và môi trường.",
                "Save locations and replay custom routes.", "Lưu vị trí và phát lại tuyến tùy chỉnh.",
                "Review logs and diagnostic output.", "Xem log và kết quả chẩn đoán.",
                "Open Luna configs, logs, and exports.", "Mở cấu hình, log và bản xuất của Luna.",
                "Read setup instructions and usage guides.", "Đọc hướng dẫn thiết lập và sử dụng.",
                "Report issues and suggest improvements.", "Báo lỗi và đề xuất cải tiến.",
                "Configure Luna preferences and appearance.", "Cấu hình tùy chọn và giao diện Luna.",
                "Support Luna's continued development.", "Ủng hộ quá trình phát triển Luna.",
                "View contributors and acknowledgements.", "Xem người đóng góp và lời cảm ơn.",
                "Attach Luna to the active game process.", "Kết nối Luna với tiến trình game đang chạy.");

            AddUiTranslations("Dutch",
                "Welcome to Luna", "Welkom bij Luna", "Unleash the Horizon.", "Ontketen de Horizon.",
                "Photo Mode", "Fotomodus", "Feedback", "Feedback", "Disconnected", "Verbroken", "Connecting", "Verbinden", "Connected", "Verbonden",
                "Configure runtime tools, rewards, and gameplay options.", "Configureer runtime-tools, beloningen en gameplay-opties.",
                "Manage garage entries and vehicle unlocks.", "Beheer garage-items en voertuigontgrendelingen.",
                "Repair saves and edit database records.", "Repareer saves en bewerk databasegegevens.",
                "Edit vehicle parts and database values.", "Bewerk voertuigonderdelen en databasewaarden.",
                "Control live grip, motion, and handling.", "Beheer live grip, beweging en wegligging.",
                "Edit live tires, gearing, and suspension.", "Bewerk live banden, versnellingen en ophanging.",
                "Adjust camera and environment settings.", "Pas camera- en omgevingsinstellingen aan.",
                "Save locations and replay custom routes.", "Sla locaties op en speel aangepaste routes opnieuw af.",
                "Review logs and diagnostic output.", "Bekijk logs en diagnostische uitvoer.",
                "Open Luna configs, logs, and exports.", "Open Luna-configs, logs en exports.",
                "Read setup instructions and usage guides.", "Lees installatie-instructies en gebruiksgidsen.",
                "Report issues and suggest improvements.", "Meld problemen en stel verbeteringen voor.",
                "Configure Luna preferences and appearance.", "Configureer Luna-voorkeuren en uiterlijk.",
                "Support Luna's continued development.", "Steun de verdere ontwikkeling van Luna.",
                "View contributors and acknowledgements.", "Bekijk bijdragers en dankbetuigingen.",
                "Attach Luna to the active game process.", "Verbind Luna met het actieve spelproces.");
        }

        private static void SeedKoreanUiTranslations()
        {
            AddUiTranslations("Korean",
                "Connect", "연결", "Features", "기능", "Autoshow", "오토쇼", "Database Tools", "데이터베이스 도구",
                "Database Tuning", "데이터베이스 튜닝", "Driving Editor", "주행 편집기", "Driving Tuning", "주행 튜닝",
                "Photo Mode", "사진 모드", "Teleport", "텔레포트", "Console", "콘솔", "Tutorial", "튜토리얼",
                "Feedback", "피드백", "Feedback/Bug", "피드백", "Settings", "설정", "Donate", "후원",
                "Credits", "크레딧", "Open Folder", "폴더 열기", "Open Results", "결과 열기", "Home", "홈",
                "Theme", "테마", "Support!", "후원!", "Back", "뒤로", "Apply", "적용", "Cancel", "취소",
                "Confirm", "확인", "Close", "닫기", "OK", "확인", "Save", "저장", "Browse", "찾아보기",
                "Pick", "선택", "Use Value", "값 사용", "Use Settings", "설정 사용", "Load", "불러오기",
                "Load Current", "현재 값 불러오기", "Load Garage", "차고 불러오기", "Add Selected", "선택 항목 추가",
                "Add All", "모두 추가", "Remove Selected", "선택 항목 제거", "Save List", "목록 저장",
                "Load List", "목록 불러오기", "Premade", "기본 제공", "Save Preset", "프리셋 저장",
                "Load Preset", "프리셋 불러오기", "Save Config", "설정 저장", "Reset Defaults", "기본값 복원",
                "Apply Language", "언어 적용", "Apply Colors", "색상 적용", "Reset Colors", "색상 초기화",
                "Display language", "표시 언어", "Language", "언어", "Color Theme", "색상 테마",
                "Connection Status", "연결 상태", "Ready", "준비됨", "Attached", "연결됨",
                "Not attached", "연결 안 됨", "Disconnected", "연결 끊김", "Connecting", "연결 중",
                "Connected", "연결됨", "Attach", "연결", "Auto Attach", "자동 연결", "Manual Attach", "수동 연결",
                "Runtime Hooks", "런타임 훅", "Quick Tutorials", "빠른 튜토리얼", "Feature How To Use", "기능 사용법",
                "Project Credits", "프로젝트 크레딧", "Start Here", "여기서 시작", "Database Actions", "데이터베이스 작업",
                "Quick Actions", "빠른 작업", "Internal Actions", "내부 작업", "Editors", "편집기",
                "Tables & Unlocks", "테이블 및 해금", "Tuning", "튜닝", "Tuning Actions", "튜닝 작업",
                "Driving Tuning Actions", "주행 튜닝 작업", "Live Tuning Table", "실시간 튜닝 표",
                "Driving Tuning Table", "주행 튜닝 표", "Scan Tuning", "튜닝 검색",
                "Start Live Scan", "실시간 스캔 시작", "Stop Live Scan", "실시간 스캔 중지",
                "Apply Selected", "선택 항목 적용", "Restore Snapshot", "스냅샷 복원",
                "Selected Car Editor", "선택 차량 편집기", "Pick Part", "부품 선택", "New Value", "새 값",
                "Section", "섹션", "Source", "원본", "Decal Unlocker", "데칼 해금",
                "Setting", "설정", "What it does", "기능 설명", "Strength", "강도", "Toggle", "토글",
                "Car ID", "차량 ID", "Year", "연식", "Make", "제조사", "Model", "모델", "Class", "클래스",
                "Copies", "복사본", "State", "상태", "Status", "상태", "Value", "값", "Current", "현재",
                "Default", "기본값", "Description", "설명", "Name", "이름", "File", "파일", "Select", "선택",
                "Owned", "보유", "Price", "가격", "Quantity", "수량",
                "No Water Drag", "물 저항 제거", "Super Handling", "슈퍼 핸들링", "Landing Stabilizer", "착지 안정화",
                "Slide Calmer", "슬라이드 완화", "Road Magnet", "도로 자석", "Speed Tamer", "속도 완화",
                "Air Lift", "공중 리프트", "Bounce Cushion", "바운스 완충", "Momentum Control", "모멘텀 제어",
                "Left Right Calm", "좌우 안정화", "Forward Back Calm", "전후 안정화", "Side Push", "측면 밀기",
                "Forward Push", "전방 밀기", "Vertical Trim", "수직 보정", "Side Lock", "측면 고정",
                "Forward Lock", "전방 고정", "Vertical Hold", "수직 유지", "Motion Freeze", "모션 정지",
                "Wheelie Boost", "윌리 부스트", "Drift Kick", "드리프트 킥", "Hover Glide", "호버 글라이드",
                "Air Brake", "공중 브레이크", "Corner Stabilizer", "코너 안정화", "Planted Boost", "접지 부스트",
                "Forward Launch", "전방 런치", "Straight Launch", "직선 런치", "Tire Bite", "타이어 접지",
                "Grip Lock", "그립 고정", "Forward Grip", "전방 그립", "Rail Grip", "레일 그립",
                "High Speed Stabilizer", "고속 안정화", "Ground Clamp", "지면 고정", "Stability Rail", "안정화 레일",
                "Corner Bite", "코너 접지", "Spin Recovery", "스핀 복구", "Boost", "부스트",
                "Drift Mode", "드리프트 모드", "Both Wheelspins", "양쪽 휠스핀", "Skill Points", "스킬 포인트",
                "Super Strength", "슈퍼 스트렝스", "Phase Dash", "페이즈 대시", "Config", "설정",
                "Super Strength Configuration", "슈퍼 스트렝스 설정",
                "Car Class Panel", "차량 클래스 패널", "Stats Editor", "통계 편집기", "Garage Favorites", "차고 즐겨찾기",
                "Traffic Editor", "트래픽 편집기", "Wheelspin Odds", "휠스핀 확률", "Photo Capture", "사진 캡처",
                "Routes", "경로", "Races", "레이스", "Free Prices", "무료 가격", "DLC Gates", "DLC 게이트",
                "Install Flags", "설치 플래그", "New Tags", "새 태그", "Unlock Presets", "해금 프리셋",
                "Unobtainable Gate", "획득 불가 게이트", "Vehicle Tuner", "차량 튜너",
                "Restore All Car Classes", "모든 차량 클래스 복원", "Class Restrictions: OFF", "클래스 제한: 꺼짐",
                "Class Restrictions: ON", "클래스 제한: 켜짐", "Disable Damage: OFF", "피해 비활성화: 꺼짐",
                "Disable Damage: ON", "피해 비활성화: 켜짐", "Key OFF", "키 꺼짐", "Key ON", "키 켜짐",
                "Hotkey OFF", "핫키 꺼짐", "Hotkey ON", "핫키 켜짐", "Turn Off", "끄기", "Turn All Off", "모두 끄기",
                "Donate on Ko-fi", "Ko-fi로 후원", "Donate with Crypto", "암호화폐로 후원",
                "Support Luna's continued development.", "Luna의 지속적인 개발을 지원합니다.",
                "Thank you for being here and supporting Luna.", "Luna를 응원해 주셔서 감사합니다.",
                "The people and tools that helped Luna become possible.", "Luna가 만들어질 수 있도록 도와준 사람들과 도구입니다.",
                "Configure runtime tools, rewards, and gameplay options.", "런타임 도구, 보상, 게임플레이 옵션을 설정합니다.",
                "Manage garage entries and vehicle unlocks.", "차고 항목과 차량 해금을 관리합니다.",
                "Repair saves and edit database records.", "저장 데이터를 복구하고 데이터베이스 기록을 편집합니다.",
                "Edit vehicle parts and database values.", "차량 부품과 데이터베이스 값을 편집합니다.",
                "Control live grip, motion, and handling.", "실시간 그립, 움직임, 핸들링을 제어합니다.",
                "Edit live tires, gearing, and suspension.", "실시간 타이어, 기어비, 서스펜션을 편집합니다.",
                "Adjust camera and environment settings.", "카메라와 환경 설정을 조정합니다.",
                "Save locations and replay custom routes.", "위치를 저장하고 사용자 경로를 재생합니다.",
                "Review logs and diagnostic output.", "로그와 진단 출력을 확인합니다.",
                "Open Luna configs, logs, and exports.", "Luna 설정, 로그, 내보내기 파일을 엽니다.",
                "Read setup instructions and usage guides.", "설정 지침과 사용 가이드를 읽습니다.",
                "Report issues and suggest improvements.", "문제를 보고하고 개선 사항을 제안합니다.",
                "Configure Luna preferences and appearance.", "Luna 환경 설정과 모양을 구성합니다.",
                "View contributors and acknowledgements.", "기여자와 감사 인사를 확인합니다.",
                "Attach Luna to the active game process.", "Luna를 실행 중인 게임 프로세스에 연결합니다.",
                "Type a number, turn the dot green, then press Apply. Turning the dot red stops that row right away.",
                "숫자를 입력하고 토글을 켠 뒤 적용을 누르세요. 토글을 끄면 해당 항목이 즉시 중지됩니다.",
                "Live tune values for the current car. Start scan, edit values, then apply selected rows.",
                "현재 차량 값을 실시간으로 조정합니다. 스캔을 시작하고 값을 편집한 뒤 선택 항목을 적용하세요.",
                "Save locations, load custom lists, and teleport back with one key.",
                "위치를 저장하고 사용자 목록을 불러오며 하나의 키로 돌아갑니다.",
                "Pick how Luna attaches and change the display language.",
                "Luna 연결 방식과 표시 언어를 설정합니다.");
        }

        private static void SeedKoreanPageDetailTranslations()
        {
            AddUiTranslations("Korean",
                "Proceed", "계속",
                "Before you continue", "계속하기 전에",
                "Please confirm", "확인 필요",
                "Update", "알림",
                "Review this recommendation before continuing.", "계속하기 전에 이 권장 사항을 확인하세요.",
                "This action needs your confirmation.", "이 작업은 확인이 필요합니다.",
                "Review the details below.", "아래 내용을 확인하세요.",
                "You do understand that depending on the game version, these database tools may or may not work.", "게임 버전에 따라 이 데이터베이스 도구가 작동하지 않을 수 있음을 이해했습니다.",
                "Choose how Luna attaches, where it reads FH6 files, and which language the UI uses.", "Luna 연결 방식, FH6 파일을 읽는 위치, UI 언어를 설정합니다.",
                "Connection", "연결",
                "Attach finds the FH6 game process automatically. Manual folder mode is only a fallback for game files.", "연결은 FH6 게임 프로세스를 자동으로 찾습니다. 수동 폴더 모드는 게임 파일을 위한 예비 방식입니다.",
                "Normal use: leave manual folder mode off and let Luna find FH6 automatically.", "일반 사용 시 수동 폴더 모드는 끄고 Luna가 FH6를 자동으로 찾게 하세요.",
                "Detected process", "감지된 프로세스",
                "Game folder fallback", "게임 폴더 예비 설정",
                "Use manual game folder", "수동 게임 폴더 사용",
                "Load current values after attach", "연결 후 현재 값 불러오기",
                "Manual only changes the folder Luna reads names from; it does not change which process is attached.", "수동 설정은 Luna가 이름을 읽는 폴더만 바꾸며 연결 대상 프로세스는 바꾸지 않습니다.",
                "Interface", "인터페이스",
                "Changes buttons, labels, and UI help text. Console logs stay in English.", "버튼, 레이블, UI 도움말을 변경합니다. 콘솔 로그는 영어로 유지됩니다.",
                "Only interface text is translated. Logs, numbers, paths, and raw game values stay unchanged.", "인터페이스 텍스트만 번역됩니다. 로그, 숫자, 경로, 원본 게임 값은 변경되지 않습니다.",
                "Luna Version:", "Luna 버전:");

            AddUiTranslations("Korean",
                "Buttons for garage fixes, unlocks, prices, and backups.", "차고 수정, 해금, 가격, 백업을 위한 버튼입니다.",
                "Choose one action, let it finish, then check the game.", "작업 하나를 선택하고 완료될 때까지 기다린 뒤 게임에서 확인하세요.",
                "Profile Cosmetics", "프로필 꾸미기",
                "AI Behavior Editor", "AI 동작 편집기",
                "Quick Actions - Save & Database", "빠른 작업 - 저장 및 데이터베이스",
                "Quick Actions - Garage & Cars", "빠른 작업 - 차고 및 차량",
                "Quick Actions - Live Switches", "빠른 작업 - 실시간 스위치",
                "Drive Traffic", "트래픽 주행",
                "Barn Finds", "헛간 발견",
                "Refresh Summary", "요약 새로고침",
                "Autoshow DB", "오토쇼 DB",
                "Dump", "덤프",
                "Backup Save", "저장 백업",
                "Reapply Unlock", "해금 다시 적용",
                "Add All Grants", "모든 지급 차량 추가",
                "Remove Dupe Cars", "중복 차량 제거",
                "Fix Thumbnails", "썸네일 수정",
                "Free Upgrades", "무료 업그레이드",
                "Auction All Cars", "모든 차량 경매 가능",
                "Remove Traffic: OFF", "트래픽 제거: 꺼짐",
                "Remove Traffic: ON", "트래픽 제거: 켜짐",
                "Freeze AI Traffic: OFF", "AI 트래픽 정지: 꺼짐",
                "Freeze AI Traffic: ON", "AI 트래픽 정지: 켜짐");

            AddUiTranslations("Korean",
                "Advanced selected-car database editor for tuning values, installed parts, performance, physics, and flags.", "선택한 차량의 튜닝 값, 설치 부품, 성능, 물리, 플래그를 편집하는 고급 데이터베이스 편집기입니다.",
                "Select an owned car, edit checked rows, then apply. Restore uses Luna backups for the selected car.", "보유 차량을 선택하고 체크한 행을 편집한 뒤 적용하세요. 복원은 선택 차량의 Luna 백업을 사용합니다.",
                "Selected car: none", "선택 차량: 없음",
                "Fields: load garage and select a car", "필드: 차고를 불러오고 차량을 선택하세요",
                "Restore Selected", "선택 항목 복원",
                "Pick Value", "값 선택",
                "Tip: Pick Part highlights safe parts in yellow and blocks crash-prone parts in red.", "팁: 부품 선택은 안전한 부품을 노란색으로 표시하고 충돌 위험 부품은 빨간색으로 차단합니다.",
                "Owned Garage Cars", "보유 차고 차량",
                "Search your garage, select one car, then edit its tuning data below.", "차고를 검색하고 차량 하나를 선택한 뒤 아래에서 튜닝 데이터를 편집하세요.",
                "Search, pick parts, apply selected values, and restore backups for the chosen car.", "선택 차량을 검색하고 부품을 고른 뒤 선택 값을 적용하거나 백업을 복원합니다.",
                "Sort Options", "정렬 옵션",
                "Remove Filter", "필터 제거",
                "Filter: All options", "필터: 모든 옵션",
                "Selected:", "선택:");

            AddUiTranslations("Korean",
                "Live tune values for the current car. Start scan, edit values, then apply selected rows.", "현재 차량 값을 실시간으로 조정합니다. 스캔을 시작하고 값을 편집한 뒤 선택 항목을 적용하세요.",
                "Keep live scan running while you edit. Luna updates the values below if the game changes.", "편집하는 동안 실시간 스캔을 유지합니다. 게임 값이 바뀌면 Luna가 아래 값을 업데이트합니다.",
                "Live scan stays active until you stop it. Edited values remain while current values refresh.", "중지할 때까지 실시간 스캔이 유지됩니다. 현재 값이 갱신되어도 편집한 값은 유지됩니다.",
                "Start Live Scan, then edit and apply the rows you want.", "실시간 스캔을 시작한 뒤 원하는 행을 편집하고 적용하세요.",
                "TIRES", "타이어",
                "GEARING", "기어",
                "ALIGNMENT", "얼라인먼트",
                "AERO", "공력",
                "SPRINGS", "스프링",
                "DAMPING", "댐핑",
                "STEERING", "스티어링",
                "BODY & WHEELS", "차체 & 휠",
                "Front Left Tire Pressure", "앞 왼쪽 타이어 압력",
                "Front Right Tire Pressure", "앞 오른쪽 타이어 압력",
                "Rear Left Tire Pressure", "뒤 왼쪽 타이어 압력",
                "Rear Right Tire Pressure", "뒤 오른쪽 타이어 압력",
                "Sets front-left tire pressure.", "앞 왼쪽 타이어 압력을 설정합니다.",
                "Sets front-right tire pressure.", "앞 오른쪽 타이어 압력을 설정합니다.",
                "Sets rear-left tire pressure.", "뒤 왼쪽 타이어 압력을 설정합니다.",
                "Sets rear-right tire pressure.", "뒤 오른쪽 타이어 압력을 설정합니다.",
                "Final Drive", "최종 감속비",
                "Reverse Gear", "후진 기어",
                "1st Gear", "1단 기어",
                "2nd Gear", "2단 기어",
                "3rd Gear", "3단 기어",
                "4th Gear", "4단 기어",
                "5th Gear", "5단 기어",
                "6th Gear", "6단 기어",
                "7th Gear", "7단 기어",
                "8th Gear", "8단 기어",
                "9th Gear", "9단 기어",
                "10th Gear", "10단 기어",
                "Changes the final drive ratio.", "최종 감속비를 변경합니다.",
                "Changes the reverse gear ratio.", "후진 기어비를 변경합니다.",
                "Changes first gear.", "1단 기어를 변경합니다.",
                "Changes second gear.", "2단 기어를 변경합니다.",
                "Changes third gear.", "3단 기어를 변경합니다.",
                "Changes fourth gear.", "4단 기어를 변경합니다.",
                "Changes fifth gear.", "5단 기어를 변경합니다.",
                "Changes sixth gear.", "6단 기어를 변경합니다.",
                "Changes seventh gear.", "7단 기어를 변경합니다.",
                "Changes eighth gear.", "8단 기어를 변경합니다.",
                "Changes ninth gear.", "9단 기어를 변경합니다.",
                "Changes tenth gear.", "10단 기어를 변경합니다.",
                "Found", "찾음",
                "Not Found", "찾을 수 없음",
                "Set Value", "값 설정");

            AddUiTranslations("Korean",
                "Tune grip, launch, bounce, and stability live while you drive. Use small changes first, then apply the rows you want.", "주행 중 그립, 런치, 바운스, 안정성을 실시간으로 조정합니다. 먼저 작은 값으로 시작한 뒤 원하는 항목을 적용하세요.",
                "Many features need proper values to work correctly.", "많은 기능은 올바른 값이 있어야 정상 작동합니다.",
                "ACTIONS", "작업",
                "GRIP & CORNERING", "그립 & 코너링",
                "AIR & LANDING", "공중 & 착지",
                "LAUNCH & FORCE", "런치 & 힘",
                "STABILITY & MOTION", "안정성 & 움직임",
                "Skips the water-drag callback so puddles and water crossings stop slowing the car. No number needed.", "물 저항 콜백을 건너뛰어 물웅덩이와 물길이 차량을 늦추지 않게 합니다. 숫자는 필요 없습니다.",
                "Adds constant downward force on the vertical lane for flatter, grippier turns. Higher = more planted.", "더 평평하고 그립 있는 코너링을 위해 수직 방향으로 지속적인 하향 힘을 더합니다. 높을수록 더 안정됩니다.",
                "Turns vertical landing stabilization on or off. Public build uses Luna's fixed tuned value.", "수직 착지 안정화를 켜거나 끕니다. 공개 빌드는 Luna의 고정 튜닝 값을 사용합니다.",
                "Adds light downward force while driving to reduce loose sliding without freezing movement.", "움직임을 멈추지 않고 느슨한 슬라이드를 줄이기 위해 주행 중 약한 하향 힘을 더합니다.",
                "Adds upward force on the vertical lane. Higher = more lift, negative values push down.", "수직 방향으로 상승 힘을 더합니다. 높을수록 더 뜨고 음수는 아래로 누릅니다.",
                "Multiplies vertical motion, then adds lift, creating a soft hover/glide effect.", "수직 움직임을 배율 조정한 뒤 리프트를 더해 부드러운 호버/글라이드 효과를 만듭니다.",
                "When armed, holding S in the air multiplies side, vertical, and forward motion. Lower = stronger air brake.", "활성화 후 공중에서 S를 누르면 좌우, 수직, 전후 움직임을 배율 조정합니다. 낮을수록 공중 브레이크가 강합니다.",
                "Sets vertical motion to a direct downward clamp. Higher = stronger clamp; negative lifts.", "수직 움직임을 직접적인 하향 고정으로 설정합니다. 높을수록 강하게 고정되고 음수는 들어 올립니다.",
                "Adds side force and forward force together for a controlled drift kick. Positive and negative pick direction.", "제어된 드리프트 킥을 위해 측면 힘과 전방 힘을 함께 더합니다. 양수와 음수로 방향을 선택합니다.",
                "On forward input, zeros side sway and applies a boost-style forward shove for 2 seconds.", "전방 입력 시 좌우 흔들림을 0으로 만들고 2초 동안 부스트식 전방 밀기를 적용합니다.",
                "Multiplies side, vertical, and forward motion to zero together. No number needed.", "좌우, 수직, 전방 움직임을 모두 0으로 만듭니다. 숫자는 필요 없습니다.",
                "Adds downward force at speed without cutting forward motion. Higher = steadier fast runs.", "전방 움직임을 줄이지 않고 속도에서 하향 힘을 더합니다. 높을수록 고속 주행이 안정됩니다.",
                "Adds downward recovery force so the car settles faster after spins or hard slides.", "스핀이나 강한 슬라이드 후 차량이 더 빨리 안정되도록 하향 회복 힘을 더합니다.");

            AddUiTranslations("Korean",
                "FH6 Photo Mode camera modifiers. Scan first, edit values, then apply selected rows.", "FH6 사진 모드 카메라 수정값입니다. 먼저 스캔하고 값을 편집한 뒤 선택 항목을 적용하세요.",
                "Photo Mode Actions", "사진 모드 작업",
                "Open Photo Mode in-game, scan, then turn on the camera controls you want.", "게임에서 사진 모드를 열고 스캔한 뒤 원하는 카메라 제어를 켜세요.",
                "Scan Photo Mode", "사진 모드 스캔",
                "Restore Defaults", "기본값 복원",
                "Height OFF", "높이 꺼짐",
                "Height ON", "높이 켜짐",
                "Zoom OFF", "줌 꺼짐",
                "Zoom ON", "줌 켜짐",
                "Reset View", "뷰 초기화",
                "Height uses Shift/Ctrl. Zoom uses the mouse wheel.", "높이는 Shift/Ctrl을 사용합니다. 줌은 마우스 휠을 사용합니다.",
                "Camera Effects", "카메라 효과",
                "Live colour grading for your shots - saturation, contrast, brightness, exposure, sepia, blur and vignette. Click Find Camera (from normal view, before grading), type values, then Apply. Reset restores defaults.", "촬영 화면의 실시간 색 보정입니다 - 채도, 대비, 밝기, 노출, 세피아, 블러, 비네트. 일반 화면에서 보정 전에 카메라 찾기를 누르고 값을 입력한 뒤 적용하세요. 초기화는 기본값으로 되돌립니다.",
                "Find Camera", "카메라 찾기",
                "Apply Effects", "효과 적용",
                "Saturation", "채도",
                "Contrast", "대비",
                "Brightness", "밝기",
                "Exposure", "노출",
                "Sepia", "세피아",
                "Blur", "블러",
                "Vignette", "비네트",
                "Photo Mode Modifiers", "사진 모드 수정값",
                "Values write directly to FH6's live Photo Mode modifier table.", "값은 FH6 실시간 사진 모드 수정 테이블에 직접 기록됩니다.");

            AddUiTranslations("Korean",
                "Teleport Workspace", "텔레포트 작업 공간",
                "Configure hotkeys, manage saved locations, and run location lists from one place.", "단축키를 설정하고 저장 위치를 관리하며 위치 목록을 한곳에서 실행합니다.",
                "Saved location hotkey", "저장 위치 단축키",
                "Location actions", "위치 작업",
                "Location lists", "위치 목록",
                "List player", "목록 플레이어",
                "SAVED LOCATION HOTKEY", "저장 위치 단축키",
                "LOCATION ACTIONS", "위치 작업",
                "LOCATION LISTS", "위치 목록",
                "LIST PLAYER", "목록 플레이어",
                "Teleport key", "텔레포트 키",
                "Save key", "저장 키",
                "Selected: none", "선택: 없음",
                "Hotkey OFF", "단축키 꺼짐",
                "Hotkey ON", "단축키 켜짐",
                "Save Current", "현재 위치 저장",
                "Save Waypoint", "웨이포인트 저장",
                "Overwrite", "덮어쓰기",
                "Teleport Now", "지금 텔레포트",
                "Remove", "제거",
                "Turn Off", "끄기",
                "Save, load, or open a premade route without changing the selected hotkeys.", "선택한 단축키를 바꾸지 않고 경로를 저장, 불러오기, 또는 기본 경로를 엽니다.",
                "Save List", "목록 저장",
                "Load List", "목록 불러오기",
                "Premade", "기본 제공",
                "Play key", "재생 키",
                "Every", "간격",
                "seconds", "초",
                "Start List", "목록 시작",
                "Stop List", "목록 중지",
                "Runs saved locations in order and loops until stopped.", "저장 위치를 순서대로 실행하고 중지할 때까지 반복합니다.",
                "Saved Locations", "저장된 위치",
                "Select a row to use it. Double-click its name or press F2 to rename it.", "사용할 행을 선택하세요. 이름을 두 번 클릭하거나 F2를 눌러 이름을 바꿉니다.",
                "Search", "검색",
                "Filter by saved name, X, Y, or Z.", "저장 이름, X, Y, Z로 필터링합니다.");

            AddUiTranslations("Korean",
                "Learn Luna's main workflows with direct, task-focused guides.", "직접적이고 작업 중심의 가이드로 Luna의 주요 작업 흐름을 익힙니다.",
                "Start with the safe workflow", "안전한 작업 흐름으로 시작",
                "The same three steps apply to most Luna tools.", "대부분의 Luna 도구에는 같은 세 단계가 적용됩니다.",
                "Launch FH6", "FH6 실행",
                "Reach the menu or world before attaching.", "연결하기 전에 메뉴 또는 월드에 진입하세요.",
                "Connect Luna", "Luna 연결",
                "Wait for the green Connected badge.", "초록색 연결됨 배지를 기다리세요.",
                "Apply carefully", "신중하게 적용",
                "Use small values and verify in game.", "작은 값을 사용하고 게임에서 확인하세요.",
                "Tool guides", "도구 가이드",
                "Open a guide card to go directly to that tool.", "가이드 카드를 열어 해당 도구로 바로 이동합니다.",
                "Start FH6 first. Luna normally attaches automatically; Settings contains the manual path options.", "먼저 FH6를 실행하세요. Luna는 보통 자동으로 연결되며, 수동 경로 옵션은 설정에 있습니다.",
                "Set a value, switch the row on, then press Apply. Switching it off sends the stop command immediately.", "값을 설정하고 행을 켠 뒤 적용을 누르세요. 끄면 중지 명령이 즉시 전송됩니다.",
                "Load cars, select the entries you want, then use Add Selected. Review removals before confirming.", "차량을 불러오고 원하는 항목을 선택한 뒤 선택 항목 추가를 사용하세요. 제거는 확인 전에 검토하세요.",
                "Choose one database action, let it finish, then reload the matching FH6 menu to refresh cached data.", "데이터베이스 작업 하나를 선택하고 완료될 때까지 기다린 뒤 해당 FH6 메뉴를 다시 열어 캐시된 데이터를 갱신하세요.",
                "Use small live handling values first. Enable only the rows you need, then apply the live hooks.", "먼저 작은 실시간 핸들링 값을 사용하세요. 필요한 행만 켠 뒤 실시간 훅을 적용하세요.",
                "Start the live scan, load current values, edit selected rows, then apply or restore the snapshot.", "실시간 스캔을 시작하고 현재 값을 불러온 뒤 선택 행을 편집하고 적용하거나 스냅샷을 복원하세요.",
                "Save a location, select it, enable the hotkey, or replay a saved list using the configured play key.", "위치를 저장하고 선택한 뒤 단축키를 켜거나 설정한 재생 키로 저장 목록을 재생하세요.",
                "Change language, appearance, attach behavior, and review the installed Luna version.", "언어, 모양, 연결 방식, 설치된 Luna 버전을 확인합니다.",
                "Review the live action history, copy useful diagnostics, or open the persistent log folder.", "실시간 작업 기록을 확인하고 유용한 진단 내용을 복사하거나 영구 로그 폴더를 엽니다.",
                "Report a problem or suggest an improvement through Luna's embedded feedback page.", "Luna 내장 피드백 페이지로 문제를 보고하거나 개선 사항을 제안합니다.",
                "Live feature workflow", "실시간 기능 작업 흐름",
                "A consistent pattern used throughout Features and Driving Editor.", "기능과 주행 편집기 전체에서 사용하는 일관된 패턴입니다.",
                "Choose a value", "값 선택",
                "Type a number or open the row's configuration.", "숫자를 입력하거나 행 설정을 엽니다.",
                "Switch it on", "켜기",
                "Green means active. Red means stopped.", "초록색은 활성, 빨간색은 중지를 의미합니다.",
                "Press Apply", "적용 누르기",
                "Wait for the status message before continuing.", "계속하기 전에 상태 메시지를 기다리세요.",
                "Verify in FH6", "FH6에서 확인",
                "Reload the relevant menu if the game cached it.", "게임이 캐시했다면 관련 메뉴를 다시 불러오세요.",
                "Useful refresh triggers", "유용한 새로고침 조건",
                "Some live values update only when FH6 reads that system again.", "일부 실시간 값은 FH6가 해당 시스템을 다시 읽을 때만 업데이트됩니다.",
                "Profile values", "프로필 값",
                "Enter and leave Autoshow for credits, wheelspins, and related profile values.", "크레딧, 휠스핀, 관련 프로필 값은 오토쇼에 들어갔다 나오세요.",
                "Driving values", "주행 값",
                "Switch cars or reload the world when handling or database values appear cached.", "핸들링 또는 데이터베이스 값이 캐시된 것처럼 보이면 차량을 바꾸거나 월드를 다시 불러오세요.",
                "Logs and support", "로그 및 지원",
                "Use Console and Open Folder when sharing a reproducible failure.", "재현 가능한 오류를 공유할 때 콘솔과 폴더 열기를 사용하세요.");
        }

        private static void SeedCurrentNavigationLabels()
        {
            AddUiTranslations("Japanese",
                "Connect", "接続", "Features", "機能", "Autoshow", "オートショー", "Database Tools", "データベースツール",
                "Database Tuning", "データベース調整", "Driving Editor", "走行エディター", "Driving Tuning", "走行チューニング",
                "Photo Mode", "フォトモード", "Teleport", "テレポート", "Console", "コンソール", "Open Folder", "フォルダーを開く",
                "Tutorial", "チュートリアル", "Feedback", "フィードバック", "Feedback/Bug", "フィードバック",
                "Settings", "設定", "Donate", "寄付", "Credits", "クレジット");

            AddUiTranslations("Chinese",
                "Connect", "连接", "Features", "功能", "Autoshow", "车展", "Database Tools", "数据库工具",
                "Database Tuning", "数据库调校", "Driving Editor", "驾驶编辑器", "Driving Tuning", "驾驶调校",
                "Photo Mode", "拍照模式", "Teleport", "传送", "Console", "控制台", "Open Folder", "打开文件夹",
                "Tutorial", "教程", "Feedback", "反馈", "Feedback/Bug", "反馈",
                "Settings", "设置", "Donate", "捐赠", "Credits", "鸣谢");

            AddUiTranslations("Spanish",
                "Connect", "Conectar", "Features", "Funciones", "Autoshow", "Autoshow", "Database Tools", "Herramientas de base de datos",
                "Database Tuning", "Ajuste de base de datos", "Driving Editor", "Editor de conducción", "Driving Tuning", "Ajuste de conducción",
                "Photo Mode", "Modo Foto", "Teleport", "Teletransporte", "Console", "Consola", "Open Folder", "Abrir carpeta",
                "Tutorial", "Tutorial", "Feedback", "Comentarios", "Feedback/Bug", "Comentarios",
                "Settings", "Ajustes", "Donate", "Donar", "Credits", "Créditos");

            AddUiTranslations("Arabic",
                "Connect", "اتصال", "Features", "الميزات", "Autoshow", "معرض السيارات", "Database Tools", "أدوات قاعدة البيانات",
                "Database Tuning", "ضبط قاعدة البيانات", "Driving Editor", "محرر القيادة", "Driving Tuning", "ضبط القيادة",
                "Photo Mode", "وضع التصوير", "Teleport", "انتقال فوري", "Console", "وحدة التحكم", "Open Folder", "فتح المجلد",
                "Tutorial", "الدليل", "Feedback", "الملاحظات", "Feedback/Bug", "الملاحظات",
                "Settings", "الإعدادات", "Donate", "تبرع", "Credits", "الشكر");

            AddUiTranslations("Turkish",
                "Connect", "Bağlan", "Features", "Özellikler", "Autoshow", "Autoshow", "Database Tools", "Veritabanı Araçları",
                "Database Tuning", "Veritabanı Ayarı", "Driving Editor", "Sürüş Düzenleyici", "Driving Tuning", "Sürüş Ayarı",
                "Photo Mode", "Fotoğraf Modu", "Teleport", "Işınlanma", "Console", "Konsol", "Open Folder", "Klasörü Aç",
                "Tutorial", "Rehber", "Feedback", "Geri Bildirim", "Feedback/Bug", "Geri Bildirim",
                "Settings", "Ayarlar", "Donate", "Bağış", "Credits", "Katkılar");

            AddUiTranslations("Polish",
                "Connect", "Połącz", "Features", "Funkcje", "Autoshow", "Autoshow", "Database Tools", "Narzędzia bazy danych",
                "Database Tuning", "Tuning bazy danych", "Driving Editor", "Edytor jazdy", "Driving Tuning", "Tuning jazdy",
                "Photo Mode", "Tryb zdjęć", "Teleport", "Teleport", "Console", "Konsola", "Open Folder", "Otwórz folder",
                "Tutorial", "Poradnik", "Feedback", "Opinie", "Feedback/Bug", "Opinie",
                "Settings", "Ustawienia", "Donate", "Wesprzyj", "Credits", "Podziękowania");

            AddUiTranslations("German",
                "Connect", "Verbinden", "Features", "Funktionen", "Autoshow", "Autoshow", "Database Tools", "Datenbanktools",
                "Database Tuning", "Datenbank-Tuning", "Driving Editor", "Fahreditor", "Driving Tuning", "Fahr-Tuning",
                "Photo Mode", "Fotomodus", "Teleport", "Teleport", "Console", "Konsole", "Open Folder", "Ordner öffnen",
                "Tutorial", "Anleitung", "Feedback", "Feedback", "Feedback/Bug", "Feedback",
                "Settings", "Einstellungen", "Donate", "Spenden", "Credits", "Danksagungen");

            AddUiTranslations("Swedish",
                "Connect", "Anslut", "Features", "Funktioner", "Autoshow", "Autoshow", "Database Tools", "Databasverktyg",
                "Database Tuning", "Databastuning", "Driving Editor", "Körredigerare", "Driving Tuning", "Körtuning",
                "Photo Mode", "Fotoläge", "Teleport", "Teleport", "Console", "Konsol", "Open Folder", "Öppna mapp",
                "Tutorial", "Guide", "Feedback", "Feedback", "Feedback/Bug", "Feedback",
                "Settings", "Inställningar", "Donate", "Donera", "Credits", "Tack");

            AddUiTranslations("Farsi",
                "Connect", "اتصال", "Features", "امکانات", "Autoshow", "اتوشو", "Database Tools", "ابزارهای پایگاه داده",
                "Database Tuning", "تنظیم پایگاه داده", "Driving Editor", "ویرایشگر رانندگی", "Driving Tuning", "تنظیم رانندگی",
                "Photo Mode", "حالت عکس", "Teleport", "تلپورت", "Console", "کنسول", "Open Folder", "باز کردن پوشه",
                "Tutorial", "آموزش", "Feedback", "بازخورد", "Feedback/Bug", "بازخورد",
                "Settings", "تنظیمات", "Donate", "حمایت", "Credits", "قدردانی");

            AddUiTranslations("French",
                "Connect", "Connecter", "Features", "Fonctions", "Autoshow", "Autoshow", "Database Tools", "Outils de base de données",
                "Database Tuning", "Réglage de base de données", "Driving Editor", "Éditeur de conduite", "Driving Tuning", "Réglage conduite",
                "Photo Mode", "Mode Photo", "Teleport", "Téléportation", "Console", "Console", "Open Folder", "Ouvrir le dossier",
                "Tutorial", "Guide", "Feedback", "Retour", "Feedback/Bug", "Retour",
                "Settings", "Paramètres", "Donate", "Don", "Credits", "Remerciements");

            AddUiTranslations("Lithuanian",
                "Connect", "Prisijungti", "Features", "Funkcijos", "Autoshow", "Autoshow", "Database Tools", "Duomenų bazės įrankiai",
                "Database Tuning", "Duomenų bazės derinimas", "Driving Editor", "Vairavimo redaktorius", "Driving Tuning", "Vairavimo derinimas",
                "Photo Mode", "Foto režimas", "Teleport", "Teleportas", "Console", "Konsolė", "Open Folder", "Atidaryti aplanką",
                "Tutorial", "Vadovas", "Feedback", "Atsiliepimai", "Feedback/Bug", "Atsiliepimai",
                "Settings", "Nustatymai", "Donate", "Paremti", "Credits", "Padėkos");

            AddUiTranslations("Portuguese",
                "Connect", "Conectar", "Features", "Recursos", "Autoshow", "Autoshow", "Database Tools", "Ferramentas de banco de dados",
                "Database Tuning", "Ajuste de banco de dados", "Driving Editor", "Editor de condução", "Driving Tuning", "Ajuste de condução",
                "Photo Mode", "Modo Foto", "Teleport", "Teleporte", "Console", "Console", "Open Folder", "Abrir pasta",
                "Tutorial", "Tutorial", "Feedback", "Feedback", "Feedback/Bug", "Feedback",
                "Settings", "Configurações", "Donate", "Doar", "Credits", "Créditos");

            AddUiTranslations("Indonesian",
                "Connect", "Hubungkan", "Features", "Fitur", "Autoshow", "Autoshow", "Database Tools", "Alat Database",
                "Database Tuning", "Tuning Database", "Driving Editor", "Editor Mengemudi", "Driving Tuning", "Tuning Mengemudi",
                "Photo Mode", "Mode Foto", "Teleport", "Teleport", "Console", "Konsol", "Open Folder", "Buka Folder",
                "Tutorial", "Tutorial", "Feedback", "Masukan", "Feedback/Bug", "Masukan",
                "Settings", "Pengaturan", "Donate", "Donasi", "Credits", "Ucapan Terima Kasih");

            AddUiTranslations("Georgian",
                "Connect", "დაკავშირება", "Features", "ფუნქციები", "Autoshow", "Autoshow", "Database Tools", "ბაზის ხელსაწყოები",
                "Database Tuning", "ბაზის ტიუნინგი", "Driving Editor", "მართვის რედაქტორი", "Driving Tuning", "მართვის ტიუნინგი",
                "Photo Mode", "ფოტო რეჟიმი", "Teleport", "ტელეპორტი", "Console", "კონსოლი", "Open Folder", "საქაღალდის გახსნა",
                "Tutorial", "გზამკვლევი", "Feedback", "უკუკავშირი", "Feedback/Bug", "უკუკავშირი",
                "Settings", "პარამეტრები", "Donate", "დონაცია", "Credits", "მადლობები");

            AddUiTranslations("Vietnamese",
                "Connect", "Kết nối", "Features", "Tính năng", "Autoshow", "Autoshow", "Database Tools", "Công cụ cơ sở dữ liệu",
                "Database Tuning", "Tinh chỉnh cơ sở dữ liệu", "Driving Editor", "Trình chỉnh lái xe", "Driving Tuning", "Tinh chỉnh lái xe",
                "Photo Mode", "Chế độ ảnh", "Teleport", "Dịch chuyển", "Console", "Bảng điều khiển", "Open Folder", "Mở thư mục",
                "Tutorial", "Hướng dẫn", "Feedback", "Phản hồi", "Feedback/Bug", "Phản hồi",
                "Settings", "Cài đặt", "Donate", "Ủng hộ", "Credits", "Ghi công");

            AddUiTranslations("Dutch",
                "Connect", "Verbinden", "Features", "Functies", "Autoshow", "Autoshow", "Database Tools", "Databasetools",
                "Database Tuning", "Databasetuning", "Driving Editor", "Rij-editor", "Driving Tuning", "Rijtuning",
                "Photo Mode", "Fotomodus", "Teleport", "Teleport", "Console", "Console", "Open Folder", "Map openen",
                "Tutorial", "Handleiding", "Feedback", "Feedback", "Feedback/Bug", "Feedback",
                "Settings", "Instellingen", "Donate", "Doneren", "Credits", "Dankwoord");

            AddUiTranslations("Korean",
                "Welcome to Luna", "Luna에 오신 것을 환영합니다", "Unleash the Horizon.", "Horizon을 해방하세요.",
                "Connect", "연결", "Features", "기능", "Autoshow", "오토쇼", "Database Tools", "데이터베이스 도구",
                "Database Tuning", "데이터베이스 튜닝", "Driving Editor", "주행 편집기", "Driving Tuning", "주행 튜닝",
                "Photo Mode", "사진 모드", "Teleport", "텔레포트", "Console", "콘솔", "Open Folder", "폴더 열기",
                "Tutorial", "튜토리얼", "Feedback", "피드백", "Feedback/Bug", "피드백",
                "Settings", "설정", "Donate", "후원", "Credits", "크레딧");
        }

        private static readonly string[] FeaturePageUiKeys = new[]
        {
            "Type a number, turn the dot green, then press Apply. Turning the dot red stops that row right away.",
            "ECONOMY & PROGRESSION", "SCORE MULTIPLIERS", "PHYSICS & HANDLING", "MOVEMENT & ABILITIES", "ENVIRONMENT & RULES", "CONFIGURATION",
            "Profile Boost Pack", "Credits", "Both Wheelspins", "Skill Points", "XP Gain",
            "Drift Score Multiplier", "Speed Zone Multiplier",
            "Puddle Correction", "Super Grip", "Top Speed", "Acceleration", "Acceleration Ramp", "Gravity", "Super Brake", "Adaptive Brake", "Cruise Control",
            "No Clip", "Super Strength", "Jump", "Fly", "Phase Dash", "Boost", "Teleport To Waypoint", "Auto Race (Drive)", "Auto Race (Teleport)", "Drift Mode",
            "Mission Timer", "Race Timer", "Time of Day", "FOV Slider", "No Build Limit", "Freeze AI", "No Skill Break",
            "Save Config", "Load Config", "Autosave", "Live values fill in automatically when Luna attaches.",
            "Click here", "Edit Pack", "Config", "Key OFF", "Key ON", "Safe", "Custom", "Speed", "Distance", "All", "Custom FOV", "Defaults", "Save", "Cancel", "Apply",
            "Green is on. Red turns it off right away.", "Green is armed. Red turns it off right away."
        };

        private static void SeedFeaturePageTranslations()
        {
            AddFeaturePageTranslationSet("Japanese",
                "数値を入力し、トグルをオンにしてから適用を押します。オフにするとその行はすぐ停止します。",
                "経済 & 進行", "スコア倍率", "物理 & ハンドリング", "移動 & アビリティ", "環境 & ルール", "設定",
                "プロフィールブーストパック", "クレジット", "両方のホイールスピン", "スキルポイント", "XP獲得",
                "ドリフトスコア倍率", "スピードゾーン倍率",
                "水たまり補正", "スーパーグリップ", "最高速度", "加速", "加速ランプ", "重力", "スーパーブレーキ", "アダプティブブレーキ", "クルーズコントロール",
                "ノークリップ", "スーパー強化", "ジャンプ", "飛行", "フェーズダッシュ", "ブースト", "ウェイポイントへテレポート", "オートレース (運転)", "オートレース (テレポート)", "ドリフトモード",
                "ミッションタイマー", "レースタイマー", "時刻", "FOVスライダー", "ビルド制限なし", "AI停止", "スキルチェーン維持",
                "設定を保存", "設定を読み込み", "自動保存", "Lunaが接続されるとライブ値は自動で入力されます。",
                "ここをクリック", "パックを編集", "設定", "キー OFF", "キー ON", "安全", "カスタム", "速度", "距離", "すべて", "カスタムFOV", "既定値", "保存", "キャンセル", "適用",
                "オンにすると有効です。オフにするとすぐ停止します。", "オンにすると待機状態です。オフにするとすぐ停止します。");

            AddFeaturePageTranslationSet("Chinese",
                "输入数字，打开开关，然后按应用。关闭开关会立即停止该行。",
                "经济 & 进度", "分数倍率", "物理 & 操控", "移动 & 能力", "环境 & 规则", "配置",
                "档案增强包", "点数", "两种抽奖", "技能点", "XP 增益",
                "漂移分数倍率", "测速区间倍率",
                "水坑修正", "超级抓地", "最高速度", "加速", "加速渐进", "重力", "超级刹车", "自适应刹车", "巡航控制",
                "穿墙", "超级力量", "跳跃", "飞行", "相位冲刺", "推进", "传送到路点", "自动比赛 (驾驶)", "自动比赛 (传送)", "漂移模式",
                "任务计时器", "比赛计时器", "时间", "FOV 滑块", "无建造限制", "冻结 AI", "技能链不断",
                "保存配置", "加载配置", "自动保存", "Luna 连接后会自动填入实时数值。",
                "点击这里", "编辑包", "配置", "按键关", "按键开", "安全", "自定义", "速度", "距离", "全部", "自定义 FOV", "默认", "保存", "取消", "应用",
                "开启为启用。关闭会立即停止。", "开启为待命。关闭会立即停止。");

            AddFeaturePageTranslationSet("Spanish",
                "Escribe un número, activa el interruptor y pulsa Aplicar. Desactivarlo detiene esa fila al instante.",
                "ECONOMÍA & PROGRESIÓN", "MULTIPLICADORES DE PUNTUACIÓN", "FÍSICA & MANEJO", "MOVIMIENTO & HABILIDADES", "ENTORNO & REGLAS", "CONFIGURACIÓN",
                "Paquete de mejora de perfil", "Créditos", "Ambos Wheelspins", "Puntos de habilidad", "Ganancia de XP",
                "Multiplicador de derrape", "Multiplicador de zona de velocidad",
                "Corrección de charcos", "Super agarre", "Velocidad máxima", "Aceleración", "Rampa de aceleración", "Gravedad", "Super freno", "Freno adaptativo", "Control de crucero",
                "Sin colisión", "Super fuerza", "Salto", "Volar", "Impulso de fase", "Impulso", "Teletransportar a punto de ruta", "Carrera automática (conducir)", "Carrera automática (teletransporte)", "Modo derrape",
                "Temporizador de misión", "Temporizador de carrera", "Hora del día", "Control FOV", "Sin límite de construcción", "Congelar IA", "Sin romper cadena",
                "Guardar config.", "Cargar config.", "Autoguardado", "Los valores en vivo se rellenan automáticamente cuando Luna se conecta.",
                "Haz clic aquí", "Editar paquete", "Config.", "Tecla OFF", "Tecla ON", "Seguro", "Personalizado", "Velocidad", "Distancia", "Todo", "FOV personalizado", "Predeterminados", "Guardar", "Cancelar", "Aplicar",
                "Verde activa. Rojo lo detiene al instante.", "Verde lo arma. Rojo lo detiene al instante.");

            AddFeaturePageTranslationSet("Arabic",
                "اكتب رقمًا، فعّل المفتاح، ثم اضغط تطبيق. إيقافه يوقف هذا السطر فورًا.",
                "الاقتصاد & التقدم", "مضاعفات النقاط", "الفيزياء & التحكم", "الحركة & القدرات", "البيئة & القواعد", "الإعدادات",
                "حزمة تعزيز الملف", "الرصيد", "كلتا اللفات", "نقاط المهارة", "زيادة XP",
                "مضاعف نقاط الدرفت", "مضاعف منطقة السرعة",
                "تصحيح البرك", "تماسك خارق", "السرعة القصوى", "التسارع", "منحنى التسارع", "الجاذبية", "فرامل خارقة", "فرامل تكيفية", "مثبت السرعة",
                "بدون تصادم", "قوة خارقة", "قفز", "طيران", "اندفاع الطور", "دفع", "انتقال إلى نقطة الطريق", "سباق تلقائي (قيادة)", "سباق تلقائي (انتقال)", "وضع الدرفت",
                "مؤقت المهمة", "مؤقت السباق", "وقت اليوم", "منزلق FOV", "بدون حد بناء", "تجميد الذكاء الاصطناعي", "منع كسر المهارة",
                "حفظ الإعدادات", "تحميل الإعدادات", "حفظ تلقائي", "تُملأ القيم المباشرة تلقائيًا عند اتصال Luna.",
                "اضغط هنا", "تعديل الحزمة", "إعداد", "المفتاح OFF", "المفتاح ON", "آمن", "مخصص", "السرعة", "المسافة", "الكل", "FOV مخصص", "الافتراضي", "حفظ", "إلغاء", "تطبيق",
                "الأخضر تشغيل. الأحمر يوقفه فورًا.", "الأخضر جاهز. الأحمر يوقفه فورًا.");

            AddFeaturePageTranslationSet("Turkish",
                "Bir sayı yaz, anahtarı aç, sonra Uygula'ya bas. Kapatmak bu satırı hemen durdurur.",
                "EKONOMİ & İLERLEME", "SKOR ÇARPANLARI", "FİZİK & YOL TUTUŞ", "HAREKET & YETENEKLER", "ORTAM & KURALLAR", "YAPILANDIRMA",
                "Profil güçlendirme paketi", "Krediler", "İki Wheelspin", "Yetenek puanları", "XP kazancı",
                "Drift skoru çarpanı", "Hız bölgesi çarpanı",
                "Su birikintisi düzeltmesi", "Süper tutuş", "Azami hız", "Hızlanma", "Hızlanma rampası", "Yerçekimi", "Süper fren", "Uyarlanabilir fren", "Hız sabitleyici",
                "Çarpışmasız geçiş", "Süper güç", "Zıpla", "Uç", "Faz atılışı", "Güçlendirme", "Yol noktasına ışınlan", "Otomatik yarış (sür)", "Otomatik yarış (ışınlan)", "Drift modu",
                "Görev zamanlayıcı", "Yarış zamanlayıcı", "Günün saati", "FOV kaydırıcı", "İnşa sınırı yok", "Yapay zekayı dondur", "Yetenek zinciri kopmasın",
                "Ayarı kaydet", "Ayarı yükle", "Otomatik kayıt", "Luna bağlandığında canlı değerler otomatik dolar.",
                "Buraya tıkla", "Paketi düzenle", "Ayar", "Tuş OFF", "Tuş ON", "Güvenli", "Özel", "Hız", "Mesafe", "Tümü", "Özel FOV", "Varsayılanlar", "Kaydet", "İptal", "Uygula",
                "Yeşil açık. Kırmızı hemen kapatır.", "Yeşil hazırlar. Kırmızı hemen kapatır.");

            AddFeaturePageTranslationSet("Polish",
                "Wpisz liczbę, włącz przełącznik i naciśnij Zastosuj. Wyłączenie od razu zatrzymuje ten wiersz.",
                "EKONOMIA & POSTĘP", "MNOŻNIKI WYNIKU", "FIZYKA & PROWADZENIE", "RUCH & UMIEJĘTNOŚCI", "ŚRODOWISKO & ZASADY", "KONFIGURACJA",
                "Pakiet wzmocnienia profilu", "Kredyty", "Oba Wheelspiny", "Punkty umiejętności", "Przyrost XP",
                "Mnożnik driftu", "Mnożnik strefy prędkości",
                "Korekta kałuż", "Super przyczepność", "Prędkość maksymalna", "Przyspieszenie", "Rampa przyspieszenia", "Grawitacja", "Super hamulec", "Hamulec adaptacyjny", "Tempomat",
                "Bez kolizji", "Super siła", "Skok", "Lot", "Przeskok fazowy", "Dopalacz", "Teleport do punktu trasy", "Auto wyścig (jazda)", "Auto wyścig (teleport)", "Tryb driftu",
                "Timer misji", "Timer wyścigu", "Pora dnia", "Suwak FOV", "Bez limitu budowy", "Zamroź SI", "Bez przerwania umiejętności",
                "Zapisz konfigurację", "Wczytaj konfigurację", "Autozapis", "Wartości live wypełnią się automatycznie po połączeniu Luny.",
                "Kliknij tutaj", "Edytuj pakiet", "Konfiguracja", "Klawisz OFF", "Klawisz ON", "Bezpieczny", "Własny", "Prędkość", "Dystans", "Wszystko", "Własne FOV", "Domyślne", "Zapisz", "Anuluj", "Zastosuj",
                "Zielony włącza. Czerwony natychmiast zatrzymuje.", "Zielony uzbraja. Czerwony natychmiast zatrzymuje.");

            AddFeaturePageTranslationSet("German",
                "Gib eine Zahl ein, aktiviere den Schalter und drücke Anwenden. Ausschalten stoppt diese Zeile sofort.",
                "WIRTSCHAFT & FORTSCHRITT", "PUNKTEMULTIPLIKATOREN", "PHYSIK & HANDLING", "BEWEGUNG & FÄHIGKEITEN", "UMGEBUNG & REGELN", "KONFIGURATION",
                "Profil-Boost-Paket", "Credits", "Beide Wheelspins", "Skillpunkte", "XP-Gewinn",
                "Driftpunkte-Multiplikator", "Speed-Zone-Multiplikator",
                "Pfützenkorrektur", "Super-Grip", "Höchstgeschwindigkeit", "Beschleunigung", "Beschleunigungsrampe", "Schwerkraft", "Superbremse", "Adaptive Bremse", "Tempomat",
                "No Clip", "Superkraft", "Sprung", "Fliegen", "Phasen-Dash", "Boost", "Zum Wegpunkt teleportieren", "Autorennen (fahren)", "Autorennen (teleportieren)", "Driftmodus",
                "Missionstimer", "Renn-Timer", "Tageszeit", "FOV-Regler", "Kein Baulimit", "KI einfrieren", "Kein Skill-Abbruch",
                "Konfig speichern", "Konfig laden", "Autospeichern", "Live-Werte werden automatisch gefüllt, wenn Luna verbunden ist.",
                "Hier klicken", "Paket bearbeiten", "Konfig", "Taste AUS", "Taste AN", "Sicher", "Benutzerdefiniert", "Geschwindigkeit", "Distanz", "Alle", "Eigenes FOV", "Standardwerte", "Speichern", "Abbrechen", "Anwenden",
                "Grün ist an. Rot stoppt es sofort.", "Grün ist bereit. Rot stoppt es sofort.");

            AddFeaturePageTranslationSet("Swedish",
                "Skriv ett nummer, slå på reglaget och tryck Verkställ. Att slå av stoppar raden direkt.",
                "EKONOMI & FRAMSTEG", "POÄNGMULTIPLIKATORER", "FYSIK & HANTERING", "RÖRELSE & FÖRMÅGOR", "MILJÖ & REGLER", "KONFIGURATION",
                "Profilboostpaket", "Krediter", "Båda wheelspins", "Färdighetspoäng", "XP-vinst",
                "Driftpoängsmultiplikator", "Hastighetszonsmultiplikator",
                "Pölkorrigering", "Supergrepp", "Topphastighet", "Acceleration", "Accelerationsramp", "Gravitation", "Superbroms", "Adaptiv broms", "Farthållare",
                "No Clip", "Superstyrka", "Hoppa", "Flyg", "Fasdash", "Boost", "Teleportera till waypoint", "Autorace (kör)", "Autorace (teleport)", "Driftläge",
                "Uppdragstimer", "Racetimer", "Tid på dagen", "FOV-reglage", "Ingen bygggräns", "Frys AI", "Ingen skill break",
                "Spara konfig", "Ladda konfig", "Autospara", "Livevärden fylls i automatiskt när Luna ansluter.",
                "Klicka här", "Redigera paket", "Konfig", "Tangent AV", "Tangent PÅ", "Säker", "Anpassad", "Hastighet", "Distans", "Alla", "Anpassad FOV", "Standard", "Spara", "Avbryt", "Verkställ",
                "Grönt är på. Rött stoppar direkt.", "Grönt är redo. Rött stoppar direkt.");

            AddFeaturePageTranslationSet("Farsi",
                "یک عدد وارد کنید، کلید را روشن کنید و سپس اعمال را بزنید. خاموش کردن، همان ردیف را فوراً متوقف می‌کند.",
                "اقتصاد & پیشرفت", "ضریب‌های امتیاز", "فیزیک & هندلینگ", "حرکت & توانایی‌ها", "محیط & قوانین", "پیکربندی",
                "بسته تقویت پروفایل", "اعتبار", "هر دو Wheelspin", "امتیازهای مهارت", "افزایش XP",
                "ضریب امتیاز دریفت", "ضریب منطقه سرعت",
                "اصلاح آب‌گرفتگی", "چسبندگی فوق‌العاده", "حداکثر سرعت", "شتاب", "رمپ شتاب", "گرانش", "ترمز فوق‌العاده", "ترمز تطبیقی", "کروز کنترل",
                "عبور بدون برخورد", "قدرت فوق‌العاده", "پرش", "پرواز", "داش فازی", "بوست", "تلپورت به نقطه مسیر", "مسابقه خودکار (رانندگی)", "مسابقه خودکار (تلپورت)", "حالت دریفت",
                "تایمر ماموریت", "تایمر مسابقه", "زمان روز", "اسلایدر FOV", "بدون محدودیت ساخت", "فریز کردن AI", "بدون قطع زنجیره مهارت",
                "ذخیره تنظیمات", "بارگذاری تنظیمات", "ذخیره خودکار", "وقتی Luna متصل شود مقدارهای زنده خودکار پر می‌شوند.",
                "اینجا کلیک کنید", "ویرایش بسته", "تنظیمات", "کلید خاموش", "کلید روشن", "ایمن", "سفارشی", "سرعت", "فاصله", "همه", "FOV سفارشی", "پیش‌فرض‌ها", "ذخیره", "لغو", "اعمال",
                "سبز یعنی روشن. قرمز فوراً متوقف می‌کند.", "سبز یعنی آماده. قرمز فوراً متوقف می‌کند.");

            AddFeaturePageTranslationSet("French",
                "Saisis un nombre, active l'interrupteur, puis appuie sur Appliquer. Le désactiver arrête cette ligne immédiatement.",
                "ÉCONOMIE & PROGRESSION", "MULTIPLICATEURS DE SCORE", "PHYSIQUE & CONDUITE", "MOUVEMENT & CAPACITÉS", "ENVIRONNEMENT & RÈGLES", "CONFIGURATION",
                "Pack boost de profil", "Crédits", "Deux Wheelspins", "Points de compétence", "Gain d'XP",
                "Multiplicateur de drift", "Multiplicateur de zone de vitesse",
                "Correction des flaques", "Super adhérence", "Vitesse max", "Accélération", "Rampe d'accélération", "Gravité", "Super frein", "Frein adaptatif", "Régulateur de vitesse",
                "No Clip", "Super force", "Saut", "Vol", "Dash phase", "Boost", "Téléporter au point de route", "Course auto (conduire)", "Course auto (téléporter)", "Mode drift",
                "Minuteur de mission", "Minuteur de course", "Heure du jour", "Curseur FOV", "Sans limite de construction", "Geler l'IA", "Aucune rupture de compétence",
                "Enregistrer config.", "Charger config.", "Enregistrement auto", "Les valeurs en direct se remplissent automatiquement quand Luna est connecté.",
                "Cliquer ici", "Modifier le pack", "Config.", "Touche OFF", "Touche ON", "Sûr", "Personnalisé", "Vitesse", "Distance", "Tout", "FOV personnalisé", "Par défaut", "Enregistrer", "Annuler", "Appliquer",
                "Vert active. Rouge arrête immédiatement.", "Vert arme. Rouge arrête immédiatement.");

            AddFeaturePageTranslationSet("Lithuanian",
                "Įvesk skaičių, įjunk jungiklį ir spausk Taikyti. Išjungimas tą eilutę sustabdo iškart.",
                "EKONOMIKA & PROGRESAS", "TAŠKŲ DAUGIKLIAI", "FIZIKA & VALDYMAS", "JUDĖJIMAS & GEBĖJIMAI", "APLINKA & TAISYKLĖS", "KONFIGŪRACIJA",
                "Profilio stiprinimo paketas", "Kreditai", "Abu Wheelspin", "Įgūdžių taškai", "XP prieaugis",
                "Drifto taškų daugiklis", "Greičio zonos daugiklis",
                "Balų korekcija", "Super sukibimas", "Maks. greitis", "Akceleracija", "Akceleracijos rampa", "Gravitacija", "Super stabdis", "Adaptyvus stabdis", "Kruizo kontrolė",
                "Be susidūrimų", "Super jėga", "Šuolis", "Skrydis", "Fazinis šuolis", "Pagreitis", "Teleportuoti į kelio tašką", "Auto lenktynės (vairuoti)", "Auto lenktynės (teleportuoti)", "Drifto režimas",
                "Misijos laikmatis", "Lenktynių laikmatis", "Paros laikas", "FOV slankiklis", "Be konstravimo ribos", "Sustabdyti DI", "Be įgūdžio nutrūkimo",
                "Išsaugoti konfig.", "Įkelti konfig.", "Automatinis saugojimas", "Tiesioginės reikšmės automatiškai užsipildo, kai Luna prisijungia.",
                "Spustelėk čia", "Redaguoti paketą", "Konfig.", "Klavišas OFF", "Klavišas ON", "Saugus", "Pasirinktinis", "Greitis", "Atstumas", "Visi", "Pasirinktinis FOV", "Numatytieji", "Išsaugoti", "Atšaukti", "Taikyti",
                "Žalia įjungia. Raudona iškart sustabdo.", "Žalia paruošia. Raudona iškart sustabdo.");

            AddFeaturePageTranslationSet("Portuguese",
                "Digite um número, ligue o botão e pressione Aplicar. Desligar para essa linha imediatamente.",
                "ECONOMIA & PROGRESSÃO", "MULTIPLICADORES DE PONTUAÇÃO", "FÍSICA & CONTROLE", "MOVIMENTO & HABILIDADES", "AMBIENTE & REGRAS", "CONFIGURAÇÃO",
                "Pacote de reforço de perfil", "Créditos", "Ambos Wheelspins", "Pontos de habilidade", "Ganho de XP",
                "Multiplicador de drift", "Multiplicador de zona de velocidade",
                "Correção de poças", "Super aderência", "Velocidade máxima", "Aceleração", "Rampa de aceleração", "Gravidade", "Super freio", "Freio adaptativo", "Controle de cruzeiro",
                "Sem colisão", "Super força", "Pulo", "Voar", "Dash de fase", "Boost", "Teleportar para waypoint", "Corrida automática (dirigir)", "Corrida automática (teleporte)", "Modo drift",
                "Temporizador de missão", "Temporizador de corrida", "Hora do dia", "Controle FOV", "Sem limite de construção", "Congelar IA", "Sem quebrar habilidade",
                "Salvar config.", "Carregar config.", "Salvamento automático", "Os valores ao vivo são preenchidos automaticamente quando a Luna conecta.",
                "Clique aqui", "Editar pacote", "Config.", "Tecla OFF", "Tecla ON", "Seguro", "Personalizado", "Velocidade", "Distância", "Tudo", "FOV personalizado", "Padrões", "Salvar", "Cancelar", "Aplicar",
                "Verde liga. Vermelho para imediatamente.", "Verde arma. Vermelho para imediatamente.");

            AddFeaturePageTranslationSet("Indonesian",
                "Ketik angka, nyalakan toggle, lalu tekan Terapkan. Mematikannya langsung menghentikan baris itu.",
                "EKONOMI & PROGRESI", "PENGALI SKOR", "FISIKA & KENDALI", "GERAKAN & KEMAMPUAN", "LINGKUNGAN & ATURAN", "KONFIGURASI",
                "Paket boost profil", "Kredit", "Kedua Wheelspin", "Poin skill", "Gains XP",
                "Pengali skor drift", "Pengali zona kecepatan",
                "Koreksi genangan", "Grip super", "Kecepatan maksimum", "Akselerasi", "Ramp akselerasi", "Gravitasi", "Rem super", "Rem adaptif", "Cruise control",
                "No Clip", "Kekuatan super", "Lompat", "Terbang", "Dash fase", "Boost", "Teleport ke waypoint", "Balapan otomatis (mengemudi)", "Balapan otomatis (teleport)", "Mode drift",
                "Timer misi", "Timer balapan", "Waktu hari", "Slider FOV", "Tanpa batas build", "Bekukan AI", "Skill tidak putus",
                "Simpan konfigurasi", "Muat konfigurasi", "Simpan otomatis", "Nilai live akan terisi otomatis saat Luna terhubung.",
                "Klik di sini", "Edit paket", "Konfigurasi", "Tombol OFF", "Tombol ON", "Aman", "Kustom", "Kecepatan", "Jarak", "Semua", "FOV kustom", "Default", "Simpan", "Batal", "Terapkan",
                "Hijau aktif. Merah langsung menghentikan.", "Hijau siap. Merah langsung menghentikan.");

            AddFeaturePageTranslationSet("Georgian",
                "შეიყვანე რიცხვი, ჩართე გადამრთველი და დააჭირე გამოყენებას. გამორთვა ამ რიგს მაშინვე აჩერებს.",
                "ეკონომიკა & პროგრესი", "ქულის მულტიპლიკატორები", "ფიზიკა & მართვა", "მოძრაობა & უნარები", "გარემო & წესები", "კონფიგურაცია",
                "პროფილის ბუსტ პაკეტი", "კრედიტები", "ორივე Wheelspin", "უნარის ქულები", "XP მომატება",
                "დრიფტის ქულის მულტიპლიკატორი", "სიჩქარის ზონის მულტიპლიკატორი",
                "გუბის კორექცია", "სუპერ მოჭიდება", "მაქს. სიჩქარე", "აჩქარება", "აჩქარების რამპა", "გრავიტაცია", "სუპერ მუხრუჭი", "ადაპტური მუხრუჭი", "კრუიზ კონტროლი",
                "No Clip", "სუპერ ძალა", "ხტომა", "ფრენა", "ფაზური დაში", "ბუსტი", "ტელეპორტი გზის წერტილზე", "ავტო რბოლა (მართვა)", "ავტო რბოლა (ტელეპორტი)", "დრიფტის რეჟიმი",
                "მისიის ტაიმერი", "რბოლის ტაიმერი", "დღის დრო", "FOV სლაიდერი", "აშენების ლიმიტის გარეშე", "AI-ის გაყინვა", "უნარის ჯაჭვი არ გაწყდეს",
                "კონფიგის შენახვა", "კონფიგის ჩატვირთვა", "ავტოშენახვა", "ცოცხალი მნიშვნელობები ავტომატურად შეივსება, როცა Luna დაერთვება.",
                "დააჭირე აქ", "პაკეტის რედაქტირება", "კონფიგი", "ღილაკი OFF", "ღილაკი ON", "უსაფრთხო", "მორგებული", "სიჩქარე", "მანძილი", "ყველა", "მორგებული FOV", "ნაგულისხმევი", "შენახვა", "გაუქმება", "გამოყენება",
                "მწვანე ჩართულია. წითელი მაშინვე აჩერებს.", "მწვანე მზადაა. წითელი მაშინვე აჩერებს.");

            AddFeaturePageTranslationSet("Vietnamese",
                "Nhập một số, bật công tắc rồi nhấn Áp dụng. Tắt công tắc sẽ dừng dòng đó ngay.",
                "KINH TẾ & TIẾN TRÌNH", "HỆ SỐ ĐIỂM", "VẬT LÝ & ĐIỀU KHIỂN", "DI CHUYỂN & KỸ NĂNG", "MÔI TRƯỜNG & LUẬT", "CẤU HÌNH",
                "Gói tăng hồ sơ", "Tín dụng", "Cả hai Wheelspin", "Điểm kỹ năng", "Tăng XP",
                "Hệ số điểm drift", "Hệ số vùng tốc độ",
                "Sửa hiệu ứng vũng nước", "Siêu bám đường", "Tốc độ tối đa", "Tăng tốc", "Ramp tăng tốc", "Trọng lực", "Siêu phanh", "Phanh thích ứng", "Cruise Control",
                "Không va chạm", "Siêu sức mạnh", "Nhảy", "Bay", "Lướt xuyên pha", "Tăng lực", "Dịch chuyển tới waypoint", "Đua tự động (lái)", "Đua tự động (dịch chuyển)", "Chế độ drift",
                "Bộ đếm nhiệm vụ", "Bộ đếm cuộc đua", "Thời gian trong ngày", "Thanh FOV", "Không giới hạn build", "Đóng băng AI", "Không đứt chuỗi kỹ năng",
                "Lưu cấu hình", "Tải cấu hình", "Tự động lưu", "Giá trị live tự điền khi Luna kết nối.",
                "Nhấn vào đây", "Sửa gói", "Cấu hình", "Phím OFF", "Phím ON", "An toàn", "Tùy chỉnh", "Tốc độ", "Khoảng cách", "Tất cả", "FOV tùy chỉnh", "Mặc định", "Lưu", "Hủy", "Áp dụng",
                "Xanh là bật. Đỏ dừng ngay.", "Xanh là sẵn sàng. Đỏ dừng ngay.");

            AddFeaturePageTranslationSet("Dutch",
                "Typ een getal, zet de schakelaar aan en druk op Toepassen. Uitzetten stopt die rij meteen.",
                "ECONOMIE & VOORTGANG", "SCOREMULTIPLIERS", "FYSICA & BESTURING", "BEWEGING & VAARDIGHEDEN", "OMGEVING & REGELS", "CONFIGURATIE",
                "Profielboostpakket", "Credits", "Beide Wheelspins", "Skillpunten", "XP-winst",
                "Driftscore-multiplier", "Snelheidszone-multiplier",
                "Plascorrectie", "Supergrip", "Topsnelheid", "Acceleratie", "Acceleratieramp", "Zwaartekracht", "Superrem", "Adaptieve rem", "Cruisecontrol",
                "No Clip", "Superkracht", "Springen", "Vliegen", "Fasedash", "Boost", "Teleport naar waypoint", "Autorace (rijden)", "Autorace (teleport)", "Driftmodus",
                "Missietimer", "Racetimer", "Tijd van de dag", "FOV-schuif", "Geen bouwlimiet", "AI bevriezen", "Geen skillbreuk",
                "Config opslaan", "Config laden", "Autosave", "Livewaarden worden automatisch ingevuld wanneer Luna verbindt.",
                "Klik hier", "Pakket bewerken", "Config", "Toets UIT", "Toets AAN", "Veilig", "Aangepast", "Snelheid", "Afstand", "Alles", "Aangepaste FOV", "Standaard", "Opslaan", "Annuleren", "Toepassen",
                "Groen is aan. Rood stopt meteen.", "Groen is gereed. Rood stopt meteen.");

            AddFeaturePageTranslationSet("Korean",
                "숫자를 입력하고 토글을 켠 뒤 적용을 누르세요. 토글을 끄면 해당 항목이 즉시 중지됩니다.",
                "경제 & 진행", "점수 배율", "물리 & 핸들링", "이동 & 능력", "환경 & 규칙", "구성",
                "프로필 부스트 팩", "크레딧", "양쪽 휠스핀", "스킬 포인트", "XP 증가",
                "드리프트 점수 배율", "스피드 존 배율",
                "물웅덩이 보정", "슈퍼 그립", "최고 속도", "가속", "가속 램프", "중력", "슈퍼 브레이크", "어댑티브 브레이크", "크루즈 컨트롤",
                "노클립", "슈퍼 스트렝스", "점프", "비행", "페이즈 대시", "부스트", "웨이포인트로 텔레포트", "자동 레이스 (주행)", "자동 레이스 (텔레포트)", "드리프트 모드",
                "미션 타이머", "레이스 타이머", "시간대", "FOV 슬라이더", "빌드 제한 없음", "AI 정지", "스킬 체인 유지",
                "구성 저장", "구성 불러오기", "자동 저장", "Luna가 연결되면 라이브 값이 자동으로 채워집니다.",
                "여기를 클릭", "팩 편집", "구성", "키 OFF", "키 ON", "안전", "사용자 지정", "속도", "거리", "전체", "사용자 FOV", "기본값", "저장", "취소", "적용",
                "초록색은 켜짐입니다. 빨간색은 즉시 중지합니다.", "초록색은 준비됨입니다. 빨간색은 즉시 중지합니다.");
        }

        private static void AddFeaturePageTranslationSet(string language, params string[] values)
        {
            var count = Math.Min(FeaturePageUiKeys.Length, values == null ? 0 : values.Length);
            var pairs = new string[count * 2];
            for (var i = 0; i < count; i++)
            {
                pairs[i * 2] = FeaturePageUiKeys[i];
                pairs[(i * 2) + 1] = values[i];
            }

            AddUiTranslations(language, pairs);
        }

        private static void AddUiTranslations(string language, params string[] pairs)
        {
            Dictionary<string, string> map;
            if (!UiTranslations.TryGetValue(language, out map))
                return;

            for (var i = 0; i + 1 < (pairs == null ? 0 : pairs.Length); i += 2)
            {
                if (ShouldTranslateUiKey(pairs[i]) && !IsBrokenTranslationValue(pairs[i + 1]))
                    map[pairs[i]] = pairs[i + 1];
            }
        }

        private static Dictionary<string, string> BuildUiTranslation(params string[] pairs)
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            for (var i = 0; i + 1 < (pairs == null ? 0 : pairs.Length); i += 2)
            {
                if (ShouldTranslateUiKey(pairs[i]) && !IsBrokenTranslationValue(pairs[i + 1]))
                    map[pairs[i]] = pairs[i + 1];
            }
            return map;
        }

        private static bool ShouldTranslateUiKey(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var trimmed = text.Trim();
            if (trimmed.Length == 0)
                return false;

            if (IsNumericOrHotkeyText(trimmed) || IsOperationalOrLogText(trimmed))
                return false;

            return true;
        }

        private static bool IsNumericOrHotkeyText(string text)
        {
            var value = text.EndsWith("%", StringComparison.Ordinal) ? text.Substring(0, text.Length - 1) : text;
            double parsed;
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                return true;

            if (text.Length <= 4 && text.Any(char.IsDigit) && text.All(c => char.IsLetterOrDigit(c) || c == '+' || c == '-'))
                return true;

            return false;
        }

        private static bool IsOperationalOrLogText(string text)
        {
            if (text.StartsWith("[", StringComparison.Ordinal) || text.StartsWith("--", StringComparison.Ordinal))
                return true;

            if (text.IndexOf(".log", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("0x", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            if (text.Equals("running...", StringComparison.OrdinalIgnoreCase) ||
                text.Equals("success", StringComparison.OrdinalIgnoreCase) ||
                text.Equals("failed", StringComparison.OrdinalIgnoreCase))
                return true;

            if (text.EndsWith(" running...", StringComparison.OrdinalIgnoreCase) ||
                text.EndsWith(" success", StringComparison.OrdinalIgnoreCase) ||
                text.EndsWith(" failed", StringComparison.OrdinalIgnoreCase))
                return true;

            if (text.IndexOf(": COMPLETED", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf(": RUNNING", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf(": FAILED", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            return false;
        }

        private static bool IsBrokenTranslationValue(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return true;

            var trimmed = text.Trim();
            if (trimmed.IndexOf('\uFFFD') >= 0)
                return true;

            for (var m = 0; m < trimmed.Length - 1; m++)
            {
                var mc = trimmed[m];
                if ((mc == 'Ã' || mc == 'Â') && trimmed[m + 1] >= '' && trimmed[m + 1] <= 'ÿ')
                    return true;
            }

            var questionCount = trimmed.Count(c => c == '?');
            if (questionCount >= 2 && questionCount * 2 >= trimmed.Count(c => !char.IsWhiteSpace(c)))
                return true;

            for (var i = 0; i < trimmed.Length - 1; i++)
            {
                if (trimmed[i] == '?' && char.IsLetter(trimmed[i + 1]))
                    return true;
            }

            return false;
        }

        private static void SanitizeUiTranslations()
        {
            foreach (var map in UiTranslations.Values)
            {
                var brokenKeys = map
                    .Where(pair => IsBrokenTranslationValue(pair.Value))
                    .Select(pair => pair.Key)
                    .ToList();

                foreach (var key in brokenKeys)
                    map.Remove(key);
            }
        }

        private sealed class ConsoleOverlayForm : Form
        {
            private readonly Action _closeRequested;
            private readonly Action<Rectangle> _boundsCommitted;
            private readonly Panel _root;
            private readonly Panel _header;
            private readonly AnimatedGradientLabel _title;
            private readonly Label _subtitle;
            private readonly Label _close;
            private readonly RichTextBox _log;
            private readonly Label _footer;
            private readonly Label _grip;
            private Color _background;
            private Color _surface;
            private Color _surfaceAlt;
            private Color _border;
            private Color _textPrimary;
            private Color _textMuted;
            private Color _accentBlue;
            private Color _accentGreen;
            private Color _accentRed;
            private Point _dragStartMouse;
            private Point _dragStartLocation;
            private Size _resizeStartSize;
            private bool _dragging;
            private bool _resizing;

            public ConsoleOverlayForm(
                Color background,
                Color surface,
                Color surfaceAlt,
                Color border,
                Color textPrimary,
                Color textMuted,
                Color accentBlue,
                Color accentGreen,
                Color accentRed,
                Action closeRequested,
                Action<Rectangle> boundsCommitted)
            {
                _closeRequested = closeRequested;
                _boundsCommitted = boundsCommitted;
                _background = background;
                _surface = surface;
                _surfaceAlt = surfaceAlt;
                _border = border;
                _textPrimary = textPrimary;
                _textMuted = textMuted;
                _accentBlue = accentBlue;
                _accentGreen = accentGreen;
                _accentRed = accentRed;

                Text = "Luna Console";
                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.Manual;
                ShowInTaskbar = false;
                TopMost = true;
                MinimumSize = new Size(360, 220);
                Size = new Size(580, 360);
                DoubleBuffered = true;
                BackColor = _background;
                Padding = new Padding(1);
                Opacity = 0.96D;

                _root = new Panel();
                _root.Dock = DockStyle.Fill;
                _root.Padding = new Padding(14, 12, 14, 12);
                Controls.Add(_root);

                _header = new Panel();
                _header.Cursor = Cursors.SizeAll;
                _root.Controls.Add(_header);

                _title = new AnimatedGradientLabel();
                _title.Text = "Luna Console";
                _title.Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold);
                _title.AutoSize = false;
                _title.TextAlign = ContentAlignment.MiddleCenter;
                _title.Cursor = Cursors.SizeAll;
                _header.Controls.Add(_title);

                _subtitle = new Label();
                _subtitle.Text = "Live activity feed";
                _subtitle.Font = new Font("Segoe UI", 8.25F);
                _subtitle.AutoSize = false;
                _subtitle.TextAlign = ContentAlignment.MiddleCenter;
                _subtitle.Cursor = Cursors.SizeAll;
                _header.Controls.Add(_subtitle);

                _close = new Label();
                _close.Text = string.Empty;
                _close.AutoSize = false;
                _close.Cursor = Cursors.Hand;
                _close.Click += delegate
                {
                    if (_closeRequested != null)
                        _closeRequested();
                };
                _close.MouseEnter += delegate { _close.BackColor = ControlPaint.Dark(_accentRed, 0.08F); };
                _close.MouseLeave += delegate { _close.BackColor = _accentRed; };
                _close.Paint += delegate(object sender, PaintEventArgs pe)
                {
                    pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    var s = Math.Min(_close.Width, _close.Height);
                    var arm = s * 0.22F;
                    var cx = _close.Width / 2F;
                    var cy = _close.Height / 2F;
                    using (var pen = new Pen(Color.White, 2.2F))
                    {
                        pen.StartCap = LineCap.Round;
                        pen.EndCap = LineCap.Round;
                        pe.Graphics.DrawLine(pen, cx - arm, cy - arm, cx + arm, cy + arm);
                        pe.Graphics.DrawLine(pen, cx + arm, cy - arm, cx - arm, cy + arm);
                    }
                };
                _header.Controls.Add(_close);

                _log = new RichTextBox();
                _log.BorderStyle = BorderStyle.None;
                _log.ReadOnly = true;
                _log.DetectUrls = false;
                _log.HideSelection = false;
                _log.ScrollBars = RichTextBoxScrollBars.Vertical;
                _log.WordWrap = false;
                _log.Font = new Font("Consolas", 9F, FontStyle.Regular);
                _root.Controls.Add(_log);

                _footer = new Label();
                _footer.Font = new Font("Segoe UI", 8F);
                _footer.AutoSize = false;
                _footer.TextAlign = ContentAlignment.MiddleLeft;
                _footer.Text = "Drag header / resize corner";
                _root.Controls.Add(_footer);

                _grip = new Label();
                _grip.Text = "◢";
                _grip.Font = new Font("Segoe UI Symbol", 10F);
                _grip.AutoSize = false;
                _grip.TextAlign = ContentAlignment.MiddleCenter;
                _grip.Cursor = Cursors.SizeNWSE;
                _grip.Size = new Size(22, 22);
                Controls.Add(_grip);
                _grip.BringToFront();

                WireDrag(_header);
                WireDrag(_title);
                WireDrag(_subtitle);
                _grip.MouseDown += GripMouseDown;
                _grip.MouseMove += GripMouseMove;
                _grip.MouseUp += GripMouseUp;
                _grip.MouseCaptureChanged += delegate
                {
                    if (_resizing && Control.MouseButtons != MouseButtons.Left)
                        StopResizing(true);
                };
                MouseMove += HeaderMouseMove;
                MouseUp += HeaderMouseUp;
                MouseCaptureChanged += delegate
                {
                    if (_dragging && Control.MouseButtons != MouseButtons.Left)
                        StopDragging(true);
                    if (_resizing && Control.MouseButtons != MouseButtons.Left)
                        StopResizing(true);
                };
                Resize += delegate { LayoutOverlay(); };
                ApplyPalette(background, surface, surfaceAlt, border, textPrimary, textMuted, accentBlue, accentGreen, accentRed);
                LayoutOverlay();
            }

            public void ApplyPalette(
                Color background,
                Color surface,
                Color surfaceAlt,
                Color border,
                Color textPrimary,
                Color textMuted,
                Color accentBlue,
                Color accentGreen,
                Color accentRed)
            {
                _background = background;
                _surface = surface;
                _surfaceAlt = surfaceAlt;
                _border = border;
                _textPrimary = textPrimary;
                _textMuted = textMuted;
                _accentBlue = accentBlue;
                _accentGreen = accentGreen;
                _accentRed = accentRed;

                BackColor = _background;
                _root.BackColor = _surface;
                _header.BackColor = _surface;
                _log.BackColor = _surfaceAlt;
                _log.ForeColor = _textPrimary;
                _subtitle.ForeColor = _textMuted;
                _footer.BackColor = _surface;
                _footer.ForeColor = _textMuted;
                _close.ForeColor = Color.White;
                _close.BackColor = _accentRed;
                _grip.ForeColor = _accentBlue;
                _grip.BackColor = _surface;
                Invalidate(true);
            }

            public void SetLogText(string text)
            {
                text = TrimText(text);
                if (string.Equals(_log.Text, text, StringComparison.Ordinal))
                    return;

                _log.SuspendLayout();
                try
                {
                    _log.Clear();
                    _log.ForeColor = _textPrimary;
                    _log.Text = text ?? string.Empty;
                    ScrollToBottom();
                }
                finally
                {
                    _log.ResumeLayout();
                }
            }

            public void AppendLine(string line, Color color)
            {
                if (string.IsNullOrEmpty(line))
                    return;

                if (_log.TextLength + line.Length > ConsoleOverlayMaxCharacters)
                {
                    SetLogText(_log.Text + line);
                    return;
                }

                var start = _log.TextLength;
                _log.AppendText(line);
                _log.Select(start, line.Length);
                _log.SelectionColor = color;
                _log.SelectionLength = 0;
                ScrollToBottom();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, Width - 1, Height - 1);
                using (var path = CreateRoundPath(rect, 14))
                using (var fill = new SolidBrush(_surface))
                using (var pen = new Pen(_border, 1.25F))
                {
                    e.Graphics.FillPath(fill, path);
                    e.Graphics.DrawPath(pen, path);
                }
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                using (var path = CreateRoundPath(new Rectangle(0, 0, Width, Height), 14))
                    Region = new Region(path);
                if (_root != null)
                    LayoutOverlay();
            }

            private void LayoutOverlay()
            {
                if (_header == null || _log == null || _footer == null || _grip == null)
                    return;

                var left = _root.Padding.Left;
                var top = _root.Padding.Top;
                var width = Math.Max(120, _root.ClientSize.Width - _root.Padding.Left - _root.Padding.Right);
                const int headerHeight = 54;
                const int footerHeight = 24;
                _header.SetBounds(left, top, width, headerHeight);
                _title.SetBounds(42, 0, Math.Max(20, _header.Width - 84), 30);
                _subtitle.SetBounds(42, 29, Math.Max(20, _header.Width - 84), 20);
                _close.SetBounds(Math.Max(0, _header.Width - 34), 1, 32, 32);
                RoundCloseButton();

                var footerTop = Math.Max(_header.Bottom + 60, _root.ClientSize.Height - _root.Padding.Bottom - footerHeight);
                _footer.SetBounds(left, footerTop, width, footerHeight);
                _log.SetBounds(left, _header.Bottom + 8, width, Math.Max(60, _footer.Top - _header.Bottom - 10));
                using (var path = CreateRoundPath(new Rectangle(0, 0, Math.Max(1, _log.Width), Math.Max(1, _log.Height)), 10))
                    _log.Region = new Region(path);

                _grip.Location = new Point(Math.Max(0, ClientSize.Width - _grip.Width - 4), Math.Max(0, ClientSize.Height - _grip.Height - 4));
                Invalidate();
            }

            private void RoundCloseButton()
            {
                if (_close.Width <= 1 || _close.Height <= 1)
                    return;
                using (var path = CreateRoundPath(new Rectangle(0, 0, _close.Width, _close.Height), 10))
                    _close.Region = new Region(path);
            }

            private void ScrollToBottom()
            {
                _log.SelectionStart = _log.TextLength;
                _log.ScrollToCaret();
            }

            private static string TrimText(string text)
            {
                if (string.IsNullOrEmpty(text) || text.Length <= ConsoleOverlayMaxCharacters)
                    return text ?? string.Empty;
                return text.Substring(text.Length - ConsoleOverlayMaxCharacters);
            }

            private void WireDrag(Control control)
            {
                control.MouseDown += HeaderMouseDown;
                control.MouseMove += HeaderMouseMove;
                control.MouseUp += HeaderMouseUp;
                control.MouseCaptureChanged += delegate
                {
                    if (_dragging && Control.MouseButtons != MouseButtons.Left)
                        StopDragging(true);
                };
            }

            private void HeaderMouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left)
                    return;

                _dragging = true;
                _dragStartMouse = Cursor.Position;
                _dragStartLocation = Location;
                var control = sender as Control;
                if (control != null)
                    control.Capture = true;
            }

            private void HeaderMouseMove(object sender, MouseEventArgs e)
            {
                if (!_dragging)
                    return;
                var delta = new Size(Cursor.Position.X - _dragStartMouse.X, Cursor.Position.Y - _dragStartMouse.Y);
                Location = _dragStartLocation + delta;
            }

            private void HeaderMouseUp(object sender, MouseEventArgs e)
            {
                if (_dragging)
                    StopDragging(true);
            }

            private void GripMouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left)
                    return;

                _resizing = true;
                _dragStartMouse = Cursor.Position;
                _resizeStartSize = Size;
                _grip.Capture = true;
            }

            private void GripMouseMove(object sender, MouseEventArgs e)
            {
                if (!_resizing)
                    return;
                var dx = Cursor.Position.X - _dragStartMouse.X;
                var dy = Cursor.Position.Y - _dragStartMouse.Y;
                Size = new Size(
                    Math.Max(MinimumSize.Width, _resizeStartSize.Width + dx),
                    Math.Max(MinimumSize.Height, _resizeStartSize.Height + dy));
            }

            private void GripMouseUp(object sender, MouseEventArgs e)
            {
                if (_resizing)
                    StopResizing(true);
            }

            private void StopDragging(bool commit)
            {
                if (!_dragging)
                    return;
                _dragging = false;
                Capture = false;
                if (_header != null)
                    _header.Capture = false;
                if (_title != null)
                    _title.Capture = false;
                if (_subtitle != null)
                    _subtitle.Capture = false;
                if (commit)
                    CommitBounds();
            }

            private void StopResizing(bool commit)
            {
                if (!_resizing)
                    return;
                _resizing = false;
                Capture = false;
                if (_grip != null)
                    _grip.Capture = false;
                if (commit)
                    CommitBounds();
            }

            private void CommitBounds()
            {
                if (_boundsCommitted != null)
                    _boundsCommitted(Bounds);
            }

            private static GraphicsPath CreateRoundPath(Rectangle rect, int radius)
            {
                var path = new GraphicsPath();
                if (rect.Width <= 0 || rect.Height <= 0)
                    return path;
                radius = Math.Max(1, Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2));
                var diameter = radius * 2;
                var arc = new Rectangle(rect.X, rect.Y, diameter, diameter);
                path.AddArc(arc, 180, 90);
                arc.X = rect.Right - diameter;
                path.AddArc(arc, 270, 90);
                arc.Y = rect.Bottom - diameter;
                path.AddArc(arc, 0, 90);
                arc.X = rect.Left;
                path.AddArc(arc, 90, 90);
                path.CloseFigure();
                return path;
            }
        }

        private sealed class ChargeJumpMeterForm : Form
        {
            private readonly Panel _root;
            private readonly AnimatedGradientLabel _title;
            private readonly Label _subtitle;
            private readonly Panel _barBack;
            private readonly Panel _barFill;
            private readonly Label _percentLabel;
            private Color _background;
            private Color _surface;
            private Color _surfaceAlt;
            private Color _border;
            private Color _textPrimary;
            private Color _textMuted;
            private Color _accentBlue;
            private Color _accentPurple;
            private Color _accentGreen;
            private bool _dragging;
            private Point _dragStartMouse;
            private Point _dragStartLocation;

            public ChargeJumpMeterForm(
                Color background,
                Color surface,
                Color surfaceAlt,
                Color border,
                Color textPrimary,
                Color textMuted,
                Color accentBlue,
                Color accentPurple,
                Color accentGreen)
            {
                Text = "Charge Jump";
                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.Manual;
                ShowInTaskbar = false;
                TopMost = true;
                Size = new Size(330, 94);
                MinimumSize = Size;
                MaximumSize = Size;
                DoubleBuffered = true;
                Padding = new Padding(1);
                Opacity = 0.96D;

                _root = new Panel();
                _root.Dock = DockStyle.Fill;
                _root.Padding = new Padding(14, 10, 14, 12);
                _root.Cursor = Cursors.SizeAll;
                Controls.Add(_root);

                _title = new AnimatedGradientLabel();
                _title.Text = "Charge Jump";
                _title.Font = new Font("Segoe UI Semibold", 11F);
                _title.AutoSize = false;
                _title.TextAlign = ContentAlignment.MiddleLeft;
                _title.Cursor = Cursors.SizeAll;
                _root.Controls.Add(_title);

                _subtitle = new Label();
                _subtitle.Text = "Hold to charge, release to launch";
                _subtitle.Font = new Font("Segoe UI", 8.25F);
                _subtitle.AutoSize = false;
                _subtitle.TextAlign = ContentAlignment.MiddleLeft;
                _subtitle.Cursor = Cursors.SizeAll;
                _root.Controls.Add(_subtitle);

                _percentLabel = new Label();
                _percentLabel.Font = new Font("Segoe UI Semibold", 10F);
                _percentLabel.AutoSize = false;
                _percentLabel.TextAlign = ContentAlignment.MiddleRight;
                _percentLabel.Cursor = Cursors.SizeAll;
                _root.Controls.Add(_percentLabel);

                _barBack = new Panel();
                _barBack.Height = 14;
                _barBack.Cursor = Cursors.SizeAll;
                _root.Controls.Add(_barBack);

                _barFill = new Panel();
                _barFill.Height = 14;
                _barBack.Controls.Add(_barFill);

                WireDrag(this);
                WireDrag(_root);
                WireDrag(_title);
                WireDrag(_subtitle);
                WireDrag(_percentLabel);
                WireDrag(_barBack);
                WireDrag(_barFill);
                MouseMove += DragMouseMove;
                MouseUp += DragMouseUp;
                MouseCaptureChanged += delegate
                {
                    if (_dragging && Control.MouseButtons != MouseButtons.Left)
                        StopDragging();
                };
                Resize += delegate { LayoutMeter(); };
                ApplyPalette(background, surface, surfaceAlt, border, textPrimary, textMuted, accentBlue, accentPurple, accentGreen);
                SetPercent(0);
                LayoutMeter();
            }

            public void ApplyPalette(
                Color background,
                Color surface,
                Color surfaceAlt,
                Color border,
                Color textPrimary,
                Color textMuted,
                Color accentBlue,
                Color accentPurple,
                Color accentGreen)
            {
                _background = background;
                _surface = surface;
                _surfaceAlt = surfaceAlt;
                _border = border;
                _textPrimary = textPrimary;
                _textMuted = textMuted;
                _accentBlue = accentBlue;
                _accentPurple = accentPurple;
                _accentGreen = accentGreen;

                BackColor = _background;
                _root.BackColor = _surface;
                _title.ForeColor = _textPrimary;
                _subtitle.ForeColor = _textMuted;
                _percentLabel.ForeColor = _accentGreen;
                _percentLabel.BackColor = _surface;
                _barBack.BackColor = BlendMeterColor(_surfaceAlt, _border, 0.20F);
                _barFill.BackColor = _accentGreen;
                Invalidate(true);
            }

            public void SetPercent(int percent)
            {
                var clamped = Math.Max(0, Math.Min(100, percent));
                if (_percentLabel != null)
                    _percentLabel.Text = clamped.ToString(CultureInfo.InvariantCulture) + "%";
                if (_barBack != null && _barFill != null)
                {
                    var width = Math.Max(0, (int)Math.Round((_barBack.ClientSize.Width * clamped) / 100.0));
                    _barFill.SetBounds(0, 0, width, _barBack.ClientSize.Height);
                    _barFill.BackColor = BlendMeterColor(_accentBlue, _accentPurple, clamped / 100F);
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, Width - 1, Height - 1);
                using (var path = CreateMeterRoundPath(rect, 14))
                using (var fill = new SolidBrush(_surface))
                using (var pen = new Pen(_border, 1.25F))
                {
                    e.Graphics.FillPath(fill, path);
                    e.Graphics.DrawPath(pen, path);
                }
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                using (var path = CreateMeterRoundPath(new Rectangle(0, 0, Width, Height), 14))
                    Region = new Region(path);
                LayoutMeter();
            }

            private void LayoutMeter()
            {
                if (_root == null || _title == null || _subtitle == null || _percentLabel == null || _barBack == null)
                    return;

                var left = _root.Padding.Left;
                var top = _root.Padding.Top;
                var width = Math.Max(80, _root.ClientSize.Width - _root.Padding.Left - _root.Padding.Right);
                _title.SetBounds(left, top, width - 76, 24);
                _percentLabel.SetBounds(left + width - 72, top, 72, 24);
                _subtitle.SetBounds(left, top + 25, width, 20);
                _barBack.SetBounds(left, top + 56, width, 14);
                SetPercent(ParsePercentLabel());
                Invalidate();
            }

            private static GraphicsPath CreateMeterRoundPath(Rectangle rect, int radius)
            {
                var path = new GraphicsPath();
                var diameter = Math.Max(1, radius * 2);
                path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
                path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
                path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
                path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90);
                path.CloseFigure();
                return path;
            }

            private static Color BlendMeterColor(Color from, Color to, float amount)
            {
                if (amount < 0F) amount = 0F;
                if (amount > 1F) amount = 1F;
                var inv = 1F - amount;
                return Color.FromArgb(
                    (int)Math.Round((from.A * inv) + (to.A * amount)),
                    (int)Math.Round((from.R * inv) + (to.R * amount)),
                    (int)Math.Round((from.G * inv) + (to.G * amount)),
                    (int)Math.Round((from.B * inv) + (to.B * amount)));
            }

            private int ParsePercentLabel()
            {
                if (_percentLabel == null)
                    return 0;
                var text = (_percentLabel.Text ?? string.Empty).Trim().TrimEnd('%');
                int value;
                return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ? value : 0;
            }

            private void WireDrag(Control control)
            {
                if (control == null)
                    return;
                control.MouseDown += DragMouseDown;
                control.MouseMove += DragMouseMove;
                control.MouseUp += DragMouseUp;
                control.MouseCaptureChanged += delegate
                {
                    if (_dragging && Control.MouseButtons != MouseButtons.Left)
                        StopDragging();
                };
            }

            private void DragMouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left)
                    return;
                _dragging = true;
                _dragStartMouse = Cursor.Position;
                _dragStartLocation = Location;
                var control = sender as Control;
                if (control != null)
                    control.Capture = true;
            }

            private void DragMouseMove(object sender, MouseEventArgs e)
            {
                if (!_dragging)
                    return;
                var delta = new Size(Cursor.Position.X - _dragStartMouse.X, Cursor.Position.Y - _dragStartMouse.Y);
                Location = _dragStartLocation + delta;
            }

            private void DragMouseUp(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                    StopDragging();
            }

            private void StopDragging()
            {
                _dragging = false;
                foreach (Control control in Controls)
                    control.Capture = false;
                if (_root != null)
                    _root.Capture = false;
            }
        }

        private sealed class FeatureOverlayForm : Form
        {
            private readonly Action _closeRequested;
            private readonly Action<Rectangle> _boundsCommitted;
            private readonly Panel _root;
            private readonly Panel _header;
            private readonly Label _title;
            private readonly Label _subtitle;
            private readonly Label _close;
            private readonly Panel _list;
            private readonly OverlayItemsPanel _listContent;
            private readonly Label _footer;
            private readonly Label _grip;
            private Color _background;
            private Color _surface;
            private Color _surfaceAlt;
            private Color _border;
            private Color _textPrimary;
            private Color _textMuted;
            private Color _accentGreen;
            private Color _accentBlue;
            private Color _accentRed;
            private Point _dragStartMouse;
            private Point _dragStartLocation;
            private Size _resizeStartSize;
            private bool _dragging;
            private bool _resizing;
            private string _hotkeyText;

            public FeatureOverlayForm(
                Color background,
                Color surface,
                Color surfaceAlt,
                Color border,
                Color textPrimary,
                Color textMuted,
                Color accentGreen,
                Color accentBlue,
                Color accentRed,
                string hotkeyText,
                Action closeRequested,
                Action<Rectangle> boundsCommitted)
            {
                _closeRequested = closeRequested;
                _boundsCommitted = boundsCommitted;
                _hotkeyText = hotkeyText ?? "F3";
                _background = background;
                _surface = surface;
                _surfaceAlt = surfaceAlt;
                _border = border;
                _textPrimary = textPrimary;
                _textMuted = textMuted;
                _accentGreen = accentGreen;
                _accentBlue = accentBlue;
                _accentRed = accentRed;

                Text = "Luna Feature Overlay";
                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.Manual;
                ShowInTaskbar = false;
                TopMost = true;
                MinimumSize = new Size(260, 150);
                Size = new Size(340, 260);
                DoubleBuffered = true;
                BackColor = _background;
                Padding = new Padding(1);
                Opacity = 0.96D;

                _root = new Panel();
                _root.Dock = DockStyle.Fill;
                _root.Padding = new Padding(12, 10, 12, 12);
                Controls.Add(_root);

                _header = new Panel();
                _header.Height = 46;
                _header.Dock = DockStyle.None;
                _header.Cursor = Cursors.SizeAll;
                _root.Controls.Add(_header);

                _title = new AnimatedGradientLabel();
                _title.Text = "Luna";
                _title.Font = new Font("Segoe UI Semibold", 12F);
                _title.AutoSize = false;
                _title.TextAlign = ContentAlignment.MiddleCenter;
                _title.Cursor = Cursors.SizeAll;
                _header.Controls.Add(_title);

                _subtitle = new Label();
                _subtitle.Font = new Font("Segoe UI", 8.25F);
                _subtitle.AutoSize = false;
                _subtitle.TextAlign = ContentAlignment.MiddleCenter;
                _subtitle.Cursor = Cursors.SizeAll;
                _header.Controls.Add(_subtitle);

                _close = new Label();
                _close.Text = "×";
                _close.Font = new Font("Segoe UI Semibold", 12F);
                _close.AutoSize = false;
                _close.TextAlign = ContentAlignment.MiddleCenter;
                _close.Cursor = Cursors.Hand;
                _close.Click += delegate
                {
                    if (_closeRequested != null)
                        _closeRequested();
                };
                _header.Controls.Add(_close);

                _list = new Panel();
                _list.Dock = DockStyle.None;
                _list.AutoScroll = true;
                _list.Padding = Padding.Empty;
                _root.Controls.Add(_list);

                _listContent = new OverlayItemsPanel();
                _listContent.Location = Point.Empty;
                _listContent.BackColor = _surface;
                _list.Controls.Add(_listContent);

                _footer = new Label();
                _footer.Height = 24;
                _footer.Dock = DockStyle.None;
                _footer.Font = new Font("Segoe UI", 8F);
                _footer.AutoSize = false;
                _footer.TextAlign = ContentAlignment.MiddleLeft;
                _root.Controls.Add(_footer);

                _grip = new Label();
                _grip.Text = "◢";
                _grip.Font = new Font("Segoe UI Symbol", 10F);
                _grip.AutoSize = false;
                _grip.TextAlign = ContentAlignment.MiddleCenter;
                _grip.Cursor = Cursors.SizeNWSE;
                _grip.Size = new Size(22, 22);
                Controls.Add(_grip);
                _grip.BringToFront();

                WireDrag(_header);
                WireDrag(_title);
                WireDrag(_subtitle);
                _grip.MouseDown += GripMouseDown;
                _grip.MouseMove += GripMouseMove;
                _grip.MouseUp += GripMouseUp;
                _grip.MouseCaptureChanged += delegate
                {
                    if (_resizing && Control.MouseButtons != MouseButtons.Left)
                        StopResizing(true);
                };
                MouseMove += HeaderMouseMove;
                MouseUp += HeaderMouseUp;
                MouseCaptureChanged += delegate
                {
                    if (_dragging && Control.MouseButtons != MouseButtons.Left)
                        StopDragging(true);
                    if (_resizing && Control.MouseButtons != MouseButtons.Left)
                        StopResizing(true);
                };
                Resize += delegate { LayoutOverlay(); };
                ApplyPalette(background, surface, surfaceAlt, border, textPrimary, textMuted, accentGreen, accentBlue, accentRed);
                LayoutOverlay();
            }

            public void SetHotkeyText(string text)
            {
                _hotkeyText = string.IsNullOrWhiteSpace(text) ? "None" : text;
                UpdateFooterText();
            }

            public void ApplyPalette(
                Color background,
                Color surface,
                Color surfaceAlt,
                Color border,
                Color textPrimary,
                Color textMuted,
                Color accentGreen,
                Color accentBlue,
                Color accentRed)
            {
                _background = background;
                _surface = surface;
                _surfaceAlt = surfaceAlt;
                _border = border;
                _textPrimary = textPrimary;
                _textMuted = textMuted;
                _accentGreen = accentGreen;
                _accentBlue = accentBlue;
                _accentRed = accentRed;

                BackColor = _background;
                _root.BackColor = _surface;
                _header.BackColor = _surface;
                _list.BackColor = _surface;
                _listContent.BackColor = _surface;
                _listContent.SetPalette(_surface, _surfaceAlt, _border, _textPrimary, _textMuted, _accentGreen);
                _footer.BackColor = _surface;
                _title.ForeColor = _textPrimary;
                _subtitle.ForeColor = _textMuted;
                _close.ForeColor = _accentRed;
                _close.BackColor = _surface;
                _grip.ForeColor = _accentBlue;
                _grip.BackColor = _surface;
                UpdateFooterText();
                Invalidate(true);
            }

            public void UpdateItems(IList<FeatureOverlayItem> items)
            {
                if (items == null)
                    items = new List<FeatureOverlayItem>();

                _subtitle.Text = items.Count.ToString(CultureInfo.InvariantCulture) + " enabled";
                _list.SuspendLayout();
                try
                {
                    ResetListScrollToTop();
                    _listContent.SetItems(items);
                    LayoutRows();
                    ResetListScrollToTop();
                }
                finally
                {
                    _list.ResumeLayout(true);
                    _listContent.Invalidate(true);
                    _listContent.Update();
                    _list.Invalidate(true);
                    _list.Update();
                    Invalidate(true);
                    Update();
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, Width - 1, Height - 1);
                using (var path = CreateRoundPath(rect, 14))
                using (var fill = new SolidBrush(_surface))
                using (var pen = new Pen(_border, 1.25F))
                {
                    e.Graphics.FillPath(fill, path);
                    e.Graphics.DrawPath(pen, path);
                }
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                using (var path = CreateRoundPath(new Rectangle(0, 0, Width, Height), 14))
                    Region = new Region(path);
                if (_root != null)
                    LayoutOverlay();
            }

            private void LayoutOverlay()
            {
                if (_header == null || _title == null || _subtitle == null || _close == null || _grip == null)
                    return;

                var left = _root.Padding.Left;
                var top = _root.Padding.Top;
                var width = Math.Max(80, _root.ClientSize.Width - _root.Padding.Left - _root.Padding.Right);
                var footerHeight = 24;
                var footerTop = Math.Max(top + _header.Height, _root.ClientSize.Height - _root.Padding.Bottom - footerHeight);
                _header.SetBounds(left, top, width, 46);
                _footer.SetBounds(left, footerTop, width, footerHeight);
                _list.SetBounds(left, _header.Bottom, width, Math.Max(30, _footer.Top - _header.Bottom));

                _title.SetBounds(30, 0, Math.Max(20, _header.Width - 60), 24);
                _subtitle.SetBounds(30, 23, Math.Max(20, _header.Width - 60), 20);
                _close.SetBounds(Math.Max(0, _header.Width - 30), 2, 28, 28);
                _grip.Location = new Point(Math.Max(0, ClientSize.Width - _grip.Width - 4), Math.Max(0, ClientSize.Height - _grip.Height - 4));
                LayoutRows();
                Invalidate();
            }

            private void LayoutRows()
            {
                if (_list == null || _listContent == null)
                    return;

                var width = Math.Max(180, _list.ClientSize.Width - 2);
                var preferredHeight = _listContent.GetPreferredContentHeight();
                _listContent.Location = Point.Empty;
                _listContent.Size = new Size(width, Math.Max(_list.ClientSize.Height, preferredHeight));
                _list.AutoScrollMinSize = new Size(width, preferredHeight);
            }

            private void ResetListScrollToTop()
            {
                if (_list == null)
                    return;

                try
                {
                    _list.AutoScrollPosition = Point.Empty;
                }
                catch
                {
                }
            }

            private void UpdateFooterText()
            {
                if (_footer != null)
                {
                    _footer.ForeColor = _textMuted;
                    _footer.Text = "Hotkey: " + _hotkeyText + "  •  Drag header / resize corner";
                }
            }

            private Control CreateOverlayRow(FeatureOverlayItem item, bool enabled)
            {
                var row = new Panel();
                row.Height = 36;
                row.Margin = new Padding(0, 0, 0, 6);
                row.BackColor = enabled ? BlendColor(_surfaceAlt, _accentGreen, 0.08F) : _surfaceAlt;

                var dot = new Label();
                dot.Text = "●";
                dot.Font = new Font("Segoe UI Symbol", 8F);
                dot.ForeColor = enabled ? _accentGreen : _textMuted;
                dot.BackColor = Color.Transparent;
                dot.TextAlign = ContentAlignment.MiddleCenter;
                dot.SetBounds(8, 0, 20, 36);
                row.Controls.Add(dot);

                var label = new Label();
                label.Name = "Name";
                label.Text = item == null ? string.Empty : item.Label;
                label.Font = new Font("Segoe UI Semibold", 9F);
                label.ForeColor = enabled ? _textPrimary : _textMuted;
                label.BackColor = Color.Transparent;
                label.AutoEllipsis = true;
                label.TextAlign = ContentAlignment.MiddleLeft;
                label.SetBounds(32, 0, Math.Max(20, Width - 84), 36);
                label.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                row.Controls.Add(label);

                var value = new Label();
                value.Name = "Value";
                value.Text = item == null ? string.Empty : item.Value;
                value.Font = new Font("Segoe UI", 8.25F);
                value.ForeColor = _textMuted;
                value.BackColor = Color.Transparent;
                value.AutoEllipsis = true;
                value.TextAlign = ContentAlignment.MiddleRight;
                value.SetBounds(Math.Max(64, row.Width - 118), 0, 108, 36);
                value.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                row.Controls.Add(value);

                row.Resize += delegate { LayoutOverlayRow(row); RoundOverlayRow(row); };
                LayoutOverlayRow(row);
                RoundOverlayRow(row);
                return row;
            }

            private void RoundOverlayRow(Control row)
            {
                if (row == null || row.Width <= 1 || row.Height <= 1)
                    return;
                using (var path = CreateRoundPath(new Rectangle(0, 0, row.Width, row.Height), 12))
                    row.Region = new Region(path);
            }

            private static string BuildItemsSignature(IList<FeatureOverlayItem> items)
            {
                var sb = new StringBuilder();
                foreach (var item in items)
                {
                    if (item == null)
                        continue;
                    sb.Append(item.Key).Append('\t').Append(item.Label).Append('\t').Append(item.Value).Append('\n');
                }
                return sb.ToString();
            }

            private void LayoutOverlayRow(Control row)
            {
                if (row == null)
                    return;
                var width = Math.Max(180, row.Width);
                var valueLabel = row.Controls.Find("Value", false).FirstOrDefault() as Label;
                var nameLabel = row.Controls.Find("Name", false).FirstOrDefault() as Label;
                var hasValue = valueLabel != null && !string.IsNullOrWhiteSpace(valueLabel.Text);
                var valueWidth = hasValue ? Math.Min(150, Math.Max(78, width / 3)) : 0;
                if (valueLabel != null)
                {
                    valueLabel.Visible = hasValue;
                    valueLabel.SetBounds(width - valueWidth - 12, 0, valueWidth, row.Height);
                }
                if (nameLabel != null)
                {
                    var right = hasValue ? valueWidth + 22 : 12;
                    nameLabel.SetBounds(32, 0, Math.Max(30, width - 32 - right), row.Height);
                }
            }

            private sealed class OverlayItemsPanel : Panel
            {
                private const int RowHeight = 36;
                private const int RowGap = 6;
                private const int TopPadding = 6;
                private const int BottomPadding = 10;
                private List<FeatureOverlayItem> _items = new List<FeatureOverlayItem>();
                private Color _surface = Color.FromArgb(24, 24, 28);
                private Color _surfaceAlt = Color.FromArgb(32, 32, 38);
                private Color _border = Color.FromArgb(55, 58, 66);
                private Color _textPrimary = Color.White;
                private Color _textMuted = Color.FromArgb(190, 198, 214);
                private Color _accentGreen = Color.FromArgb(34, 197, 94);

                public OverlayItemsPanel()
                {
                    SetStyle(
                        ControlStyles.AllPaintingInWmPaint |
                        ControlStyles.OptimizedDoubleBuffer |
                        ControlStyles.ResizeRedraw |
                        ControlStyles.UserPaint,
                        true);
                }

                public int GetPreferredContentHeight()
                {
                    var rows = Math.Max(1, _items.Count);
                    return TopPadding + (rows * RowHeight) + ((rows - 1) * RowGap) + BottomPadding;
                }

                public void SetPalette(Color surface, Color surfaceAlt, Color border, Color textPrimary, Color textMuted, Color accentGreen)
                {
                    _surface = surface;
                    _surfaceAlt = surfaceAlt;
                    _border = border;
                    _textPrimary = textPrimary;
                    _textMuted = textMuted;
                    _accentGreen = accentGreen;
                    BackColor = _surface;
                    Invalidate();
                }

                public void SetItems(IList<FeatureOverlayItem> items)
                {
                    _items = items == null
                        ? new List<FeatureOverlayItem>()
                        : items.Where(item => item != null).ToList();
                    Invalidate();
                }

                protected override void OnPaint(PaintEventArgs e)
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.Clear(_surface);

                    using (var nameFont = new Font("Segoe UI Semibold", 9F))
                    using (var valueFont = new Font("Segoe UI", 8.25F))
                    {
                        var y = TopPadding;
                        if (_items.Count == 0)
                        {
                            DrawRow(
                                e.Graphics,
                                new Rectangle(0, y, Math.Max(1, ClientSize.Width - 2), RowHeight),
                                "No Luna features enabled",
                                string.Empty,
                                false,
                                nameFont,
                                valueFont);
                            return;
                        }

                        foreach (var item in _items)
                        {
                            DrawRow(
                                e.Graphics,
                                new Rectangle(0, y, Math.Max(1, ClientSize.Width - 2), RowHeight),
                                item.Label,
                                item.Value,
                                true,
                                nameFont,
                                valueFont);
                            y += RowHeight + RowGap;
                        }
                    }
                }

                private void DrawRow(Graphics graphics, Rectangle bounds, string label, string value, bool enabled, Font nameFont, Font valueFont)
                {
                    if (bounds.Width <= 1 || bounds.Height <= 1)
                        return;

                    var fillColor = enabled ? BlendColor(_surfaceAlt, _accentGreen, 0.08F) : _surfaceAlt;
                    using (var path = CreateRoundPath(bounds, 12))
                    using (var fill = new SolidBrush(fillColor))
                    using (var border = new Pen(_border, 1F))
                    {
                        graphics.FillPath(fill, path);
                        graphics.DrawPath(border, path);
                    }

                    var dotSize = 8;
                    var dotRect = new Rectangle(bounds.Left + 14, bounds.Top + (bounds.Height - dotSize) / 2, dotSize, dotSize);
                    using (var dotBrush = new SolidBrush(enabled ? _accentGreen : _textMuted))
                        graphics.FillEllipse(dotBrush, dotRect);

                    var hasValue = !string.IsNullOrWhiteSpace(value);
                    var valueWidth = hasValue ? Math.Min(150, Math.Max(78, bounds.Width / 3)) : 0;
                    if (hasValue)
                    {
                        var valueRect = new Rectangle(bounds.Right - valueWidth - 12, bounds.Top, valueWidth, bounds.Height);
                        TextRenderer.DrawText(
                            graphics,
                            value,
                            valueFont,
                            valueRect,
                            _textMuted,
                            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                    }

                    var rightPadding = hasValue ? valueWidth + 22 : 12;
                    var nameRect = new Rectangle(bounds.Left + 32, bounds.Top, Math.Max(30, bounds.Width - 32 - rightPadding), bounds.Height);
                    TextRenderer.DrawText(
                        graphics,
                        label ?? string.Empty,
                        nameFont,
                        nameRect,
                        enabled ? _textPrimary : _textMuted,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                }
            }

            private void WireDrag(Control control)
            {
                control.MouseDown += HeaderMouseDown;
                control.MouseMove += HeaderMouseMove;
                control.MouseUp += HeaderMouseUp;
                control.MouseCaptureChanged += delegate
                {
                    if (_dragging && Control.MouseButtons != MouseButtons.Left)
                        StopDragging(true);
                };
            }

            private void HeaderMouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left)
                    return;

                if (_dragging)
                {
                    StopDragging(true);
                    return;
                }

                _dragging = true;
                _dragStartMouse = Cursor.Position;
                _dragStartLocation = Location;
                var control = sender as Control;
                if (control != null)
                    control.Capture = true;
                Capture = true;
            }

            private void HeaderMouseMove(object sender, MouseEventArgs e)
            {
                if (!_dragging)
                    return;
                var delta = new Size(Cursor.Position.X - _dragStartMouse.X, Cursor.Position.Y - _dragStartMouse.Y);
                Location = _dragStartLocation + delta;
            }

            private void HeaderMouseUp(object sender, MouseEventArgs e)
            {
                if (!_dragging)
                    return;
                StopDragging(true);
            }

            private void GripMouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left)
                    return;
                _resizing = true;
                _dragStartMouse = Cursor.Position;
                _resizeStartSize = Size;
                _grip.Capture = true;
            }

            private void GripMouseMove(object sender, MouseEventArgs e)
            {
                if (!_resizing)
                    return;
                var dx = Cursor.Position.X - _dragStartMouse.X;
                var dy = Cursor.Position.Y - _dragStartMouse.Y;
                Size = new Size(Math.Max(MinimumSize.Width, _resizeStartSize.Width + dx), Math.Max(MinimumSize.Height, _resizeStartSize.Height + dy));
            }

            private void GripMouseUp(object sender, MouseEventArgs e)
            {
                if (!_resizing)
                    return;
                StopResizing(true);
            }

            private void StopDragging(bool commit)
            {
                if (!_dragging)
                    return;
                _dragging = false;
                Capture = false;
                if (_header != null)
                    _header.Capture = false;
                if (_title != null)
                    _title.Capture = false;
                if (_subtitle != null)
                    _subtitle.Capture = false;
                if (commit)
                    CommitBounds();
            }

            private void StopResizing(bool commit)
            {
                if (!_resizing)
                    return;
                _resizing = false;
                if (_grip != null)
                    _grip.Capture = false;
                if (commit)
                    CommitBounds();
            }

            private void CommitBounds()
            {
                if (_boundsCommitted != null)
                    _boundsCommitted(Bounds);
            }

            private static GraphicsPath CreateRoundPath(Rectangle rect, int radius)
            {
                var path = new GraphicsPath();
                if (rect.Width <= 0 || rect.Height <= 0)
                    return path;
                radius = Math.Max(1, Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2));
                var diameter = radius * 2;
                var arc = new Rectangle(rect.X, rect.Y, diameter, diameter);
                path.AddArc(arc, 180, 90);
                arc.X = rect.Right - diameter;
                path.AddArc(arc, 270, 90);
                arc.Y = rect.Bottom - diameter;
                path.AddArc(arc, 0, 90);
                arc.X = rect.Left;
                path.AddArc(arc, 90, 90);
                path.CloseFigure();
                return path;
            }

            private static Color BlendColor(Color from, Color to, float amount)
            {
                amount = Math.Max(0F, Math.Min(1F, amount));
                return Color.FromArgb(
                    from.A + (int)((to.A - from.A) * amount),
                    from.R + (int)((to.R - from.R) * amount),
                    from.G + (int)((to.G - from.G) * amount),
                    from.B + (int)((to.B - from.B) * amount));
            }
        }

        private void ShowSettingsPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowSettingsPage);
                return;
            }
            HidePages();
            _settingsPage.Visible = true;
            _settingsPage.BringToFront();
            SetStatus("Settings");
            UpdateNavigationState(_navSettings);
        }
    }
}


