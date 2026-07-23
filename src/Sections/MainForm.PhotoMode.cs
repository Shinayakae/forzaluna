using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace ForzaHorizon6AutoshowUnlocker
{
    public sealed partial class MainForm
    {
        private readonly Dictionary<int, TextBox> _camFxBoxes = new Dictionary<int, TextBox>();
        private readonly Dictionary<int, string> _camFxDefaults = new Dictionary<int, string>();
        private Button _camFxFindButton;
        private Button _camFxApplyButton;
        private Button _camFxResetButton;

        private void EnsurePhotoModeAttached()
        {
            if (_database == null || !_database.IsAlive)
                throw new InvalidOperationException("Attach Luna to FH6 first.");
        }

        private void AddCameraEffectField(Control parent, string label, int index, string defaultText, int x, int y, string tip)
        {
            var lbl = MakeLabel(label, x, y + 4);
            lbl.Width = 78;
            lbl.AutoSize = false;
            parent.Controls.Add(lbl);
            SetTranslatedToolTip(lbl, tip);

            var box = new TextBox();
            box.Text = defaultText;
            box.Location = new Point(x + 82, y);
            box.Size = new Size(56, 26);
            box.TextAlign = HorizontalAlignment.Right;
            StyleTextBox(box);
            parent.Controls.Add(box);
            SetTranslatedToolTip(box, tip);
            _camFxBoxes[index] = box;
            _camFxDefaults[index] = defaultText;
        }

        private void FindCameraEffects()
        {
            RunWorker("Find Camera", delegate
            {
                EnsurePhotoModeAttached();
                var count = _database.ScanCameraEffectArrays();
                BeginInvoke((Action)(delegate
                {
                    SetPhotoModeStatus(count > 0
                        ? "Locked onto the camera effects. Type values and Apply."
                        : "Camera effects not found - try again from the normal driving view.", count > 0 ? AccentGreen : AccentRed);
                }));
            }, _camFxFindButton);
        }

        private void ApplyCameraEffects()
        {
            RunWorker("Camera Effects", delegate
            {
                EnsurePhotoModeAttached();
                var count = _database.EnsureCameraEffectScan();
                if (count == 0)
                {
                    BeginInvoke((Action)(delegate { SetPhotoModeStatus("Camera effects not found - click Find Camera from the normal view.", AccentRed); }));
                    return;
                }

                var pairs = (List<KeyValuePair<int, float>>)Invoke(new Func<List<KeyValuePair<int, float>>>(CaptureCameraEffectInputs));
                foreach (var pair in pairs)
                    _database.SetCameraEffect(pair.Key, true, pair.Value);
                BeginInvoke((Action)(delegate { SetPhotoModeStatus("Applied " + pairs.Count.ToString(CultureInfo.InvariantCulture) + " camera effect(s).", AccentGreen); }));
            }, _camFxApplyButton);
        }

        private List<KeyValuePair<int, float>> CaptureCameraEffectInputs()
        {
            var result = new List<KeyValuePair<int, float>>();
            foreach (var pair in _camFxBoxes)
            {
                float value;
                if (float.TryParse(pair.Value.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value) && !float.IsNaN(value) && !float.IsInfinity(value))
                    result.Add(new KeyValuePair<int, float>(pair.Key, value));
            }
            return result;
        }

        private void ResetCameraEffectControls()
        {
            RunWorker("Camera Effects Reset", delegate
            {
                EnsurePhotoModeAttached();
                _database.ResetCameraEffects();
                BeginInvoke((Action)(delegate
                {
                    foreach (var pair in _camFxBoxes)
                        pair.Value.Text = _camFxDefaults.ContainsKey(pair.Key) ? _camFxDefaults[pair.Key] : "0";
                    SetPhotoModeStatus("Camera effects reset to default.", AccentBlue);
                }));
            }, _camFxResetButton);
        }

        private void BuildPhotoModePage()
        {
            _photoModePage.AutoScroll = true;
            AddPageHeader(_photoModePage, "Photo Mode", "FH6 Photo Mode camera modifiers. Scan first, edit values, then apply selected rows.");

            _photoModeStatus = new Label();
            _photoModeStatus.Text = "Ready";
            _photoModeStatus.Font = new Font("Segoe UI Semibold", 9F);
            _photoModeStatus.ForeColor = AccentBlue;
            _photoModeStatus.BackColor = AppBackground;
            _photoModeStatus.Location = new Point(640, 12);
            _photoModeStatus.Size = new Size(260, 24);
            _photoModeStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            _photoModeStatus.TextAlign = ContentAlignment.MiddleRight;
            _photoModePage.Controls.Add(_photoModeStatus);
            ResizePageHeader(_photoModePage, TwoColumnContentWidth, _photoModeStatus);

            var actions = MakeCard(_photoModePage, 0, 72, TwoColumnWidth, 196, "Photo Mode Actions", "Open Photo Mode in-game, scan, then turn on the camera controls you want.");
            actions.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var panel = new Panel();
            panel.Location = new Point(18, 58);
            panel.Size = new Size(TwoColumnWidth - 36, 122);
            panel.BackColor = Surface;
            panel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            actions.Controls.Add(panel);

            _photoModeScanButton = MakeButton("Scan Photo Mode", 0, 0, 150, 38);
            MakeAccentButton(_photoModeScanButton, Color.FromArgb(0, 188, 212));
            _photoModeScanButton.Click += delegate { RunWorker("Photo Mode Scan", ScanPhotoMode, _photoModeScanButton); };
            panel.Controls.Add(_photoModeScanButton);
            SetTranslatedToolTip(_photoModeScanButton, "Finds FH6 Photo Mode modifier addresses and supported hook paths.");

            var apply = MakeButton("Apply Selected", 166, 0, 140, 38);
            MakeAccentButton(apply, AccentGreen);
            apply.Click += delegate { RunWorker("Photo Mode Apply", ApplyPhotoModeSelectedRows, apply); };
            panel.Controls.Add(apply);
            SetTranslatedToolTip(apply, "Applies only the selected green-dot Photo Mode modifier rows.");

            var restore = MakeButton("Restore Defaults", 322, 0, 142, 38);
            restore.Click += delegate { RunWorker("Photo Mode Restore", RestorePhotoModeDefaults, restore); };
            panel.Controls.Add(restore);
            SetTranslatedToolTip(restore, "Restores the Photo Mode modifier values Luna captured at scan time.");

            _photoModeHeightButton = MakeButton("Height OFF", 480, 0, 116, 38);
            _photoModeHeightButton.Click += delegate { TogglePhotoModeNoHeightLimit(); };
            panel.Controls.Add(_photoModeHeightButton);
            SetTranslatedToolTip(_photoModeHeightButton, "Photo Mode height control. When on, hold Shift to move the camera up and Ctrl to move it down.");

            _photoModeZoomButton = MakeButton("Zoom OFF", 0, 52, 110, 38);
            _photoModeZoomButton.Click += delegate { TogglePhotoModeIncreasedZoom(); };
            panel.Controls.Add(_photoModeZoomButton);
            SetTranslatedToolTip(_photoModeZoomButton, "Photo Mode zoom control. When on, use the mouse wheel to move the camera in or out.");

            _photoModeResetViewButton = MakeButton("Reset View", 126, 52, 120, 34);
            _photoModeResetViewButton.Click += delegate { ResetPhotoModeViewControls(); };
            panel.Controls.Add(_photoModeResetViewButton);
            SetTranslatedToolTip(_photoModeResetViewButton, "Resets Luna's Photo Mode zoom and height offsets back to zero.");

            var hint = MakeBodyLabel("Height uses Shift/Ctrl. Zoom uses the mouse wheel.", 264, 58, 410, 20);
            hint.BackColor = Surface;
            panel.Controls.Add(hint);

            var fxCard = MakeCard(_photoModePage, TwoColumnWidth + TwoColumnGap, 72, TwoColumnWidth, 196, "Camera Effects",
                "Live colour grading for your shots - saturation, contrast, brightness, exposure, sepia, blur and vignette. Click Find Camera (from normal view, before grading), type values, then Apply. Reset restores defaults.");
            fxCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var fxPanel = new Panel();
            fxPanel.Location = new Point(18, 56);
            fxPanel.Size = new Size(TwoColumnWidth - 36, 124);
            fxPanel.BackColor = Surface;
            fxPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            fxCard.Controls.Add(fxPanel);

            _camFxFindButton = MakeButton("Find Camera", 0, 0, 130, 34);
            MakeAccentButton(_camFxFindButton, Color.FromArgb(0, 188, 212));
            _camFxFindButton.Click += delegate { FindCameraEffects(); };
            fxPanel.Controls.Add(_camFxFindButton);
            SetTranslatedToolTip(_camFxFindButton, "Locks onto the camera's effect block. Do this from the normal view (before any grading), then type values and Apply.");

            _camFxApplyButton = MakeButton("Apply Effects", 142, 0, 140, 34);
            MakeAccentButton(_camFxApplyButton, AccentGreen);
            _camFxApplyButton.Click += delegate { ApplyCameraEffects(); };
            fxPanel.Controls.Add(_camFxApplyButton);
            SetTranslatedToolTip(_camFxApplyButton, "Applies every value you typed and holds them.");

            _camFxResetButton = MakeButton("Reset", 294, 0, 110, 34);
            _camFxResetButton.Click += delegate { ResetCameraEffectControls(); };
            fxPanel.Controls.Add(_camFxResetButton);
            SetTranslatedToolTip(_camFxResetButton, "Restores the camera back to its default look.");

            AddCameraEffectField(fxPanel, "Saturation", RemoteDatabase.CamFxSaturation, "1", 0, 48, "1 = normal, 0 = greyscale, 2 = vivid.");
            AddCameraEffectField(fxPanel, "Contrast", RemoteDatabase.CamFxContrast, "1", 168, 48, "1 = normal, higher = punchier.");
            AddCameraEffectField(fxPanel, "Brightness", RemoteDatabase.CamFxBrightness, "0", 336, 48, "0 = normal; try -0.2 to 0.2.");
            AddCameraEffectField(fxPanel, "Exposure", RemoteDatabase.CamFxExposure, "0", 504, 48, "0 = normal; +/- to brighten or darken.");
            AddCameraEffectField(fxPanel, "Sepia", RemoteDatabase.CamFxSepia, "0", 0, 88, "0 = off, 1 = full sepia tone.");
            AddCameraEffectField(fxPanel, "Blur", RemoteDatabase.CamFxBlur, "0", 168, 88, "0 = off, higher = soft full-frame blur.");
            AddCameraEffectField(fxPanel, "Vignette", RemoteDatabase.CamFxVignettePower, "1", 336, 88, "Vignette strength; 1 = default, higher = darker corners.");

            var table = MakeCard(_photoModePage, 0, 292, TwoColumnContentWidth, 642, "Photo Mode Modifiers", "Values write directly to FH6's live Photo Mode modifier table.");
            table.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            _photoModeGrid = BuildPhotoModeGrid();
            _photoModeGrid.Location = new Point(18, 56);
            _photoModeGrid.Size = new Size(TwoColumnContentWidth - 36, 568);
            table.Controls.Add(_photoModeGrid);

            _photoModePage.AutoScrollMinSize = new Size(TwoColumnContentWidth, 970);
        }

        private DataGridView BuildPhotoModeGrid()
        {
            var grid = new DataGridView();
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
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F);
            grid.ColumnHeadersHeight = 34;
            grid.DefaultCellStyle.BackColor = Surface;
            grid.DefaultCellStyle.ForeColor = TextPrimary;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(45, 92, 70);
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.AlternatingRowsDefaultCellStyle.BackColor = SurfaceAlt;
            grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Apply", HeaderText = "Apply", FillWeight = 8F });
            grid.Columns.Add("Setting", "Setting");
            grid.Columns.Add("Current", "Current");
            grid.Columns.Add("NewValue", "New Value");
            grid.Columns.Add("Default", "Default");
            grid.Columns.Add("Status", "Status");
            grid.Columns.Add("Description", "What it does");
            grid.Columns["Apply"].ReadOnly = false;
            grid.Columns["NewValue"].ReadOnly = false;
            foreach (DataGridViewColumn column in grid.Columns)
            {
                if (column.Name != "Apply" && column.Name != "NewValue")
                    column.ReadOnly = true;
            }
            grid.Columns["Setting"].FillWeight = 18F;
            grid.Columns["Current"].FillWeight = 12F;
            grid.Columns["NewValue"].FillWeight = 12F;
            grid.Columns["Default"].FillWeight = 12F;
            grid.Columns["Status"].FillWeight = 12F;
            grid.Columns["Description"].FillWeight = 34F;
            grid.CurrentCellDirtyStateChanged += delegate
            {
                if (grid.IsCurrentCellDirty)
                    grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            return grid;
        }

        private void ScanPhotoMode()
        {
            EnsurePhotoModeAttached();
            var rows = _database.ScanPhotoModeSettings();
            BeginInvoke((Action)(delegate
            {
                _photoModeRows = rows ?? new List<RemoteDatabase.PhotoModeSettingRow>();
                RefreshPhotoModeGrid();
                SetPhotoModeStatus("Photo Mode scan found " + _photoModeRows.Count.ToString(CultureInfo.InvariantCulture) + " modifier(s).", AccentGreen);
            }));
        }

        private void RefreshPhotoModeGrid()
        {
            if (_photoModeGrid == null)
                return;

            _photoModeGrid.Rows.Clear();
            foreach (var row in _photoModeRows)
            {
                var index = _photoModeGrid.Rows.Add(false, row.Name, row.CurrentText, row.NewValueText, row.DefaultText, row.Status, row.Description);
                _photoModeGrid.Rows[index].Tag = row;
                _photoModeGrid.Rows[index].Cells["Status"].Style.ForeColor = row.Found ? AccentGreen : AccentRed;
            }
        }

        private List<RemoteDatabase.PhotoModeSettingInput> CapturePhotoModeInputs()
        {
            var inputs = new List<RemoteDatabase.PhotoModeSettingInput>();
            if (_photoModeGrid == null)
                return inputs;

            foreach (DataGridViewRow gridRow in _photoModeGrid.Rows)
            {
                var row = gridRow.Tag as RemoteDatabase.PhotoModeSettingRow;
                if (row == null)
                    continue;

                var apply = false;
                var raw = gridRow.Cells["Apply"].Value;
                if (raw is bool)
                    apply = (bool)raw;
                if (!apply)
                    continue;

                var text = Convert.ToString(gridRow.Cells["NewValue"].Value, CultureInfo.InvariantCulture);
                inputs.Add(new RemoteDatabase.PhotoModeSettingInput(row.Key, text));
            }
            return inputs;
        }

        private void ApplyPhotoModeSelectedRows()
        {
            EnsurePhotoModeAttached();
            var inputs = (List<RemoteDatabase.PhotoModeSettingInput>)Invoke(new Func<List<RemoteDatabase.PhotoModeSettingInput>>(CapturePhotoModeInputs));
            var summary = _database.ApplyPhotoModeSettings(inputs);
            BeginInvoke((Action)(delegate
            {
                SetPhotoModeStatus(summary, AccentGreen);
                ScanPhotoMode();
            }));
        }

        private void RestorePhotoModeDefaults()
        {
            EnsurePhotoModeAttached();
            var summary = _database.RestorePhotoModeSettings();
            BeginInvoke((Action)(delegate
            {
                SetPhotoModeStatus(summary, AccentGreen);
                ScanPhotoMode();
            }));
        }

        private void TogglePhotoModeNoHeightLimit()
        {
            RunWorker("Photo Mode Height", delegate
            {
                EnsurePhotoModeAttached();
                var target = !_photoModeNoHeightLimitOn;
                _database.ApplyPhotoModeNoHeightLimit(target);
                _photoModeNoHeightLimitOn = target;
                BeginInvoke((Action)(delegate
                {
                    UpdatePhotoModeHookButtons();
                    SetPhotoModeStatus("Photo Mode Height " + (_photoModeNoHeightLimitOn ? "ON" : "OFF"), _photoModeNoHeightLimitOn ? AccentGreen : AccentRed);
                }));
            }, _photoModeHeightButton);
        }

        private void TogglePhotoModeIncreasedZoom()
        {
            RunWorker("Photo Mode Zoom", delegate
            {
                EnsurePhotoModeAttached();
                var target = !_photoModeIncreasedZoomOn;
                _database.ApplyPhotoModeIncreasedZoom(target);
                _photoModeIncreasedZoomOn = target;
                BeginInvoke((Action)(delegate
                {
                    UpdatePhotoModeHookButtons();
                    SetPhotoModeStatus("Photo Mode Zoom " + (_photoModeIncreasedZoomOn ? "ON" : "OFF"), _photoModeIncreasedZoomOn ? AccentGreen : AccentRed);
                }));
            }, _photoModeZoomButton);
        }

        private void UpdatePhotoModeHookButtons()
        {
            if (_photoModeHeightButton != null)
                _photoModeHeightButton.Text = TranslateDynamicUi(_photoModeNoHeightLimitOn ? "Height ON" : "Height OFF");
            if (_photoModeZoomButton != null)
                _photoModeZoomButton.Text = TranslateDynamicUi(_photoModeIncreasedZoomOn ? "Zoom ON" : "Zoom OFF");
        }

        private void ResetPhotoModeViewControls()
        {
            RunWorker("Photo Mode Reset View", delegate
            {
                EnsurePhotoModeAttached();
                _database.ResetPhotoModeViewOffsets();
                BeginInvoke((Action)(delegate
                {
                    SetPhotoModeStatus("Photo Mode view offsets reset.", AccentGreen);
                }));
            }, _photoModeResetViewButton);
        }

        private void SetPhotoModeStatus(string text, Color color)
        {
            if (_photoModeStatus == null)
                return;
            if (InvokeRequired)
            {
                BeginInvoke((Action)(delegate { SetPhotoModeStatus(text, color); }));
                return;
            }
            _photoModeStatus.Text = TranslateStatusText(text);
            _photoModeStatus.ForeColor = color;
        }

        private void ShowPhotoModePage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowPhotoModePage);
                return;
            }
            HidePages();
            _photoModePage.Visible = true;
            _photoModePage.BringToFront();
            SetStatus("Photo Mode");
            SetPhotoModeStatus("Ready", AccentBlue);
            UpdateNavigationState(_navPhotoMode);
        }
    }
}
