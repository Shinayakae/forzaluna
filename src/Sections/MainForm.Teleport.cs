using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ForzaHorizon6AutoshowUnlocker
{
    public sealed partial class MainForm
    {
        private void BuildTeleportPage()
        {
            _teleportPage.AutoScroll = true;
            AddPageHeader(_teleportPage, "Teleport", "Save locations, load custom lists, and teleport back with one key.");

            var status = new Label();
            status.Text = "Ready";
            status.Font = new Font("Segoe UI Semibold", 9F);
            status.ForeColor = AccentBlue;
            status.BackColor = AppBackground;
            status.Location = new Point(806, 12);
            status.Size = new Size(190, 24);
            status.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            status.TextAlign = ContentAlignment.MiddleRight;
            _teleportPage.Controls.Add(status);
            _teleportStatus = status;

            ResizePageHeader(_teleportPage, TwoColumnContentWidth, status);

            var content = new Panel();
            content.Location = new Point(0, 72);
            content.Size = new Size(TwoColumnContentWidth, 920);
            content.BackColor = AppBackground;
            content.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            _teleportPage.Controls.Add(content);

            BuildTeleportDrivingPanel(content);
            _teleportPage.AutoScrollMinSize = new Size(TwoColumnContentWidth, 1012);
        }

        private void BuildTeleportDrivingPanel(Control parent)
        {
            var hotkeyCard = MakeCard(parent, 0, 0, TwoColumnWidth, 150, "Saved Location Hotkey", "Set the saved-location key, save key, and arm the selected spot.");
            hotkeyCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var actionsCard = MakeCard(parent, TwoColumnWidth + TwoColumnGap, 0, TwoColumnWidth, 150, "Location Actions", "Save, update, teleport, or remove the selected saved spot.");
            actionsCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var listsCard = MakeCard(parent, 0, 178, TwoColumnWidth, 136, "Location Lists", "Save, load, or open a premade route without changing the selected hotkeys.");
            listsCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var playerCard = MakeCard(parent, TwoColumnWidth + TwoColumnGap, 178, TwoColumnWidth, 136, "List Player", "Run saved locations in order and loop until stopped.");
            playerCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var savedCard = MakeCard(parent, 0, 342, TwoColumnContentWidth, 540, "Saved Locations", "Select a row to use it. Double-click its name or press F2 to rename it.");
            savedCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var keyLabel = MakeLabel("Teleport key", 18, 66);
            keyLabel.Size = new Size(96, 24);
            keyLabel.TextAlign = ContentAlignment.MiddleLeft;
            hotkeyCard.Controls.Add(keyLabel);

            var key = MakeButton(GetTeleportKeyText(), 118, 60, 110, 32);
            key.Click += delegate
            {
                var selected = ShowKeybindPrompt("Set Teleport Key", "Press a key for saved-location teleport");
                if (!selected.HasValue)
                    return;

                _teleportVirtualKey = selected.Value;
                UpdateTeleportKeyButton();
                SaveAppSettings();
                SetDrivingStatus("Teleport key set to " + GetTeleportKeyText(), AccentPurple);
                Log("Teleport keybind set to " + GetTeleportKeyText() + ".");
                if (_teleportSavedToggle != null && _teleportSavedToggle.Checked)
                    RunWorker("Saved Teleport", delegate { ApplySavedLocationTeleportRuntimeHook(true); });
            };
            hotkeyCard.Controls.Add(key);
            SetTranslatedToolTip(key, "Default is T. This key is used for saved-location teleport.");
            _teleportSavedKeyButton = key;
            _teleportKeyButton = key;

            var saveKeyLabel = MakeLabel("Save key", 18, 108);
            saveKeyLabel.Size = new Size(70, 24);
            saveKeyLabel.TextAlign = ContentAlignment.MiddleLeft;
            hotkeyCard.Controls.Add(saveKeyLabel);

            var saveKey = MakeButton(GetTeleportSaveKeyText(), 118, 102, 110, 32);
            saveKey.Click += delegate
            {
                var selected = ShowKeybindPrompt("Set Save Location Key", "Press a key to save the current car location");
                if (!selected.HasValue)
                    return;

                _teleportSaveVirtualKey = selected.Value;
                UpdateTeleportSaveKeyButton();
                ReRegisterTeleportSaveHotkey();
                SaveAppSettings();
                SetDrivingStatus("Save key set to " + GetTeleportSaveKeyText(), AccentPurple);
                Log("Teleport save keybind set to " + GetTeleportSaveKeyText() + ".");
            };
            hotkeyCard.Controls.Add(saveKey);
            _teleportSaveKeyButton = saveKey;
            SetTranslatedToolTip(saveKey, "Default is F6. Press this while FH6 is focused to save your current car location.");

            var selectedFrame = new ModernPanel();
            selectedFrame.Location = new Point(390, 60);
            selectedFrame.Size = new Size(312, 36);
            selectedFrame.FillColor = SurfaceAlt;
            selectedFrame.BorderColor = Border;
            selectedFrame.CornerRadius = 10;
            selectedFrame.BackColor = Surface;
            hotkeyCard.Controls.Add(selectedFrame);

            _teleportSelectedLocation = new Label();
            _teleportSelectedLocation.Text = "Selected: none";
            _teleportSelectedLocation.Location = new Point(12, 2);
            _teleportSelectedLocation.Size = new Size(288, 32);
            _teleportSelectedLocation.BackColor = SurfaceAlt;
            _teleportSelectedLocation.ForeColor = TextMuted;
            _teleportSelectedLocation.Font = new Font("Segoe UI Semibold", 9F);
            _teleportSelectedLocation.TextAlign = ContentAlignment.MiddleLeft;
            _teleportSelectedLocation.AutoEllipsis = true;
            selectedFrame.Controls.Add(_teleportSelectedLocation);

            var savedToggle = new StatusDotToggle();
            savedToggle.Location = new Point(250, 65);
            savedToggle.Size = new Size(40, 22);
            hotkeyCard.Controls.Add(savedToggle);
            _teleportSavedToggle = savedToggle;
            SetTranslatedToolTip(savedToggle, "Green arms the selected saved spot. Red turns saved teleport off.");
            savedToggle.CheckedChanged += delegate
            {
                RequestFeatureOverlayRefresh();
                UpdateTeleportArmButtonState();
                if (savedToggle.Checked)
                {
                    RunWorker("Saved Teleport", delegate { ApplySavedLocationTeleportRuntimeHook(true); });
                    return;
                }
                if (_database == null || !_database.IsAlive)
                    return;

                RunWorker("Saved Teleport Off", delegate { ApplySavedLocationTeleportRuntimeHook(false); });
            };

            _teleportArmButton = null;

            var saveCurrent = MakeButton("Save Current", 18, 60, 130, 36);
            MakeAccentButton(saveCurrent, AccentBlue);
            saveCurrent.Click += delegate { RunWorker("Save Current Location", SaveCurrentTeleportLocation, saveCurrent); };
            actionsCard.Controls.Add(saveCurrent);
            SetTranslatedToolTip(saveCurrent, "Reads your car's current position and adds it to the table.");

            var saveWaypoint = MakeButton("Save Waypoint", 160, 60, 154, 36);
            MakeAccentButton(saveWaypoint, AccentPurple);
            saveWaypoint.Click += delegate { RunWorker("Save Waypoint Location", SaveWaypointTeleportLocation, saveWaypoint); };
            actionsCard.Controls.Add(saveWaypoint);
            SetTranslatedToolTip(saveWaypoint, "Saves the selected FH6 map waypoint into this saved-location list.");

            var overwrite = MakeButton("Overwrite", 326, 60, 112, 36);
            MakeAccentButton(overwrite, AccentPurple);
            overwrite.Click += delegate { RunWorker("Overwrite Teleport Location", OverwriteSelectedTeleportLocation, overwrite); };
            actionsCard.Controls.Add(overwrite);
            SetTranslatedToolTip(overwrite, "Replaces the selected saved spot with your current car location after confirmation.");

            var teleportNow = MakeButton("Teleport Now", 450, 60, 126, 36);
            teleportNow.Click += delegate { RunWorker("Teleport Now", TeleportSelectedLocationNow, teleportNow); };
            actionsCard.Controls.Add(teleportNow);
            SetTranslatedToolTip(teleportNow, "Immediately teleports to the selected saved spot.");

            var remove = MakeButton("Remove", 588, 60, 96, 36);
            remove.Click += delegate { RemoveSelectedTeleportLocation(); };
            actionsCard.Controls.Add(remove);
            SetTranslatedToolTip(remove, "Removes the selected saved spot from the table.");

            var saveList = MakeButton("Save List", 18, 60, 110, 36);
            saveList.Click += delegate { SaveTeleportLocationList(); };
            listsCard.Controls.Add(saveList);
            SetTranslatedToolTip(saveList, "Saves all locations in the table to a Luna teleport list file.");

            var loadList = MakeButton("Load List", 140, 60, 110, 36);
            loadList.Click += delegate { LoadTeleportLocationList(); };
            listsCard.Controls.Add(loadList);
            SetTranslatedToolTip(loadList, "Loads a Luna teleport list file into this table.");

            var premade = MakeButton("Premade", 262, 60, 110, 36);
            premade.Click += delegate { ShowPremadeTeleportListPicker(); };
            listsCard.Controls.Add(premade);
            SetTranslatedToolTip(premade, "Open premade teleport lists. Luna shows the file name and spot count before loading.");

            var listHint = MakeBodyLabel("Save, load, or open a premade route without changing the selected hotkeys.", 18, 102, TwoColumnWidth - 36, 24);
            listHint.BackColor = Surface;
            listHint.ForeColor = TextMuted;
            listHint.TextAlign = ContentAlignment.MiddleLeft;
            listsCard.Controls.Add(listHint);

            var playlistLabel = MakeLabel("Play key", 18, 66);
            playlistLabel.Size = new Size(60, 24);
            playlistLabel.TextAlign = ContentAlignment.MiddleLeft;
            playerCard.Controls.Add(playlistLabel);

            var playKey = MakeButton(GetTeleportPlaylistKeyText(), 84, 60, 90, 32);
            playKey.Click += delegate
            {
                var selected = ShowKeybindPrompt("Set Teleport Play Key", "Press a key to start or stop the saved-location list");
                if (!selected.HasValue)
                    return;

                _teleportPlaylistVirtualKey = selected.Value;
                UpdateTeleportPlaylistKeyButton();
                ReRegisterTeleportPlaylistHotkey();
                SaveAppSettings();
                SetDrivingStatus("Play key set to " + GetTeleportPlaylistKeyText(), AccentPurple);
                Log("Teleport playlist keybind set to " + GetTeleportPlaylistKeyText() + ".");
            };
            playerCard.Controls.Add(playKey);
            _teleportPlaylistKeyButton = playKey;
            SetTranslatedToolTip(playKey, "Default is F7. Press it while FH6 is focused to start or stop the saved-location list.");

            var secondsLabel = MakeLabel("Every", 190, 66);
            secondsLabel.Size = new Size(48, 24);
            secondsLabel.TextAlign = ContentAlignment.MiddleLeft;
            playerCard.Controls.Add(secondsLabel);

            _teleportPlaylistIntervalButton = MakeButton(FormatFloat(_teleportPlaylistSeconds), 244, 60, 78, 32);
            _teleportPlaylistIntervalButton.Click += delegate
            {
                var selected = ShowTeleportPlaylistSecondsPrompt(_teleportPlaylistSeconds);
                if (!selected.HasValue)
                    return;

                _teleportPlaylistSeconds = selected.Value;
                UpdateTeleportPlaylistIntervalButton();
                SaveAppSettings();
                SetDrivingStatus("List seconds set to " + FormatFloat(_teleportPlaylistSeconds), AccentPurple);
            };
            playerCard.Controls.Add(_teleportPlaylistIntervalButton);
            SetTranslatedToolTip(_teleportPlaylistIntervalButton, "Click to edit seconds between each saved spot. Default is 5.");

            var secText = MakeBodyLabel("seconds", 334, 66, 66, 24);
            secText.BackColor = Surface;
            secText.TextAlign = ContentAlignment.MiddleLeft;
            playerCard.Controls.Add(secText);

            var startList = MakeButton("Start List", 416, 58, 130, 36);
            MakeAccentButton(startList, AccentGreen);
            startList.Click += delegate { ToggleTeleportPlaylist(); };
            playerCard.Controls.Add(startList);
            _teleportPlaylistStartButton = startList;
            SetTranslatedToolTip(startList, "Runs through saved locations in order. Press again to stop.");

            var playlistHint = MakeBodyLabel("Runs saved locations in order and loops until stopped.", 18, 102, TwoColumnWidth - 36, 24);
            playlistHint.BackColor = Surface;
            playlistHint.ForeColor = TextMuted;
            playlistHint.TextAlign = ContentAlignment.MiddleLeft;
            playerCard.Controls.Add(playlistHint);

            var searchLabel = MakeLabel("Search", 18, 60);
            searchLabel.Size = new Size(58, 28);
            searchLabel.TextAlign = ContentAlignment.MiddleLeft;
            savedCard.Controls.Add(searchLabel);

            _teleportLocationSearchBox = new TextBox();
            _teleportLocationSearchBox.Location = new Point(82, 58);
            _teleportLocationSearchBox.Size = new Size(560, 28);
            StyleTextBox(_teleportLocationSearchBox);
            _teleportLocationSearchBox.TextChanged += delegate { RefreshTeleportLocationsGrid(); };
            savedCard.Controls.Add(_teleportLocationSearchBox);
            SetTranslatedToolTip(_teleportLocationSearchBox, "Filter saved spots by name or coordinates.");

            var searchHint = MakeBodyLabel("Filter by saved name, X, Y, or Z.", 660, 60, 420, 28);
            searchHint.BackColor = Surface;
            searchHint.ForeColor = TextMuted;
            searchHint.TextAlign = ContentAlignment.MiddleLeft;
            savedCard.Controls.Add(searchHint);

            _teleportLocationsGrid = BuildTeleportLocationsGrid();
            _teleportLocationsGrid.Location = new Point(18, 104);
            _teleportLocationsGrid.Size = new Size(TwoColumnContentWidth - 36, 410);
            savedCard.Controls.Add(_teleportLocationsGrid);
        }

        private DataGridView BuildTeleportLocationsGrid()
        {
            var grid = new DataGridView();
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.MultiSelect = false;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.EditMode = DataGridViewEditMode.EditProgrammatically;
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
            grid.DefaultCellStyle.SelectionBackColor = Blend(SurfaceAlt, AccentGreen, 0.34F);
            grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(28, 34, 46);
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = Blend(Color.FromArgb(28, 34, 46), AccentGreen, 0.34F);
            grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = TextPrimary;
            grid.Font = new Font("Segoe UI", 9F);
            grid.RowTemplate.Height = 34;
            grid.ColumnHeadersHeight = 34;
            var selectFont = new Font("Segoe UI Semibold", 9F);
            grid.Disposed += delegate { selectFont.Dispose(); };

            grid.Columns.Add("Select", "Select");
            grid.Columns.Add("Name", "Name");
            grid.Columns.Add("X", "X");
            grid.Columns.Add("Y", "Y");
            grid.Columns.Add("Z", "Z");
            grid.Columns["Select"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            grid.Columns["Select"].Width = 86;
            grid.Columns["Name"].FillWeight = 44F;
            grid.Columns["X"].FillWeight = 18F;
            grid.Columns["Y"].FillWeight = 18F;
            grid.Columns["Z"].FillWeight = 18F;
            grid.Columns["Select"].ReadOnly = true;
            grid.Columns["Name"].ReadOnly = false;
            grid.Columns["X"].ReadOnly = true;
            grid.Columns["Y"].ReadOnly = true;
            grid.Columns["Z"].ReadOnly = true;

            grid.CellClick += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex < 0)
                    return;

                grid.ClearSelection();
                grid.Rows[e.RowIndex].Selected = true;
                UpdateTeleportSelectedLocationLabel();
            };
            grid.CellDoubleClick += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex < 0 || e.ColumnIndex != grid.Columns["Name"].Index)
                    return;

                var rowIndex = e.RowIndex;
                grid.BeginInvoke((Action)(delegate
                {
                    if (grid.IsDisposed || rowIndex < 0 || rowIndex >= grid.Rows.Count)
                        return;

                    try
                    {
                        grid.CurrentCell = grid.Rows[rowIndex].Cells["Name"];
                        grid.BeginEdit(true);
                    }
                    catch (InvalidOperationException ex)
                    {
                        Log("Teleport name edit delayed: " + ex.Message);
                    }
                }));
            };
            grid.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode != Keys.F2 || grid.CurrentCell == null || grid.CurrentCell.OwningColumn.Name != "Name")
                    return;

                grid.BeginEdit(true);
                e.Handled = true;
            };
            grid.CellFormatting += delegate(object sender, DataGridViewCellFormattingEventArgs e)
            {
                if (e.RowIndex < 0 || e.ColumnIndex != grid.Columns["Select"].Index)
                    return;

                var current = grid.Rows[e.RowIndex].Selected || (grid.CurrentRow != null && grid.Rows[e.RowIndex] == grid.CurrentRow);
                e.Value = current ? "Selected" : "Select";
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                e.CellStyle.Font = selectFont;
                e.CellStyle.ForeColor = current ? AccentGreen : TextMuted;
                e.CellStyle.SelectionForeColor = current ? AccentGreen : TextPrimary;
                e.CellStyle.BackColor = current ? Blend(SurfaceAlt, AccentGreen, 0.18F) : Surface;
                e.CellStyle.SelectionBackColor = current ? Blend(SurfaceAlt, AccentGreen, 0.24F) : AccentBlueSoft;
                e.FormattingApplied = true;
            };
            grid.SelectionChanged += delegate
            {
                if (_teleportGridRefreshing)
                    return;

                grid.InvalidateColumn(grid.Columns["Select"].Index);
                UpdateTeleportSelectedLocationLabel();
            };
            grid.CellEndEdit += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex < 0 || e.ColumnIndex != grid.Columns["Name"].Index)
                    return;

                var row = grid.Rows[e.RowIndex].Tag as TeleportLocationRow;
                if (row == null)
                    return;

                row.Name = CleanTeleportLocationName(Convert.ToString(grid.Rows[e.RowIndex].Cells["Name"].Value, CultureInfo.InvariantCulture));
                grid.Rows[e.RowIndex].Cells["Name"].Value = row.Name;
                UpdateTeleportSelectedLocationLabel();
                AutoSaveTeleportLocationList();
            };

            return grid;
        }

        private void SaveCurrentTeleportLocation()
        {
            var db = RequireDatabase();
            var position = db.ReadCurrentVehicleTeleportPosition();
            var name = AddTeleportLocationFromAnyThread(position);
            Log("Saved teleport location " + name + " at " + FormatTeleportLocation(position) + ".");
            NotifyTeleportLocationSaved();
        }

        private void SaveWaypointTeleportLocation()
        {
            var db = RequireDatabase();
            var position = db.ReadCurrentWaypointTeleportPosition();
            var name = AddTeleportLocationFromAnyThread(position, GetNextTeleportLocationName("Waypoint"));
            Log("Saved teleport waypoint location " + name + " at " + FormatTeleportLocation(position) + ".");
            NotifyTeleportLocationSaved();
        }

        private void TriggerTeleportSaveHotkey()
        {
            if (System.Threading.Interlocked.Exchange(ref _teleportSaveHotkeyRunning, 1) == 1)
                return;

            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    SaveCurrentTeleportLocation();
                    SetDrivingStatus("Current location saved", AccentGreen);
                }
                catch (Exception ex)
                {
                    Log("ERROR: Teleport save key failed: " + ex.Message);
                    ShowWindowsNotification("Could not save current location.");
                }
                finally
                {
                    System.Threading.Interlocked.Exchange(ref _teleportSaveHotkeyRunning, 0);
                }
            });
        }

        private void TriggerTeleportPlaylistHotkey()
        {
            if (System.Threading.Interlocked.Exchange(ref _teleportPlaylistHotkeyRunning, 1) == 1)
                return;

            BeginInvoke((Action)(delegate
            {
                try
                {
                    ToggleTeleportPlaylist();
                }
                finally
                {
                    System.Threading.Interlocked.Exchange(ref _teleportPlaylistHotkeyRunning, 0);
                }
            }));
        }

        private void ToggleTeleportPlaylist()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ToggleTeleportPlaylist);
                return;
            }

            if (_teleportPlaylistRunning)
            {
                StopTeleportPlaylist(true);
                return;
            }

            try
            {
                StartTeleportPlaylist();
            }
            catch (Exception ex)
            {
                Log("Teleport playlist failed: " + ex.Message);
                ShowInfo(ex.Message);
            }
        }

        private void StartTeleportPlaylist()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)StartTeleportPlaylist);
                return;
            }

            if (_teleportLocations.Count == 0)
                throw new InvalidOperationException("Save or load at least one teleport location first.");

            RequireDatabase();
            _teleportPlaylistSeconds = ParseTeleportPlaylistSeconds();
            SaveAppSettings();

            var selectedRow = GetSelectedTeleportLocation();
            var selectedIndex = selectedRow == null ? -1 : _teleportLocations.IndexOf(selectedRow);
            _teleportPlaylistIndex = selectedIndex >= 0 && selectedIndex < _teleportLocations.Count ? selectedIndex : 0;

            EnsureTeleportPlaylistTimer();
            _teleportPlaylistTimer.Interval = Math.Max(250, (int)Math.Round(_teleportPlaylistSeconds * 1000F));
            _teleportPlaylistRunning = true;
            UpdateTeleportPlaylistButtonState();
            SetDrivingStatus("Teleport playlist running", AccentGreen);
            Log("Teleport playlist ON. Interval " + FormatFloat(_teleportPlaylistSeconds) + " second(s), key " + GetTeleportPlaylistKeyText() + ".");
            TeleportPlaylistStep();
            _teleportPlaylistTimer.Start();
        }

        private void StopTeleportPlaylist(bool log)
        {
            if (InvokeRequired && !IsDisposed)
            {
                BeginInvoke((Action)(delegate { StopTeleportPlaylist(log); }));
                return;
            }

            _teleportPlaylistRunning = false;
            if (_teleportPlaylistTimer != null)
                _teleportPlaylistTimer.Stop();
            UpdateTeleportPlaylistButtonState();
            if (log)
            {
                SetDrivingStatus("Teleport playlist stopped", AccentRed);
                Log("Teleport playlist OFF.");
            }
        }

        private void EnsureTeleportPlaylistTimer()
        {
            if (_teleportPlaylistTimer != null)
                return;

            _teleportPlaylistTimer = new System.Windows.Forms.Timer();
            _teleportPlaylistTimer.Interval = 5000;
            _teleportPlaylistTimer.Tick += delegate { TeleportPlaylistStep(); };
        }

        private void TeleportPlaylistStep()
        {
            if (!_teleportPlaylistRunning)
                return;

            if (_teleportLocations.Count == 0)
            {
                StopTeleportPlaylist(true);
                return;
            }

            if (_teleportPlaylistIndex < 0 || _teleportPlaylistIndex >= _teleportLocations.Count)
                _teleportPlaylistIndex = 0;

            var index = _teleportPlaylistIndex;
            var row = _teleportLocations[index];
            _teleportPlaylistIndex = (_teleportPlaylistIndex + 1) % _teleportLocations.Count;
            SelectTeleportLocationRow(index);

            var position = row.RawPosition.ToArray();
            var name = row.Name;
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    var db = _database;
                    if (db == null || !db.IsAlive)
                        throw new InvalidOperationException("Luna is not attached.");
                    db.TeleportToSavedLocationNow(position, name);
                    Log("Teleport playlist moved to " + name + " at " + FormatTeleportLocation(position) + ".");
                }
                catch (Exception ex)
                {
                    Log("ERROR: Teleport playlist stopped: " + ex.Message);
                    if (!IsDisposed)
                        BeginInvoke((Action)(delegate
                        {
                            StopTeleportPlaylist(false);
                            ShowInfo("Teleport playlist stopped: " + ex.Message);
                        }));
                }
            });
        }

        private float ParseTeleportPlaylistSeconds()
        {
            var text = _teleportPlaylistIntervalButton == null ? FormatFloat(_teleportPlaylistSeconds) : _teleportPlaylistIntervalButton.Text;
            var seconds = ParsePositiveFloat(text, "Teleport list seconds");
            if (seconds < 0.25F || seconds > 3600F)
                throw new InvalidOperationException("Teleport list seconds must be between 0.25 and 3600.");
            return seconds;
        }

        private void UpdateTeleportPlaylistIntervalButton()
        {
            if (_teleportPlaylistIntervalButton == null || _teleportPlaylistIntervalButton.IsDisposed)
                return;
            _teleportPlaylistIntervalButton.Text = FormatFloat(_teleportPlaylistSeconds);
        }

        private float? ShowTeleportPlaylistSecondsPrompt(float currentValue)
        {
            if (InvokeRequired)
                return (float?)Invoke(new Func<float, float?>(ShowTeleportPlaylistSecondsPrompt), currentValue);

            using (var dialog = new Form())
            {
                dialog.Text = "Teleport List Seconds";
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.ClientSize = new Size(520, 210);
                dialog.BackColor = AppBackground;
                dialog.ForeColor = TextPrimary;
                dialog.Font = new Font("Segoe UI", 9F);

                var title = new Label();
                title.Text = "List interval";
                title.Location = new Point(18, 16);
                title.Size = new Size(300, 24);
                title.Font = new Font("Segoe UI Semibold", 11F);
                title.ForeColor = TextPrimary;
                title.BackColor = AppBackground;
                dialog.Controls.Add(title);

                var helper = new Label();
                helper.Text = "Set how many seconds Luna waits before moving to the next saved spot.";
                helper.Location = new Point(18, 48);
                helper.Size = new Size(480, 38);
                helper.ForeColor = TextMuted;
                helper.BackColor = AppBackground;
                dialog.Controls.Add(helper);

                var valueLabel = new Label();
                valueLabel.Text = "Seconds";
                valueLabel.Location = new Point(18, 110);
                valueLabel.Size = new Size(110, 24);
                valueLabel.Font = new Font("Segoe UI Semibold", 9F);
                valueLabel.ForeColor = TextPrimary;
                valueLabel.BackColor = AppBackground;
                dialog.Controls.Add(valueLabel);

                var valueBox = new TextBox();
                valueBox.Text = FormatFloat(currentValue);
                valueBox.Location = new Point(142, 108);
                valueBox.Size = new Size(110, 28);
                valueBox.TextAlign = HorizontalAlignment.Right;
                StyleTextBox(valueBox);
                dialog.Controls.Add(valueBox);

                float? result = null;
                var apply = MakeButton("Use Value", 312, 162, 100, 30);
                MakeAccentButton(apply, AccentBlue);
                apply.Click += delegate
                {
                    try
                    {
                        var seconds = ParsePositiveFloat(valueBox.Text, "Teleport list seconds");
                        if (seconds < 0.25F || seconds > 3600F)
                            throw new InvalidOperationException("Teleport list seconds must be between 0.25 and 3600.");
                        result = seconds;
                        dialog.DialogResult = DialogResult.OK;
                        dialog.Close();
                    }
                    catch (Exception ex)
                    {
                        ShowTranslatedMessageBox(dialog, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                };
                dialog.Controls.Add(apply);

                var cancel = MakeButton("Cancel", 422, 162, 78, 30);
                cancel.DialogResult = DialogResult.Cancel;
                dialog.Controls.Add(cancel);

                dialog.AcceptButton = apply;
                dialog.CancelButton = cancel;
                PrepareDialogForLanguage(dialog);
                return dialog.ShowDialog(this) == DialogResult.OK ? result : null;
            }
        }

        private void UpdateTeleportPlaylistButtonState()
        {
            if (_teleportPlaylistStartButton == null || _teleportPlaylistStartButton.IsDisposed)
                return;

            _teleportPlaylistStartButton.Text = TranslateDynamicUi(_teleportPlaylistRunning ? "Stop List" : "Start List");
            MakeAccentButton(_teleportPlaylistStartButton, _teleportPlaylistRunning ? AccentRed : AccentGreen);
        }

        private void NotifyTeleportLocationSaved()
        {
            PlayTeleportSavedAudio();
            ShowWindowsNotification("Current Location Saved.");
        }

        private void NotifyTeleportLocationOverwritten()
        {
            PlayTeleportSavedAudio();
            ShowWindowsNotification("Teleport Location Overwritten.");
        }

        private void PlayTeleportSavedAudio()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    var path = EnsureTeleportSaveSoundFile();
                    if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                    {
                        Log("Teleport save audio file was not found.");
                        return;
                    }

                    string error;
                    if (Native.TryPlayMp3File(path, out error))
                        return;

                    if (!IsDisposed)
                        BeginInvoke((Action)(delegate { PlayTeleportSavedAudioWithWindowsMediaPlayer(path); }));
                }
                catch (Exception ex)
                {
                    Log("Teleport save audio failed: " + ex.Message);
                }
            });
        }

        private void PlayTeleportSavedAudioWithWindowsMediaPlayer(string path)
        {
            try
            {
                var type = Type.GetTypeFromProgID("WMPlayer.OCX");
                if (type == null)
                    throw new InvalidOperationException("Windows Media Player COM object was not available.");

                var player = Activator.CreateInstance(type);
                var settings = type.InvokeMember("settings", System.Reflection.BindingFlags.GetProperty, null, player, null);
                if (settings != null)
                    settings.GetType().InvokeMember("volume", System.Reflection.BindingFlags.SetProperty, null, settings, new object[] { 100 });

                type.InvokeMember("URL", System.Reflection.BindingFlags.SetProperty, null, player, new object[] { path });
                var controls = type.InvokeMember("controls", System.Reflection.BindingFlags.GetProperty, null, player, null);
                if (controls != null)
                    controls.GetType().InvokeMember("play", System.Reflection.BindingFlags.InvokeMethod, null, controls, null);

                _activeAudioPlayers.Add(player);
                var cleanup = new System.Windows.Forms.Timer();
                cleanup.Interval = 7000;
                cleanup.Tick += delegate
                {
                    cleanup.Stop();
                    cleanup.Dispose();
                    try
                    {
                        var activeControls = type.InvokeMember("controls", System.Reflection.BindingFlags.GetProperty, null, player, null);
                        if (activeControls != null)
                            activeControls.GetType().InvokeMember("stop", System.Reflection.BindingFlags.InvokeMethod, null, activeControls, null);
                    }
                    catch
                    {
                    }
                    try
                    {
                        type.InvokeMember("close", System.Reflection.BindingFlags.InvokeMethod, null, player, null);
                    }
                    catch
                    {
                    }
                    _activeAudioPlayers.Remove(player);
                    try
                    {
                        if (Marshal.IsComObject(player))
                            Marshal.FinalReleaseComObject(player);
                    }
                    catch
                    {
                    }
                };
                cleanup.Start();
            }
            catch (Exception ex)
            {
                Log("Teleport save audio fallback failed: " + ex.Message);
                try
                {
                    System.Media.SystemSounds.Asterisk.Play();
                }
                catch
                {
                }
            }
        }

        private string EnsureTeleportSaveSoundFile()
        {
            if (!string.IsNullOrWhiteSpace(_teleportSaveSoundPath) && File.Exists(_teleportSaveSoundPath))
                return _teleportSaveSoundPath;

            var path = Path.Combine(_resultsDir, "teleport-location-saved.mp3");
            if (!File.Exists(path))
            {
                using (var stream = typeof(Program).Assembly.GetManifestResourceStream("teleport-location-saved.mp3"))
                {
                    if (stream == null)
                        return string.Empty;
                    using (var file = File.Create(path))
                        stream.CopyTo(file);
                }
            }

            _teleportSaveSoundPath = path;
            return path;
        }

        private void ShowWindowsNotification(string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(delegate { ShowWindowsNotification(message); }));
                return;
            }

            EnsureNotifyIcon();
            if (_notifyIcon == null)
                return;

            _notifyIcon.BalloonTipTitle = Program.AppTitle;
            _notifyIcon.BalloonTipText = message ?? string.Empty;
            _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            _notifyIcon.ShowBalloonTip(2500);
            ShowNativeToastNotification(message);
            ShowInAppNotification(message);
        }

        private void ShowNativeToastNotification(string message)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    var script =
                        "$title=$env:LUNA_TOAST_TITLE;" +
                        "$msg=$env:LUNA_TOAST_MESSAGE;" +
                        "try{" +
                        "Add-Type -AssemblyName System.Security;" +
                        "$et=[System.Security.SecurityElement]::Escape($title);" +
                        "$em=[System.Security.SecurityElement]::Escape($msg);" +
                        "[Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType=WindowsRuntime] > $null;" +
                        "[Windows.Data.Xml.Dom.XmlDocument, Windows.Data.Xml.Dom.XmlDocument, ContentType=WindowsRuntime] > $null;" +
                        "$xml=New-Object Windows.Data.Xml.Dom.XmlDocument;" +
                        "$xml.LoadXml('<toast><visual><binding template=\"ToastGeneric\"><text>'+$et+'</text><text>'+$em+'</text></binding></visual></toast>');" +
                        "$toast=[Windows.UI.Notifications.ToastNotification]::new($xml);" +
                        "[Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier($title).Show($toast);" +
                        "}catch{}";

                    var psi = new ProcessStartInfo("powershell.exe");
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;
                    psi.WindowStyle = ProcessWindowStyle.Hidden;
                    psi.Arguments = "-NoProfile -ExecutionPolicy Bypass -EncodedCommand " + Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
                    psi.EnvironmentVariables["LUNA_TOAST_TITLE"] = Program.AppTitle;
                    psi.EnvironmentVariables["LUNA_TOAST_MESSAGE"] = message ?? string.Empty;
                    Process.Start(psi);
                }
                catch
                {
                }
            });
        }

        private void ShowInAppNotification(string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(delegate { ShowInAppNotification(message); }));
                return;
            }

            try
            {
                var popup = new Form();
                popup.FormBorderStyle = FormBorderStyle.None;
                popup.ShowInTaskbar = false;
                popup.StartPosition = FormStartPosition.Manual;
                popup.TopMost = true;
                popup.BackColor = Surface;
                popup.Size = new Size(320, 86);
                popup.Opacity = 0.96;

                var area = GetNotificationWorkingArea();
                popup.Location = new Point(area.Right - popup.Width - 22, area.Bottom - popup.Height - 22);

                var panel = new ModernPanel();
                panel.Dock = DockStyle.Fill;
                panel.FillColor = Surface;
                panel.BorderColor = AccentGreen;
                panel.CornerRadius = 12;
                popup.Controls.Add(panel);

                var title = new Label();
                title.Text = Program.AppTitle;
                title.Location = new Point(18, 12);
                title.Size = new Size(284, 22);
                title.BackColor = Surface;
                title.ForeColor = TextPrimary;
                title.Font = new Font("Segoe UI Semibold", 10F);
                panel.Controls.Add(title);

                var body = new Label();
                body.Text = message ?? string.Empty;
                body.Location = new Point(18, 40);
                body.Size = new Size(284, 28);
                body.BackColor = Surface;
                body.ForeColor = AccentGreen;
                body.Font = new Font("Segoe UI Semibold", 10F);
                panel.Controls.Add(body);

                var timer = new System.Windows.Forms.Timer();
                timer.Interval = 2600;
                timer.Tick += delegate
                {
                    timer.Stop();
                    timer.Dispose();
                    popup.Close();
                    popup.Dispose();
                };
                popup.Shown += delegate { timer.Start(); };
                popup.Show();
            }
            catch
            {
            }
        }

        private Rectangle GetNotificationWorkingArea()
        {
            try
            {
                foreach (var processName in new[] { "forzahorizon6", "ForzaHorizon6" })
                {
                    foreach (var process in Process.GetProcessesByName(processName))
                    {
                        try
                        {
                            if (process.MainWindowHandle != IntPtr.Zero)
                                return Screen.FromHandle(process.MainWindowHandle).WorkingArea;
                        }
                        finally
                        {
                            process.Dispose();
                        }
                    }
                }
            }
            catch
            {
            }

            try
            {
                if (Screen.PrimaryScreen != null)
                    return Screen.PrimaryScreen.WorkingArea;
            }
            catch
            {
            }

            return Screen.FromControl(this).WorkingArea;
        }

        private string AddTeleportLocationFromAnyThread(byte[] position)
        {
            return AddTeleportLocationFromAnyThread(position, null);
        }

        private string AddTeleportLocationFromAnyThread(byte[] position, string preferredName)
        {
            if (InvokeRequired)
                return (string)Invoke(new Func<byte[], string, string>(AddTeleportLocationFromAnyThread), position, preferredName);

            var name = string.IsNullOrWhiteSpace(preferredName)
                ? GetNextTeleportLocationName()
                : CleanTeleportLocationName(preferredName);
            var row = new TeleportLocationRow(name, position);
            _teleportLocations.Add(row);
            RefreshTeleportLocationsGrid();
            SelectTeleportLocationRow(_teleportLocations.Count - 1);
            AutoSaveTeleportLocationList();
            SetDrivingStatus("Location saved", AccentGreen);
            return name;
        }

        private void RefreshTeleportLocationsGrid()
        {
            if (_teleportLocationsGrid == null || _teleportLocationsGrid.IsDisposed)
                return;
            if (_teleportLocationsGrid.InvokeRequired)
            {
                _teleportLocationsGrid.BeginInvoke((Action)RefreshTeleportLocationsGrid);
                return;
            }

            var selected = GetSelectedTeleportLocation();
            var filter = GetTeleportLocationSearchText();
            _teleportGridRefreshing = true;
            _teleportLocationsGrid.SuspendLayout();
            try
            {
                if (_teleportLocationsGrid.IsCurrentCellInEditMode)
                    _teleportLocationsGrid.EndEdit();

                _teleportLocationsGrid.Rows.Clear();
                foreach (var row in _teleportLocations)
                {
                    if (!TeleportLocationMatchesSearch(row, filter))
                        continue;

                    var index = _teleportLocationsGrid.Rows.Add(string.Empty, row.Name, FormatTeleportCoordinate(row.X), FormatTeleportCoordinate(row.Y), FormatTeleportCoordinate(row.Z));
                    _teleportLocationsGrid.Rows[index].Tag = row;
                }
            }
            finally
            {
                _teleportLocationsGrid.ResumeLayout();
                _teleportGridRefreshing = false;
            }
            if (selected != null)
                SelectTeleportLocationRow(selected);
            UpdateTeleportSelectedLocationLabel();
        }

        private void SelectTeleportLocationRow(int index)
        {
            if (index < 0 || index >= _teleportLocations.Count)
                return;

            SelectTeleportLocationRow(_teleportLocations[index]);
        }

        private void SelectTeleportLocationRow(TeleportLocationRow location)
        {
            if (_teleportLocationsGrid == null || _teleportLocationsGrid.IsDisposed || location == null)
                return;
            if (_teleportLocationsGrid.InvokeRequired)
            {
                _teleportLocationsGrid.BeginInvoke((Action)(delegate { SelectTeleportLocationRow(location); }));
                return;
            }

            _teleportGridRefreshing = true;
            _teleportLocationsGrid.ClearSelection();
            try
            {
                foreach (DataGridViewRow gridRow in _teleportLocationsGrid.Rows)
                {
                    if (!object.ReferenceEquals(gridRow.Tag, location))
                        continue;

                    gridRow.Selected = true;
                    if (gridRow.Index >= 0)
                        _teleportLocationsGrid.FirstDisplayedScrollingRowIndex = gridRow.Index;
                    break;
                }
            }
            finally
            {
                _teleportGridRefreshing = false;
            }
            _teleportLocationsGrid.InvalidateColumn(_teleportLocationsGrid.Columns["Select"].Index);
            UpdateTeleportSelectedLocationLabel();
        }

        private string GetTeleportLocationSearchText()
        {
            if (_teleportLocationSearchBox == null || _teleportLocationSearchBox.IsDisposed)
                return string.Empty;
            return (_teleportLocationSearchBox.Text ?? string.Empty).Trim();
        }

        private static bool TeleportLocationMatchesSearch(TeleportLocationRow row, string filter)
        {
            if (row == null)
                return false;
            if (string.IsNullOrWhiteSpace(filter))
                return true;

            return (row.Name ?? string.Empty).IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   FormatTeleportCoordinate(row.X).IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   FormatTeleportCoordinate(row.Y).IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   FormatTeleportCoordinate(row.Z).IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   FormatTeleportLocation(row.RawPosition).IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void UpdateTeleportSelectedLocationLabel()
        {
            if (_teleportSelectedLocation == null || _teleportSelectedLocation.IsDisposed)
                return;
            if (_teleportSelectedLocation.InvokeRequired)
            {
                _teleportSelectedLocation.BeginInvoke((Action)UpdateTeleportSelectedLocationLabel);
                return;
            }

            var selected = GetSelectedTeleportLocation();
            _teleportSelectedLocation.Text = selected == null
                ? TranslateDynamicUi("Selected: none")
                : TranslateDynamicUi("Selected:") + " " + selected.Name + "  (" + FormatTeleportLocation(selected.RawPosition) + ")";
            UpdateTeleportArmButtonState();
            RefreshArmedSavedTeleportTarget(selected);
        }

        private void UpdateTeleportArmButtonState()
        {
            if (_teleportArmButton == null || _teleportArmButton.IsDisposed)
                return;
            if (_teleportArmButton.InvokeRequired)
            {
                _teleportArmButton.BeginInvoke((Action)UpdateTeleportArmButtonState);
                return;
            }

            var armed = _teleportSavedToggle != null && _teleportSavedToggle.Checked;
            _teleportArmButton.Text = TranslateDynamicUi(armed ? "Hotkey ON" : "Hotkey OFF");
            MakeAccentButton(_teleportArmButton, armed ? AccentGreen : AccentRed);
            var selected = GetSelectedTeleportLocation();
            SetTranslatedToolTip(
                _teleportArmButton,
                armed
                    ? (selected == null ? "Hotkey is ON. Save or select a spot, then press " + GetTeleportKeyText() + "." : "Hotkey is ON for " + selected.Name + ". Press " + GetTeleportKeyText() + " in game.")
                    : "Hotkey is OFF. Click this to arm saved-location teleport.");
        }

        private void RefreshArmedSavedTeleportTarget(TeleportLocationRow selected)
        {
            if (selected == null || _teleportSavedToggle == null || !_teleportSavedToggle.Checked)
                return;
            if (_database == null || !_database.IsAlive)
                return;

            var position = selected.RawPosition.ToArray();
            var name = selected.Name;
            var key = _teleportVirtualKey;
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    _database.SetSavedLocationTeleportRuntimeHook(true, key, position, name);
                    Log("Saved Location Teleport target set to " + name + ".");
                }
                catch (Exception ex)
                {
                    Log("ERROR: Saved Location Teleport target update failed: " + ex.Message);
                }
            });
        }

        private TeleportLocationRow GetSelectedTeleportLocation()
        {
            if (_teleportLocationsGrid == null || _teleportLocationsGrid.IsDisposed)
                return null;
            if (_teleportLocationsGrid.InvokeRequired)
                return (TeleportLocationRow)_teleportLocationsGrid.Invoke(new Func<TeleportLocationRow>(GetSelectedTeleportLocation));

            if (_teleportLocationsGrid.SelectedRows.Count > 0)
                return _teleportLocationsGrid.SelectedRows[0].Tag as TeleportLocationRow;
            if (_teleportLocationsGrid.CurrentRow != null)
                return _teleportLocationsGrid.CurrentRow.Tag as TeleportLocationRow;
            return null;
        }

        private void ApplySavedLocationTeleportRuntimeHook(bool enabled)
        {
            if (!enabled)
            {
                var offDb = RequireDatabase();
                offDb.SetSavedLocationTeleportRuntimeHook(false, _teleportVirtualKey, null, string.Empty);
                Log("Saved Location Teleport runtime hook OFF.");
                return;
            }

            var selected = GetSelectedTeleportLocation();
            if (selected == null)
            {
                Log("Saved Location Teleport armed. Save or select a location, then press " + GetTeleportKeyText() + ".");
                return;
            }

            var onDb = RequireDatabase();
            onDb.SetSavedLocationTeleportRuntimeHook(true, _teleportVirtualKey, selected.RawPosition, selected.Name);
            Log("Saved Location Teleport armed for " + selected.Name + " with key " + GetTeleportKeyText() + ".");
        }

        private void TeleportSelectedLocationNow()
        {
            var selected = GetSelectedTeleportLocation();
            if (selected == null)
                throw new InvalidOperationException("Select a saved teleport location first.");

            var db = RequireDatabase();
            db.TeleportToSavedLocationNow(selected.RawPosition, selected.Name);
            Log("Teleported to saved location " + selected.Name + " at " + FormatTeleportLocation(selected.RawPosition) + ".");
        }

        private void OverwriteSelectedTeleportLocation()
        {
            var selected = GetSelectedTeleportLocation();
            if (selected == null)
                throw new InvalidOperationException("Select a saved teleport location first.");

            if (!ConfirmOverwriteTeleportLocation(selected))
                return;

            var db = RequireDatabase();
            var position = db.ReadCurrentVehicleTeleportPosition();
            Array.Clear(selected.RawPosition, 0, selected.RawPosition.Length);
            if (position != null)
                Buffer.BlockCopy(position, 0, selected.RawPosition, 0, Math.Min(selected.RawPosition.Length, position.Length));

            AutoSaveTeleportLocationList();
            RefreshTeleportLocationsGrid();
            SelectTeleportLocationRow(selected);
            SetDrivingStatus("Location overwritten", AccentGreen);
            Log("Overwrote saved teleport location " + selected.Name + " with " + FormatTeleportLocation(selected.RawPosition) + ".");
            NotifyTeleportLocationOverwritten();
        }

        private bool ConfirmOverwriteTeleportLocation(TeleportLocationRow selected)
        {
            if (selected == null)
                return false;

            var message = "You are about to overwrite this saved teleport location:" +
                          Environment.NewLine + Environment.NewLine +
                          selected.Name +
                          Environment.NewLine +
                          FormatTeleportLocation(selected.RawPosition) +
                          Environment.NewLine + Environment.NewLine +
                          "The selected row will be replaced with your current car location.";
            return ShowProceedDialog("Overwrite Teleport Location", message);
        }

        private void RemoveSelectedTeleportLocation()
        {
            var selected = GetSelectedTeleportLocation();
            if (selected == null)
                return;

            var removedIndex = _teleportLocations.IndexOf(selected);
            _teleportLocations.Remove(selected);
            RefreshTeleportLocationsGrid();
            if (_teleportLocations.Count > 0)
                SelectTeleportLocationRow(Math.Max(0, Math.Min(removedIndex, _teleportLocations.Count - 1)));
            else if (_teleportSavedToggle != null && _teleportSavedToggle.Checked)
                _teleportSavedToggle.Checked = false;
            AutoSaveTeleportLocationList();
            SetDrivingStatus("Location removed", AccentRed);
            Log("Removed saved teleport location " + selected.Name + ".");
        }

        private void SaveTeleportLocationList()
        {
            try
            {
                if (_teleportLocations.Count == 0)
                    throw new InvalidOperationException("Save at least one location first.");

                var listDir = GetTeleportListDirectory();
                using (var dialog = new SaveFileDialog())
                {
                    dialog.Title = "Save Teleport Location List";
                    dialog.InitialDirectory = listDir;
                    dialog.Filter = "Luna teleport list (*.txt)|*.txt|All files (*.*)|*.*";
                    dialog.FileName = "teleport_list_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
                    PrepareDialogForLanguage(dialog);
                if (dialog.ShowDialog(this) != DialogResult.OK)
                        return;

                    WriteTeleportLocationListFile(dialog.FileName, false);
                    _teleportLocationListPath = dialog.FileName;
                    SetDrivingStatus("Teleport list saved", AccentGreen);
                    Log("Teleport list saved: " + dialog.FileName + ".");
                }
            }
            catch (Exception ex)
            {
                Log("Teleport list save failed: " + ex.Message);
                ShowInfo(ex.Message);
            }
        }

        private void LoadTeleportLocationList()
        {
            try
            {
                var listDir = GetTeleportListDirectory();
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Title = "Load Teleport Location List";
                    dialog.InitialDirectory = listDir;
                    dialog.Filter = "Luna teleport list (*.txt)|*.txt|All files (*.*)|*.*";
                    PrepareDialogForLanguage(dialog);
                if (dialog.ShowDialog(this) != DialogResult.OK)
                        return;

                    var loaded = ReadTeleportLocationListFile(dialog.FileName);

                    if (loaded.Count == 0)
                        throw new InvalidOperationException("That list did not contain any saved teleport locations.");

                    _teleportLocations = loaded;
                    _teleportLocationListPath = dialog.FileName;
                    RefreshTeleportLocationsGrid();
                    SelectTeleportLocationRow(0);
                    SetDrivingStatus("Teleport list loaded", AccentGreen);
                    Log("Teleport list loaded: " + Path.GetFileName(dialog.FileName) + " (" + loaded.Count.ToString(CultureInfo.InvariantCulture) + " spot(s)).");
                }
            }
            catch (Exception ex)
            {
                Log("Teleport list load failed: " + ex.Message);
                ShowInfo(ex.Message);
            }
        }

        private string GetTeleportListDirectory()
        {
            var listDir = Path.Combine(_resultsDir, "teleport_lists");
            Directory.CreateDirectory(listDir);
            return listDir;
        }

        private string GetDefaultTeleportAutoSaveListPath()
        {
            return Path.Combine(GetTeleportListDirectory(), "Luna Auto Saved Teleports.txt");
        }

        private string GetTeleportAutoSaveTargetPath()
        {
            if (!string.IsNullOrWhiteSpace(_teleportLocationListPath) && !IsPremadeTeleportListPath(_teleportLocationListPath))
                return _teleportLocationListPath;
            return GetDefaultTeleportAutoSaveListPath();
        }

        private bool IsPremadeTeleportListPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                var premade = Path.GetFullPath(GetPremadeTeleportListDirectory()).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
                var full = Path.GetFullPath(path);
                return full.StartsWith(premade, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private void AutoSaveTeleportLocationList()
        {
            try
            {
                var target = GetTeleportAutoSaveTargetPath();
                WriteTeleportLocationListFile(target, true);
                _teleportLocationListPath = target;
                Log("Teleport list auto-saved: " + Path.GetFileName(target) + ".");
            }
            catch (Exception ex)
            {
                Log("Teleport list auto-save failed: " + ex.Message);
            }
        }

        private void WriteTeleportLocationListFile(string fileName, bool allowEmpty)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new InvalidOperationException("Teleport list file path is not valid.");
            if (!allowEmpty && _teleportLocations.Count == 0)
                throw new InvalidOperationException("Save at least one location first.");

            var directory = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            var lines = new List<string>();
            lines.Add("# Forza Horizon 6 Luna teleport list");
            lines.Add("# NameBase64|PositionBase64");
            foreach (var row in _teleportLocations)
                lines.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(row.Name ?? string.Empty)) + "|" + Convert.ToBase64String(row.RawPosition));

            File.WriteAllLines(fileName, lines, Encoding.UTF8);
        }

        private List<TeleportLocationRow> ReadTeleportLocationListFile(string fileName)
        {
            var loaded = new List<TeleportLocationRow>();
            foreach (var line in File.ReadAllLines(fileName, Encoding.UTF8))
            {
                var trimmed = (line ?? string.Empty).Trim();
                if (trimmed.Length == 0 || trimmed.StartsWith("#", StringComparison.Ordinal))
                    continue;

                var parts = trimmed.Split('|');
                if (parts.Length < 2)
                    continue;

                try
                {
                    var name = Encoding.UTF8.GetString(Convert.FromBase64String(parts[0].Trim()));
                    var position = Convert.FromBase64String(parts[1].Trim());
                    if (position != null && position.Length >= 16)
                        loaded.Add(new TeleportLocationRow(CleanTeleportLocationName(name), position));
                }
                catch
                {
                }
            }

            return loaded;
        }

        private string GetPremadeTeleportListDirectory()
        {
            var dir = Path.Combine(_resultsDir, "teleport_lists", "premade");
            Directory.CreateDirectory(dir);
            return dir;
        }

        private void EnsureBundledPremadeTeleportLists()
        {
            var dir = GetPremadeTeleportListDirectory();
            var bundled = GetBundledPremadeTeleportLists();
            var keep = new HashSet<string>(bundled.Select(item => item.Value), StringComparer.OrdinalIgnoreCase);

            foreach (var file in Directory.GetFiles(dir, "*.txt"))
            {
                if (!keep.Contains(Path.GetFileName(file)))
                    File.Delete(file);
            }

            foreach (var item in bundled)
                ExtractBundledPremadeTeleportList(item.Key, item.Value);
        }

        private static KeyValuePair<string, string>[] GetBundledPremadeTeleportLists()
        {
            return new[]
            {
                new KeyValuePair<string, string>("premade-teleport-all-xp-boards-ken.txt", "All XP Boards V1.1- Ken.txt"),
                new KeyValuePair<string, string>("premade-teleport-all-mascots-ken.txt", "All Mascots - Ken.txt"),
                new KeyValuePair<string, string>("premade-teleport-all-houses-ken.txt", "All Houses - Ken.txt"),
                new KeyValuePair<string, string>("premade-teleport-all-photo-list-ken.txt", "All Photo List V1.1- Ken.txt"),
                new KeyValuePair<string, string>("premade-teleport-discover-japan-ken.txt", "Discover Japan - Ken.txt"),
                new KeyValuePair<string, string>("premade-teleport-all-legends-xp-boards-patch.txt", "All Legends XP Boards - Patch.txt"),
                new KeyValuePair<string, string>("premade-teleport-all-barn-finds-patch.txt", "All Barn Finds - Patch.txt"),
                new KeyValuePair<string, string>("premade-teleport-all-treasure-cars-patch.txt", "All Treasure Cars - Patch.txt")
            };
        }

        private void ExtractBundledPremadeTeleportList(string resourceName, string fileName)
        {
            try
            {
                var dir = Path.Combine(_resultsDir, "teleport_lists", "premade");
                Directory.CreateDirectory(dir);
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
                if (_log != null)
                    Log("Premade teleport list extract skipped: " + ex.Message);
            }
        }

        private void ShowPremadeTeleportListPicker()
        {
            try
            {
                EnsureBundledPremadeTeleportLists();
                var premadeDir = GetPremadeTeleportListDirectory();
                var files = Directory.GetFiles(premadeDir, "*.txt")
                    .Select(file => new TeleportListOption(file, ReadTeleportLocationListFile(file)))
                    .Where(item => item.Locations.Count > 0)
                    .OrderBy(item => Path.GetFileNameWithoutExtension(item.FileName), StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (files.Count == 0)
                {
                    ShowInfo("No premade teleport lists were found yet." + Environment.NewLine + Environment.NewLine + premadeDir);
                    return;
                }

                using (var dialog = new Form())
                {
                    dialog.Text = "Premade Teleport Lists";
                    dialog.StartPosition = FormStartPosition.CenterParent;
                    dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                    dialog.MaximizeBox = false;
                    dialog.MinimizeBox = false;
                    dialog.ShowInTaskbar = false;
                    dialog.BackColor = AppBackground;
                    dialog.ForeColor = TextPrimary;
                    dialog.ClientSize = new Size(620, 390);

                    var title = new Label();
                    title.Text = "Choose a premade list";
                    title.Location = new Point(18, 14);
                    title.Size = new Size(584, 28);
                    title.Font = new Font("Segoe UI Semibold", 13F);
                    title.ForeColor = TextPrimary;
                    title.BackColor = AppBackground;
                    dialog.Controls.Add(title);

                    var grid = BuildReadOnlyGrid();
                    grid.Location = new Point(18, 54);
                    grid.Size = new Size(584, 276);
                    grid.Columns.Add("Name", "File");
                    grid.Columns.Add("Count", "Teleports");
                    grid.Columns["Name"].FillWeight = 76F;
                    grid.Columns["Count"].FillWeight = 24F;
                    foreach (var item in files)
                    {
                        var rowIndex = grid.Rows.Add(Path.GetFileNameWithoutExtension(item.FileName), item.Locations.Count.ToString(CultureInfo.InvariantCulture));
                        grid.Rows[rowIndex].Tag = item;
                    }
                    dialog.Controls.Add(grid);
                    if (grid.Rows.Count > 0)
                        grid.Rows[0].Selected = true;

                    var cancel = MakeButton("Cancel", 404, 346, 92, 32);
                    cancel.DialogResult = DialogResult.Cancel;
                    dialog.Controls.Add(cancel);

                    var load = MakeButton("Load", 510, 346, 92, 32);
                    MakeAccentButton(load, AccentBlue);
                    load.Click += delegate
                    {
                        if (grid.SelectedRows.Count == 0)
                            return;
                        var item = grid.SelectedRows[0].Tag as TeleportListOption;
                        if (item == null)
                            return;
                        _teleportLocations = new List<TeleportLocationRow>(item.Locations);
                        _teleportLocationListPath = GetDefaultTeleportAutoSaveListPath();
                        AutoSaveTeleportLocationList();
                        RefreshTeleportLocationsGrid();
                        SelectTeleportLocationRow(0);
                        SetDrivingStatus("Premade list loaded", AccentGreen);
                        Log("Premade teleport list loaded: " + Path.GetFileName(item.FileName) + " (" + _teleportLocations.Count.ToString(CultureInfo.InvariantCulture) + " spot(s)).");
                        dialog.DialogResult = DialogResult.OK;
                        dialog.Close();
                    };
                    dialog.Controls.Add(load);
                    dialog.AcceptButton = load;
                    dialog.CancelButton = cancel;
                    PrepareDialogForLanguage(dialog);
                dialog.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                Log("Premade teleport picker failed: " + ex.Message);
                ShowInfo(ex.Message);
            }
        }

        private string GetNextTeleportLocationName()
        {
            return GetNextTeleportLocationName("Spot");
        }

        private string GetNextTeleportLocationName(string prefix)
        {
            prefix = CleanTeleportLocationName(string.IsNullOrWhiteSpace(prefix) ? "Spot" : prefix);
            var next = _teleportLocations
                .Where(row => row != null && row.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Count() + 1;
            return prefix + " " + next.ToString(CultureInfo.InvariantCulture);
        }

        private static string CleanTeleportLocationName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Saved Spot";

            name = name.Trim();
            if (name.Length > 48)
                name = name.Substring(0, 48);
            return name;
        }

        private static string FormatTeleportCoordinate(float value)
        {
            return value.ToString("0.00", CultureInfo.InvariantCulture);
        }

        private static string FormatTeleportLocation(byte[] position)
        {
            if (position == null || position.Length < 12)
                return "unknown";

            return "X " + FormatTeleportCoordinate(BitConverter.ToSingle(position, 0)) +
                   ", Y " + FormatTeleportCoordinate(BitConverter.ToSingle(position, 4)) +
                   ", Z " + FormatTeleportCoordinate(BitConverter.ToSingle(position, 8));
        }

        private void ShowTeleportPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowTeleportPage);
                return;
            }
            HidePages();
            _teleportPage.Visible = true;
            _teleportPage.BringToFront();
            SetStatus("Teleport");
            SetDrivingStatus("Teleport");
            UpdateNavigationState(_navTeleport);
        }
    }
}
