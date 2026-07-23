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
        private readonly Dictionary<RuntimeProfileFeature, Button> _liveDrivingValueButtons =
            new Dictionary<RuntimeProfileFeature, Button>();
        private readonly Dictionary<RuntimeProfileFeature, StatusDotToggle> _liveDrivingToggleControls =
            new Dictionary<RuntimeProfileFeature, StatusDotToggle>();
        private bool _syncingLiveDrivingControls;

        private DataGridView BuildDrivingPage()
        {
            _drivingPage.AutoScroll = true;
            AddPageHeader(_drivingPage, "Driving Editor", "Tune grip, launch, bounce, and stability live while you drive. Use small changes first, then apply the rows you want.");

            var status = new Label();
            status.Text = "Ready";
            status.Font = new Font("Segoe UI Semibold", 9F);
            status.ForeColor = AccentBlue;
            status.BackColor = AppBackground;
            status.Location = new Point(710, 12);
            status.Size = new Size(190, 24);
            status.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            status.TextAlign = ContentAlignment.MiddleRight;
            _drivingPage.Controls.Add(status);
            _drivingPage.Tag = status;
            _drivingStatus = status;
            ResizePageHeader(_drivingPage, TwoColumnContentWidth, status);

            _drivingSelectedCar = null;
            _liveDrivingValueButtons.Clear();
            _liveDrivingToggleControls.Clear();

            var warning = new ModernPanel();
            warning.Location = new Point(0, 72);
            warning.Size = new Size(TwoColumnContentWidth, 50);
            warning.FillColor = Blend(SurfaceAlt, AccentRed, 0.12F);
            warning.BorderColor = Blend(Border, AccentRed, 0.46F);
            warning.CornerRadius = 12;
            warning.BackColor = AppBackground;
            _drivingPage.Controls.Add(warning);

            var warningText = new Label();
            SetUiText(warningText, "Many features need proper values to work correctly.");
            warningText.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            warningText.ForeColor = AccentRed;
            warningText.BackColor = Color.Transparent;
            warningText.Tag = "WarningLabel";
            warningText.Location = new Point(18, 12);
            warningText.Size = new Size(TwoColumnContentWidth - 36, 24);
            warningText.TextAlign = ContentAlignment.MiddleCenter;
            warning.Controls.Add(warningText);

            var grid = BuildLiveDrivingBackingGrid();
            _drivingPage.Controls.Add(grid);
            grid.SendToBack();
            foreach (var field in GetVisibleLiveDrivingFields())
            {
                var valueText = field.RequiresValue ? GetLiveDrivingDefaultDisplayValue(field) : "Ready";
                var rowIndex = grid.Rows.Add(field.DisplayName, field.Description, valueText, false);
                var row = grid.Rows[rowIndex];
                row.Tag = field;
                row.Cells[2].ReadOnly = !field.RequiresValue;
                row.Cells[0].ToolTipText = field.Description;
                row.Cells[1].ToolTipText = field.Description;
                row.Cells[2].ToolTipText = field.RequiresValue ? "Click this uncapped strength value, then turn the toggle green and press Apply Live Hooks. 100 is Luna's tuned default." : "Auto-check verifies the live signature after attach.";
                row.Cells[3].ToolTipText = "Green means on. Red means off. Click the toggle to switch.";
            }

            var actionsHeader = MakeDrivingSectionHeader("ACTIONS", 142);
            _drivingPage.Controls.Add(actionsHeader);
            var actions = MakeCard(_drivingPage, 0, 168, TwoColumnContentWidth, 84, string.Empty, string.Empty);
            actions.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            const int actionGap = 16;
            var actionGroupWidth = 180 + 150 + 128 + 128 + 128 + (actionGap * 4);
            var actionX = (TwoColumnContentWidth - actionGroupWidth) / 2;

            var apply = MakeButton("Apply Live Hooks", actionX, 23, 180, 38);
            MakeAccentButton(apply, AccentGreen);
            apply.Click += delegate
            {
                var inputs = CaptureLiveDrivingInputs(grid);
                RunWorker("Apply Live Driving Hooks", delegate { ApplyLiveDrivingInputs(inputs); }, apply);
            };
            actions.Controls.Add(apply);
            SetTranslatedToolTip(apply, "Applies every green-dot row through the live hook path.");

            actionX += 180 + actionGap;
            var stop = MakeButton("Turn All Off", actionX, 23, 150, 38);
            stop.Click += delegate { RunWorker("Turn Driving Hooks Off", delegate { TurnOffLiveDrivingHooks(grid); }, stop); };
            actions.Controls.Add(stop);
            SetTranslatedToolTip(stop, "Turns every live driving hook on this page off.");

            actionX += 150 + actionGap;
            var savePreset = MakeButton("Save Preset", actionX, 23, 128, 38);
            savePreset.Click += delegate { SaveLiveDrivingPreset(grid); };
            actions.Controls.Add(savePreset);
            SetTranslatedToolTip(savePreset, "Saves the current Driving Editor toggles and values.");

            actionX += 128 + actionGap;
            var loadPreset = MakeButton("Load Preset", actionX, 23, 128, 38);
            loadPreset.Click += delegate { LoadLiveDrivingPreset(grid); };
            actions.Controls.Add(loadPreset);
            SetTranslatedToolTip(loadPreset, "Loads saved toggles and values. Press Apply Live Hooks after loading.");

            actionX += 128 + actionGap;
            var premadePreset = MakeButton("Premade", actionX, 23, 128, 38);
            premadePreset.Click += delegate { ShowPremadeDrivingPresetPicker(grid); };
            actions.Controls.Add(premadePreset);
            SetTranslatedToolTip(premadePreset, "Choose a bundled Driving Editor preset, then press Apply Live Hooks.");

            var visibleFields = GetVisibleLiveDrivingFields().ToList();
            var leftY = 274;
            var rightY = 274;
            leftY = AddLiveDrivingCategory("GRIP & CORNERING", visibleFields.Where(field => GetLiveDrivingCategory(field.Feature) == "Grip").ToList(), 0, leftY, grid);
            rightY = AddLiveDrivingCategory("AIR & LANDING", visibleFields.Where(field => GetLiveDrivingCategory(field.Feature) == "Air").ToList(), TwoColumnWidth + TwoColumnGap, rightY, grid);
            leftY = AddLiveDrivingCategory("LAUNCH & FORCE", visibleFields.Where(field => GetLiveDrivingCategory(field.Feature) == "Launch").ToList(), 0, leftY, grid);
            rightY = AddLiveDrivingCategory("STABILITY & MOTION", visibleFields.Where(field => GetLiveDrivingCategory(field.Feature) == "Stability").ToList(), TwoColumnWidth + TwoColumnGap, rightY, grid);
            var sectionY = Math.Max(leftY, rightY);

            _drivingPage.AutoScrollMinSize = new Size(TwoColumnContentWidth, sectionY + 28);
            grid.CellValueChanged += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex >= 0)
                    SyncLiveDrivingRowControl(grid.Rows[e.RowIndex]);
            };
            return grid;
        }

        private DataGridView BuildLiveDrivingBackingGrid()
        {
            var grid = new DataGridView();
            grid.Visible = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.MultiSelect = false;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.EditMode = DataGridViewEditMode.EditOnEnter;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.BackgroundColor = Surface;
            grid.GridColor = Border;
            grid.BorderStyle = BorderStyle.None;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = SurfaceAlt;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = SurfaceAlt;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F);
            grid.DefaultCellStyle.BackColor = Surface;
            grid.DefaultCellStyle.ForeColor = TextPrimary;
            grid.DefaultCellStyle.SelectionBackColor = AccentBlueSoft;
            grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(28, 34, 46);
            grid.Font = new Font("Segoe UI", 9F);
            grid.RowTemplate.Height = 46;
            grid.ColumnHeadersHeight = 36;
            grid.ScrollBars = ScrollBars.Vertical;

            grid.Columns.Add("Setting", "Setting");
            grid.Columns.Add("Description", "What it does");
            grid.Columns.Add("Value", "Strength");
            var enabledColumn = new DataGridViewTextBoxColumn();
            enabledColumn.Name = "Enabled";
            enabledColumn.HeaderText = "Toggle";
            grid.Columns.Add(enabledColumn);
            grid.Columns[0].FillWeight = 22F;
            grid.Columns[1].FillWeight = 49F;
            grid.Columns[2].FillWeight = 15F;
            grid.Columns[3].FillWeight = 14F;
            grid.Columns[0].ReadOnly = true;
            grid.Columns[1].ReadOnly = true;
            grid.Columns[2].ReadOnly = false;
            grid.Columns[3].ReadOnly = true;
            return grid;
        }

        private Label MakeDrivingSectionHeader(string text, int y)
        {
            var header = new Label();
            SetUiText(header, text);
            header.UseMnemonic = false;
            header.Font = new Font("Segoe UI Semibold", 9F);
            header.ForeColor = TextMuted;
            header.BackColor = AppBackground;
            header.Location = new Point(6, y);
            header.Size = new Size(500, 18);
            return header;
        }

        private int AddLiveDrivingCategory(string title, List<LiveDrivingField> fields, int x, int y, DataGridView grid)
        {
            if (fields == null || fields.Count == 0)
                return y;

            var header = MakeDrivingSectionHeader(title, y);
            header.Left = x + 6;
            header.Width = TwoColumnWidth - 12;
            _drivingPage.Controls.Add(header);
            y += 26;
            const int rowHeight = 58;
            var cardHeight = (fields.Count * rowHeight) + 12;
            var card = MakeCard(_drivingPage, x, y, TwoColumnWidth, cardHeight, string.Empty, string.Empty);
            card.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            for (var index = 0; index < fields.Count; index++)
            {
                var field = fields[index];
                var row = grid.Rows.Cast<DataGridViewRow>().First(item =>
                {
                    var rowField = item.Tag as LiveDrivingField;
                    return rowField != null && rowField.Feature == field.Feature;
                });
                AddLiveDrivingCardRow(card, field, row, 6 + (index * rowHeight));
            }

            y += cardHeight + 18;
            return y;
        }

        private void AddLiveDrivingCardRow(Control parent, LiveDrivingField field, DataGridViewRow row, int y)
        {
            var name = MakeLabel(field.DisplayName, 18, y + 10);
            SetUiText(name, field.DisplayName);
            name.Size = new Size(150, 38);
            name.AutoSize = false;
            name.AutoEllipsis = true;
            name.TextAlign = ContentAlignment.MiddleLeft;
            parent.Controls.Add(name);
            SetTranslatedToolTip(name, field.Description);

            var description = MakeBodyLabel(field.Description, 184, y + 7, 330, 44);
            SetUiText(description, field.Description);
            description.BackColor = Surface;
            description.ForeColor = TextMuted;
            description.AutoEllipsis = true;
            description.TextAlign = ContentAlignment.MiddleLeft;
            parent.Controls.Add(description);
            SetTranslatedToolTip(description, field.Description);

            var valueText = Convert.ToString(row.Cells["Value"].Value, CultureInfo.InvariantCulture) ?? string.Empty;
            var value = MakeButton(valueText, 532, y + 13, 110, 32);
            value.Tag = field.RequiresValue ? "DrivingValue" : "DrivingStatus";
            if (field.RequiresValue)
            {
                value.Click += delegate
                {
                    var current = Convert.ToString(row.Cells["Value"].Value, CultureInfo.InvariantCulture) ?? GetLiveDrivingDefaultDisplayValue(field);
                    var selected = ShowLiveDrivingValuePrompt(field, current);
                    if (selected == null)
                        return;
                    row.Cells["Value"].Value = selected;
                    SyncLiveDrivingRowControl(row);
                };
                SetTranslatedToolTip(value, "Click to edit this strength. Values are uncapped.");
            }
            else
            {
                value.Cursor = Cursors.Default;
                value.TabStop = false;
                SetTranslatedToolTip(value, "Luna reports this hook's current availability here.");
            }
            parent.Controls.Add(value);
            _liveDrivingValueButtons[field.Feature] = value;

            var toggle = new StatusDotToggle();
            toggle.SetBounds(662, y + 18, 40, 22);
            toggle.Checked = row.Cells["Enabled"].Value is bool && (bool)row.Cells["Enabled"].Value;
            toggle.CheckedChanged += delegate
            {
                if (_syncingLiveDrivingControls)
                    return;
                row.Cells["Enabled"].Value = toggle.Checked;
            };
            parent.Controls.Add(toggle);
            _liveDrivingToggleControls[field.Feature] = toggle;
            SetTranslatedToolTip(toggle, "Green means on. Red means off.");

            if (y > 6)
            {
                var divider = new Panel();
                divider.Location = new Point(18, y - 1);
                divider.Size = new Size(TwoColumnWidth - 36, 1);
                divider.BackColor = Border;
                parent.Controls.Add(divider);
            }
        }

        private void SyncLiveDrivingRowControl(DataGridViewRow row)
        {
            if (row == null)
                return;
            var field = row.Tag as LiveDrivingField;
            if (field == null)
                return;

            _syncingLiveDrivingControls = true;
            try
            {
                Button value;
                if (_liveDrivingValueButtons.TryGetValue(field.Feature, out value) && value != null && !value.IsDisposed)
                {
                    var rawValue = Convert.ToString(row.Cells["Value"].Value, CultureInfo.InvariantCulture) ?? string.Empty;
                    value.Text = TranslateDynamicUi(rawValue);
                }

                StatusDotToggle toggle;
                if (_liveDrivingToggleControls.TryGetValue(field.Feature, out toggle) && toggle != null && !toggle.IsDisposed)
                {
                    var enabled = row.Cells["Enabled"].Value is bool && (bool)row.Cells["Enabled"].Value;
                    if (toggle.Checked != enabled)
                        toggle.Checked = enabled;
                }
            }
            finally
            {
                _syncingLiveDrivingControls = false;
            }
        }

        private static string GetLiveDrivingCategory(RuntimeProfileFeature feature)
        {
            switch (feature)
            {
                case RuntimeProfileFeature.NoWaterDrag:
                case RuntimeProfileFeature.SuperHandling:
                case RuntimeProfileFeature.SlideCalmer:
                case RuntimeProfileFeature.RoadMagnet:
                case RuntimeProfileFeature.CornerStabilizer:
                case RuntimeProfileFeature.TireBite:
                case RuntimeProfileFeature.GripLock:
                case RuntimeProfileFeature.ForwardGrip:
                case RuntimeProfileFeature.RailGrip:
                case RuntimeProfileFeature.CornerBite:
                    return "Grip";

                case RuntimeProfileFeature.LandingStabilizer:
                case RuntimeProfileFeature.AirLift:
                case RuntimeProfileFeature.BounceCushion:
                case RuntimeProfileFeature.VerticalTrim:
                case RuntimeProfileFeature.VerticalHold:
                case RuntimeProfileFeature.HoverGlide:
                case RuntimeProfileFeature.AirBrake:
                case RuntimeProfileFeature.GroundClamp:
                    return "Air";

                case RuntimeProfileFeature.SidePush:
                case RuntimeProfileFeature.ForwardPush:
                case RuntimeProfileFeature.WheelieBoost:
                case RuntimeProfileFeature.DriftKick:
                case RuntimeProfileFeature.PlantedBoost:
                case RuntimeProfileFeature.ForwardLaunch:
                case RuntimeProfileFeature.StraightLaunch:
                    return "Launch";

                default:
                    return "Stability";
            }
        }

        private void ProbeLiveDrivingHook()
        {
            var db = RequireDatabase();
            var fields = GetVisibleLiveDrivingFields().ToList();
            foreach (var field in fields)
                db.VerifyRuntimeFeatureHook(field.Feature);
            _liveDrivingHooksVerified = true;
            SetLiveDrivingSelectionText("Checked");
            foreach (var field in fields)
                SetLiveDrivingRowStatus(field.Feature, "Found");
            Log("Driving Editor live hook auto-check complete. " + string.Join(", ", fields.Select(field => field.DisplayName).ToArray()) + " are available on this FH6 build.");
        }

        private static IEnumerable<LiveDrivingField> GetVisibleLiveDrivingFields()
        {
            foreach (var field in LiveDrivingFields)
                yield return field;
        }

        private void TryAutoProbeLiveDrivingHooks()
        {
            if (_liveDrivingHooksVerified)
                return;
            try
            {
                ProbeLiveDrivingHook();
            }
            catch (Exception ex)
            {
                SetLiveDrivingSelectionText("Ready");
                Log("Driving Editor live hook auto-check skipped: " + ex.Message);
            }
        }

        private void LoadLiveDrivingDefaults()
        {
            if (_drivingGrid == null || _drivingGrid.IsDisposed)
                return;

            foreach (DataGridViewRow row in _drivingGrid.Rows)
            {
                var field = row.Tag as LiveDrivingField;
                if (field == null)
                    continue;

                if (field.Feature == RuntimeProfileFeature.Acceleration)
                    row.Cells["Value"].Value = FormatFloat(_accelerationToggleMultiplier);
                else if (field.Feature == RuntimeProfileFeature.Gravity)
                    row.Cells["Value"].Value = "100";
                else if (field.IsJump)
                    row.Cells["Value"].Value = GetJumpKeyText();
                else if (field.RequiresValue)
                    row.Cells["Value"].Value = GetLiveDrivingDefaultDisplayValue(field);
                else
                    row.Cells["Value"].Value = "Ready";
            }

            SetLiveDrivingSelectionText("Values loaded");
            SetDrivingStatus("Live values loaded", AccentGreen);
        }

        private List<LiveDrivingInput> CaptureLiveDrivingInputs(DataGridView grid)
        {
            var inputs = new List<LiveDrivingInput>();
            if (grid == null)
                return inputs;

            grid.EndEdit();
            foreach (DataGridViewRow row in grid.Rows)
            {
                var field = row.Tag as LiveDrivingField;
                if (field == null)
                    continue;

                var enabled = false;
                var raw = row.Cells["Enabled"].Value;
                if (raw is bool)
                    enabled = (bool)raw;
                else if (raw != null)
                    bool.TryParse(Convert.ToString(raw), out enabled);

                var value = Convert.ToString(row.Cells["Value"].Value, CultureInfo.InvariantCulture) ?? string.Empty;
                inputs.Add(new LiveDrivingInput(field, value, enabled));
            }

            return inputs;
        }

        private void ApplyLiveDrivingInputs(List<LiveDrivingInput> inputs)
        {
            if (inputs == null || inputs.Count == 0)
                throw new InvalidOperationException("No live driving rows were found.");

            foreach (var input in inputs)
            {
                if (input.Field.Feature == RuntimeProfileFeature.Jump)
                {
                    ApplyJumpRuntimeHook(input.Enabled);
                    continue;
                }

                var text = input.Field.RequiresValue ? input.RawValue : string.Empty;
                if (input.Field.Feature == RuntimeProfileFeature.Acceleration && input.Enabled)
                    _accelerationToggleMultiplier = ParsePositiveFloat(text, input.Field.DisplayName);

                ApplyProfileRuntimeHook(input.Field.Feature, input.Field.DisplayName, text, input.Enabled);
                SetLiveDrivingRowStatus(input.Field.Feature, input.Enabled ? "On" : "Off");
            }

            BeginInvoke((Action)(delegate
            {
                UpdateJumpKeyButton();
                UpdateAccelerationButtonText();
                SetLiveDrivingSelectionText("Applied");
            }));
        }

        private void TurnOffLiveDrivingHooks(DataGridView grid)
        {
            foreach (var field in LiveDrivingFields)
                ApplyProfileRuntimeHook(field.Feature, field.DisplayName, string.Empty, false);

            if (grid != null && !grid.IsDisposed)
            {
                BeginInvoke((Action)(delegate
                {
                    foreach (DataGridViewRow row in grid.Rows)
                    {
                        row.Cells["Enabled"].Value = false;
                        var field = row.Tag as LiveDrivingField;
                        if (field != null && !field.RequiresValue)
                            row.Cells["Value"].Value = "Off";
                    }
                    SetLiveDrivingSelectionText("Off");
                }));
            }
        }

        private void SaveLiveDrivingPreset(DataGridView grid)
        {
            try
            {
                var inputs = CaptureLiveDrivingInputs(grid);
                if (inputs.Count == 0)
                    throw new InvalidOperationException("No Driving Editor rows were found.");

                var presetDir = Path.Combine(_resultsDir, "driving_presets");
                Directory.CreateDirectory(presetDir);
                using (var dialog = new SaveFileDialog())
                {
                    dialog.Title = "Save Driving Editor preset";
                    dialog.InitialDirectory = presetDir;
                    dialog.Filter = "Luna driving preset (*.txt)|*.txt|All files (*.*)|*.*";
                    dialog.FileName = "driving_preset_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
                    PrepareDialogForLanguage(dialog);
                if (dialog.ShowDialog(this) != DialogResult.OK)
                        return;

                    var lines = new List<string>();
                    lines.Add("# Forza Horizon 6 Luna Driving Editor preset");
                    lines.Add("# FeatureId|Enabled|Base64Value");
                    foreach (var input in inputs)
                    {
                        var value = Convert.ToBase64String(Encoding.UTF8.GetBytes(input.RawValue ?? string.Empty));
                        lines.Add(((int)input.Field.Feature).ToString(CultureInfo.InvariantCulture) + "|" + (input.Enabled ? "1" : "0") + "|" + value);
                    }

                    File.WriteAllLines(dialog.FileName, lines, Encoding.UTF8);
                    Log("Driving Editor preset saved: " + dialog.FileName + ".");
                    SetLiveDrivingSelectionText("Preset saved");
                    SetDrivingStatus("Preset saved", AccentGreen);
                }
            }
            catch (Exception ex)
            {
                Log("Driving Editor preset save failed: " + ex.Message);
                ShowInfo(ex.Message);
            }
        }

        private void LoadLiveDrivingPreset(DataGridView grid)
        {
            try
            {
                if (grid == null)
                    throw new InvalidOperationException("Driving Editor table is not ready.");

                var presetDir = Path.Combine(_resultsDir, "driving_presets");
                Directory.CreateDirectory(presetDir);
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Title = "Load Driving Editor preset";
                    dialog.InitialDirectory = presetDir;
                    dialog.Filter = "Luna driving preset (*.txt)|*.txt|All files (*.*)|*.*";
                    PrepareDialogForLanguage(dialog);
                if (dialog.ShowDialog(this) != DialogResult.OK)
                        return;

                    LoadLiveDrivingPresetFile(grid, dialog.FileName);
                }
            }
            catch (Exception ex)
            {
                Log("Driving Editor preset load failed: " + ex.Message);
                ShowInfo(ex.Message);
            }
        }

        private void LoadLiveDrivingPresetFile(DataGridView grid, string fileName)
        {
            var preset = new Dictionary<RuntimeProfileFeature, LiveDrivingPresetRow>();
            foreach (var line in File.ReadAllLines(fileName, Encoding.UTF8))
            {
                var trimmed = (line ?? string.Empty).Trim();
                if (trimmed.Length == 0 || trimmed.StartsWith("#", StringComparison.Ordinal))
                    continue;

                var parts = trimmed.Split(new[] { '|' }, 3);
                if (parts.Length < 2)
                    continue;

                RuntimeProfileFeature feature;
                if (!TryParsePresetFeatureId(parts[0].Trim(), out feature))
                    continue;

                var enabledText = parts[1].Trim();
                var enabled = enabledText == "1" || enabledText.Equals("true", StringComparison.OrdinalIgnoreCase) || enabledText.Equals("on", StringComparison.OrdinalIgnoreCase);
                var value = string.Empty;
                if (parts.Length >= 3)
                {
                    try
                    {
                        value = Encoding.UTF8.GetString(Convert.FromBase64String(parts[2].Trim()));
                    }
                    catch
                    {
                        value = parts[2];
                    }
                }

                preset[feature] = new LiveDrivingPresetRow(enabled, value);
            }

            if (preset.Count == 0)
                throw new InvalidOperationException("That preset did not contain any Driving Editor rows.");

            var loaded = 0;
            foreach (DataGridViewRow row in grid.Rows)
            {
                var field = row.Tag as LiveDrivingField;
                if (field == null)
                    continue;

                LiveDrivingPresetRow saved;
                if (!preset.TryGetValue(field.Feature, out saved))
                    continue;

                row.Cells["Enabled"].Value = saved.Enabled;
                if (field.RequiresValue)
                    row.Cells["Value"].Value = string.IsNullOrWhiteSpace(saved.Value) ? GetLiveDrivingDefaultDisplayValue(field) : saved.Value;
                else
                    row.Cells["Value"].Value = saved.Enabled ? "Ready" : "Off";
                loaded++;
            }

            grid.Refresh();
            Log("Driving Editor preset loaded: " + Path.GetFileName(fileName) + " (" + loaded.ToString(CultureInfo.InvariantCulture) + " row(s)). Press Apply Live Hooks to use it.");
            SetLiveDrivingSelectionText("Preset loaded");
            SetDrivingStatus("Preset loaded", AccentGreen);
        }

        private string GetPremadeDrivingPresetDirectory()
        {
            var dir = Path.Combine(_resultsDir, "driving_presets", "premade");
            Directory.CreateDirectory(dir);
            return dir;
        }

        private void EnsureBundledPremadeDrivingPresets()
        {
            var dir = GetPremadeDrivingPresetDirectory();
            var bundled = GetBundledPremadeDrivingPresets();
            var keep = new HashSet<string>(bundled.Select(item => item.Value), StringComparer.OrdinalIgnoreCase);

            foreach (var file in Directory.GetFiles(dir, "*.txt"))
            {
                if (!keep.Contains(Path.GetFileName(file)))
                    File.Delete(file);
            }

            foreach (var item in bundled)
                ExtractBundledPremadeDrivingPreset(item.Key, item.Value);
        }

        private static KeyValuePair<string, string>[] GetBundledPremadeDrivingPresets()
        {
            return new[]
            {
                new KeyValuePair<string, string>("premade-driving-grip-stability-ken.txt", "Driving Editor - Girp and Stability - By Ken.txt"),
                new KeyValuePair<string, string>("premade-driving-instant-launch.txt", "Driving Editor - Instant Launch.txt"),
                new KeyValuePair<string, string>("premade-driving-chaos-preset-ken.txt", "Driving Editor Preset - Chaos Mode Preset - By Ken.txt"),
                new KeyValuePair<string, string>("premade-driving-chaos-mode-ken.txt", "Driving Editor - Chaos Mode Preset - By Ken.txt")
            };
        }

        private void ExtractBundledPremadeDrivingPreset(string resourceName, string fileName)
        {
            try
            {
                var dir = GetPremadeDrivingPresetDirectory();
                var target = Path.Combine(dir, fileName);

                using (var stream = typeof(Program).Assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        return;

                    var shouldWrite = !File.Exists(target);
                    if (!shouldWrite)
                    {
                        var existing = new FileInfo(target);
                        shouldWrite = existing.Length != stream.Length;
                    }

                    if (!shouldWrite)
                        return;

                    using (var output = File.Create(target))
                        stream.CopyTo(output);
                }
            }
            catch (Exception ex)
            {
                Log("Premade Driving Editor preset extract skipped: " + ex.Message);
            }
        }

        private void ShowPremadeDrivingPresetPicker(DataGridView grid)
        {
            try
            {
                if (grid == null)
                    throw new InvalidOperationException("Driving Editor table is not ready.");

                EnsureBundledPremadeDrivingPresets();
                var files = Directory.GetFiles(GetPremadeDrivingPresetDirectory(), "*.txt")
                    .OrderBy(file => Path.GetFileNameWithoutExtension(file), StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (files.Count == 0)
                {
                    ShowInfo("No premade Driving Editor presets were found yet.");
                    return;
                }

                using (var dialog = new Form())
                {
                    dialog.Text = "Premade Driving Editor Presets";
                    dialog.StartPosition = FormStartPosition.CenterParent;
                    dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                    dialog.MaximizeBox = false;
                    dialog.MinimizeBox = false;
                    dialog.ClientSize = new Size(520, 360);
                    dialog.BackColor = AppBackground;
                    dialog.ForeColor = TextPrimary;
                    dialog.Font = new Font("Segoe UI", 9F);
                    dialog.ShowInTaskbar = false;

                    var list = new ListBox();
                    list.Location = new Point(18, 18);
                    list.Size = new Size(484, 240);
                    list.BackColor = Surface;
                    list.ForeColor = TextPrimary;
                    list.BorderStyle = BorderStyle.FixedSingle;
                    foreach (var file in files)
                        list.Items.Add(Path.GetFileName(file));
                    list.SelectedIndex = 0;
                    dialog.Controls.Add(list);

                    var hint = MakeBodyLabel("Load the preset, review the green toggles and values, then press Apply Live Hooks.", 18, 274, 484, 28);
                    hint.BackColor = AppBackground;
                    hint.ForeColor = TextMuted;
                    dialog.Controls.Add(hint);

                    var load = MakeButton("Load Preset", 302, 316, 112, 30);
                    MakeAccentButton(load, AccentGreen);
                    load.Click += delegate
                    {
                        if (list.SelectedIndex < 0 || list.SelectedIndex >= files.Count)
                            return;
                        LoadLiveDrivingPresetFile(grid, files[list.SelectedIndex]);
                        dialog.DialogResult = DialogResult.OK;
                        dialog.Close();
                    };
                    dialog.Controls.Add(load);

                    var cancel = MakeButton("Cancel", 424, 316, 78, 30);
                    cancel.DialogResult = DialogResult.Cancel;
                    dialog.Controls.Add(cancel);
                    dialog.AcceptButton = load;
                    dialog.CancelButton = cancel;
                    PrepareDialogForLanguage(dialog);
                    dialog.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                Log("Premade Driving Editor preset load failed: " + ex.Message);
                ShowInfo(ex.Message);
            }
        }

        private static bool TryParsePresetFeatureId(string text, out RuntimeProfileFeature feature)
        {
            feature = default(RuntimeProfileFeature);
            if (string.IsNullOrWhiteSpace(text))
                return false;

            int numeric;
            if (int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out numeric) &&
                Enum.IsDefined(typeof(RuntimeProfileFeature), numeric))
            {
                feature = (RuntimeProfileFeature)numeric;
                return true;
            }

            try
            {
                feature = (RuntimeProfileFeature)Enum.Parse(typeof(RuntimeProfileFeature), text.Trim(), true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void UpdateLiveDrivingJumpRow()
        {
            if (_drivingGrid == null || _drivingGrid.IsDisposed)
                return;

            foreach (DataGridViewRow row in _drivingGrid.Rows)
            {
                var field = row.Tag as LiveDrivingField;
                if (field != null && field.Feature == RuntimeProfileFeature.Jump)
                    row.Cells["Value"].Value = GetJumpKeyText();
            }
        }

        private void SetLiveDrivingSelectionText(string text)
        {
            if (_drivingSelectedCar == null || _drivingSelectedCar.IsDisposed)
                return;
            if (_drivingSelectedCar.InvokeRequired)
            {
                _drivingSelectedCar.BeginInvoke((Action)(delegate { SetLiveDrivingSelectionText(text); }));
                return;
            }
            _drivingSelectedCar.Text = text;
        }

        private void SetLiveDrivingRowStatus(RuntimeProfileFeature feature, string status)
        {
            if (_drivingGrid == null || _drivingGrid.IsDisposed)
                return;
            if (InvokeRequired)
            {
                BeginInvoke((Action)(delegate { SetLiveDrivingRowStatus(feature, status); }));
                return;
            }

            foreach (DataGridViewRow row in _drivingGrid.Rows)
            {
                var field = row.Tag as LiveDrivingField;
                if (field != null && field.Feature == feature)
                {
                    if (field.RequiresValue)
                        row.Cells["Value"].ToolTipText = status;
                    else
                        row.Cells["Value"].Value = status;
                }
            }
        }

        private void ShowDrivingPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowDrivingPage);
                return;
            }
            HidePages();
            _drivingPage.Visible = true;
            _drivingPage.BringToFront();
            SetStatus("Driving Editor");
            SetProfileStatus("Driving Editor");
            SetDrivingStatus("Ready");
            UpdateNavigationState(_navDriving);
            if (_database != null && _database.IsAlive && !_liveDrivingHooksVerified)
                RunWorker("Auto Check Driving Hooks", TryAutoProbeLiveDrivingHooks);
        }
    }
}
