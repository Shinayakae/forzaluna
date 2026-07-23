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
        private readonly Dictionary<string, Button> _liveTuningCurrentButtons =
            new Dictionary<string, Button>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Button> _liveTuningValueButtons =
            new Dictionary<string, Button>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Label> _liveTuningStatusLabels =
            new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, StatusDotToggle> _liveTuningToggleControls =
            new Dictionary<string, StatusDotToggle>(StringComparer.OrdinalIgnoreCase);
        private bool _syncingLiveTuningControls;

        private void BuildRarityPage()
        {
            _rarityPage.AutoScroll = true;
            AddPageHeader(_rarityPage, "Car Rarity Editor", "Load your garage, select one owned car, choose a rarity, then confirm the change.");

            var status = new Label();
            status.Text = "Ready";
            status.Font = new Font("Segoe UI Semibold", 9F);
            status.ForeColor = AccentBlue;
            status.BackColor = AppBackground;
            status.Location = new Point(690, 12);
            status.Size = new Size(210, 24);
            status.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            status.TextAlign = ContentAlignment.MiddleRight;
            _rarityPage.Controls.Add(status);
            _rarityPage.Tag = status;

            var actions = MakeCard(_rarityPage, 0, 72, ContentWidth, 132, "Rarity Actions", "Load owned cars from the live database, then write the selected rarity to the selected car.");
            actions.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var actionPanel = new Panel();
            actionPanel.Location = new Point(18, 58);
            actionPanel.Size = new Size(858, 62);
            actionPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            actionPanel.BackColor = Surface;
            actions.Controls.Add(actionPanel);

            var back = MakeButton("Back", 0, 0, 92, 38);
            back.Click += delegate { ShowProfilePage(); };
            actionPanel.Controls.Add(back);
            SetTranslatedToolTip(back, "Return to Database Tools.");

            var load = MakeButton("Load Garage", 108, 0, 132, 38);
            MakeAccentButton(load, AccentBlue);
            load.Click += delegate { RunWorker("Load Rarity Garage", LoadRarityGarage, load); };
            actionPanel.Controls.Add(load);
            SetTranslatedToolTip(load, "Reads your current owned cars, their current BaseRarity, and their CarRarities row when present.");

            var rarityLabel = MakeLabel("Rarity", 260, 9);
            rarityLabel.BackColor = Surface;
            actionPanel.Controls.Add(rarityLabel);

            _rarityCombo = new ComboBox();
            _rarityCombo.Location = new Point(318, 5);
            _rarityCombo.Size = new Size(226, 28);
            _rarityCombo.DropDownStyle = ComboBoxStyle.DropDown;
            _rarityCombo.FlatStyle = FlatStyle.Flat;
            _rarityCombo.BackColor = SurfaceAlt;
            _rarityCombo.ForeColor = TextPrimary;
            _rarityCombo.Font = new Font("Segoe UI", 9F);
            foreach (var option in GetDefaultRarityOptions())
                _rarityCombo.Items.Add(option);
            if (_rarityCombo.Items.Count > 0)
                _rarityCombo.SelectedIndex = _rarityCombo.Items.Count - 1;
            actionPanel.Controls.Add(_rarityCombo);
            SetTranslatedToolTip(_rarityCombo, "Pick a readable rarity band, or type an exact BaseRarity number from 0 to 10, such as 7.3.");

            var confirm = MakeButton("Confirm", 560, 0, 118, 38);
            MakeAccentButton(confirm, AccentGreen);
            confirm.Click += delegate
            {
                RunWorker("Apply Car Rarity", delegate
                {
                    var selection = GetSelectedRarityCar();
                    var rarity = GetSelectedRarityOption();
                    ApplySelectedCarRarity(selection, rarity);
                }, confirm);
            };
            actionPanel.Controls.Add(confirm);
            SetTranslatedToolTip(confirm, "Applies the selected rarity to the selected owned car after creating backup tables.");

            _raritySelected = MakeBodyLabel("Select an owned car after loading the garage.", 0, 42, 858, 18);
            _raritySelected.BackColor = Surface;
            actionPanel.Controls.Add(_raritySelected);

            var gridCard = MakeCard(_rarityPage, 0, 224, ContentWidth, 498, "Owned Cars", "Search your garage, then select the car whose rarity you want to edit.");
            gridCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            _raritySearch = new TextBox();
            _raritySearch.Location = new Point(18, 50);
            _raritySearch.Size = new Size(858, 28);
            _raritySearch.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            StyleTextBox(_raritySearch);
            _raritySearch.TextChanged += delegate { ApplyRarityFilter(); };
            gridCard.Controls.Add(_raritySearch);
            SetTranslatedToolTip(_raritySearch, "Search owned cars by ID, year, make, model, PI, class, copies, or rarity.");

            _rarityGrid = BuildRarityGrid();
            _rarityGrid.Location = new Point(18, 88);
            _rarityGrid.Size = new Size(858, 392);
            _rarityGrid.SelectionChanged += delegate { UpdateRaritySelectionLabel(); };
            gridCard.Controls.Add(_rarityGrid);
        }

        private void BuildSkillPage()
        {
            _skillPage.AutoScroll = true;
            AddPageHeader(_skillPage, "Stats Editor", "Load your garage, pick a car, type a new stat, then press Apply.");

            var status = new Label();
            status.Text = "Ready";
            status.Font = new Font("Segoe UI Semibold", 9F);
            status.ForeColor = AccentBlue;
            status.BackColor = AppBackground;
            status.Location = new Point(690, 12);
            status.Size = new Size(210, 24);
            status.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            status.TextAlign = ContentAlignment.MiddleRight;
            _skillPage.Controls.Add(status);
            _skillPage.Tag = status;

            var actions = MakeCard(_skillPage, 0, 72, ContentWidth, 132, "Stats Actions", "Use these buttons to load cars and save the stat changes.");
            actions.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var actionPanel = new Panel();
            actionPanel.Location = new Point(18, 58);
            actionPanel.Size = new Size(858, 62);
            actionPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            actionPanel.BackColor = Surface;
            actions.Controls.Add(actionPanel);

            var back = MakeButton("Back", 0, 0, 92, 38);
            back.Click += delegate { ShowProfilePage(); };
            actionPanel.Controls.Add(back);
            SetTranslatedToolTip(back, "Go back to Database Tools.");

            var load = MakeButton("Load Garage", 108, 0, 132, 38);
            MakeAccentButton(load, AccentBlue);
            load.Click += delegate { RunWorker("Load Stats", LoadSkillData, load); };
            actionPanel.Controls.Add(load);
            SetTranslatedToolTip(load, "Loads your cars and their saved stats.");

            var applyCar = MakeButton("Apply Stats", 256, 0, 130, 38);
            MakeAccentButton(applyCar, AccentGreen);
            applyCar.Click += delegate
            {
                RunWorker("Apply Stats", delegate
                {
                    var car = GetSelectedSkillGarageCar();
                    var values = CaptureStatsEditorValues();
                    ApplySelectedCarStats(car, values);
                }, applyCar);
            };
            actionPanel.Controls.Add(applyCar);
            SetTranslatedToolTip(applyCar, "Saves the typed stats for the selected car.");

            var hint = MakeBodyLabel("After applying, switch garage screens so the game refreshes the old menu values.", 402, 4, 456, 36);
            hint.BackColor = Surface;
            actionPanel.Controls.Add(hint);

            _skillSelectedCar = MakeBodyLabel("Selected car: none", 0, 42, 858, 18);
            _skillSelectedCar.BackColor = Surface;
            actionPanel.Controls.Add(_skillSelectedCar);

            var garageCard = MakeCard(_skillPage, 0, 224, ContentWidth, 288, "Owned Garage Cars", "Pick the car you want to edit.");
            garageCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            _skillGarageSearch = new TextBox();
            _skillGarageSearch.Location = new Point(18, 50);
            _skillGarageSearch.Size = new Size(858, 28);
            _skillGarageSearch.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            StyleTextBox(_skillGarageSearch);
            _skillGarageSearch.TextChanged += delegate { ApplySkillGarageFilter(); };
            garageCard.Controls.Add(_skillGarageSearch);

            _skillGarageGrid = BuildSkillGarageGrid();
            _skillGarageGrid.Location = new Point(18, 88);
            _skillGarageGrid.Size = new Size(858, 182);
            _skillGarageGrid.SelectionChanged += delegate { UpdateSkillGarageSelection(); };
            garageCard.Controls.Add(_skillGarageGrid);

            var statsCard = MakeCard(_skillPage, 0, 530, ContentWidth, 404, "Editable Stats", "Type new values in the Value column.");
            statsCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            _statsEditorGrid = BuildStatsEditorGrid();
            _statsEditorGrid.Location = new Point(18, 58);
            _statsEditorGrid.Size = new Size(858, 328);
            statsCard.Controls.Add(_statsEditorGrid);

            var profileCard = MakeCard(_skillPage, 0, 952, ContentWidth, 404, "Profile Editable Stats", "Profile stats Luna can load and edit.");
            profileCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var applyProfile = MakeButton("Apply Profile Stats", 18, 58, 154, 36);
            MakeAccentButton(applyProfile, AccentGreen);
            applyProfile.Click += delegate
            {
                RunWorker("Apply Profile Stats", delegate
                {
                    var values = CaptureProfileStatsValues();
                    ApplyProfileStats(values);
                }, applyProfile);
            };
            profileCard.Controls.Add(applyProfile);
            SetTranslatedToolTip(applyProfile, "Saves the filled profile stat values.");

            var loadProfile = MakeButton("Load Current", 188, 58, 130, 36);
            MakeAccentButton(loadProfile, AccentBlue);
            loadProfile.Click += delegate { RunWorker("Load Profile Stats", LoadProfileEditableStats, loadProfile); };
            profileCard.Controls.Add(loadProfile);
            SetTranslatedToolTip(loadProfile, "Reloads the current profile stat values.");

            var profileHint = MakeBodyLabel("Blank Value cells are skipped. If a row says varies, type one value only when you want every matching row to use it.", 336, 58, 540, 36);
            profileHint.BackColor = Surface;
            profileCard.Controls.Add(profileHint);

            _profileStatsGrid = BuildProfileStatsGrid();
            _profileStatsGrid.Location = new Point(18, 108);
            _profileStatsGrid.Size = new Size(858, 278);
            profileCard.Controls.Add(_profileStatsGrid);
        }

        private void BuildCarEditorPage()
        {
            _carEditorPage.AutoScroll = true;
            AddPageHeader(_carEditorPage, "Car Class Panel", "Load owned cars, select one, then change only the class saved for that garage car.");

            var status = new Label();
            status.Text = "Ready";
            status.Font = new Font("Segoe UI Semibold", 9F);
            status.ForeColor = AccentBlue;
            status.BackColor = AppBackground;
            status.Location = new Point(690, 12);
            status.Size = new Size(210, 24);
            status.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            status.TextAlign = ContentAlignment.MiddleRight;
            _carEditorPage.Controls.Add(status);
            _carEditorPage.Tag = status;

            var actions = MakeCard(_carEditorPage, 0, 72, ContentWidth, 162, "Car Class Actions", "Focused class editor for owned cars. No speed, PI, cost, or performance values are changed here.");
            actions.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var actionPanel = new Panel();
            actionPanel.Location = new Point(18, 58);
            actionPanel.Size = new Size(858, 92);
            actionPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            actionPanel.BackColor = Surface;
            actions.Controls.Add(actionPanel);

            var back = MakeButton("Back", 0, 0, 92, 38);
            back.Click += delegate { ShowProfilePage(); };
            actionPanel.Controls.Add(back);
            SetTranslatedToolTip(back, "Return to Database Tools.");

            var load = MakeButton("Load Garage", 102, 0, 126, 38);
            MakeAccentButton(load, AccentBlue);
            load.Click += delegate { RunWorker("Load Car Classes", LoadCarEditorGarage, load); };
            actionPanel.Controls.Add(load);
            SetTranslatedToolTip(load, "Reads owned garage cars and their current class from the live database.");

            var classLabel = MakeLabel("New Class", 252, 10);
            classLabel.BackColor = Surface;
            actionPanel.Controls.Add(classLabel);

            _carEditorClassCombo = MakeEditorCombo(330, 5, 150);
            foreach (var option in GetCarClassOptions())
                _carEditorClassCombo.Items.Add(option);
            actionPanel.Controls.Add(_carEditorClassCombo);
            SetTranslatedToolTip(_carEditorClassCombo, "Choose the class to save for the selected owned car.");

            var apply = MakeButton("Apply Class", 500, 0, 124, 38);
            MakeAccentButton(apply, AccentGreen);
            apply.Click += delegate
            {
                var selected = GetSelectedCarEditorRow();
                var classId = GetSelectedNamedOptionId(_carEditorClassCombo, "Class");
                RunWorker("Apply Car Class", delegate { ApplySelectedCarClass(selected, classId); }, apply);
            };
            actionPanel.Controls.Add(apply);
            SetTranslatedToolTip(apply, "Creates guarded backups, then writes the selected class to Data_Car and owned garage mirror rows.");

            var hint = MakeBodyLabel("Switch garage screens after applying if FH6 cached the old class.", 642, 2, 216, 40);
            hint.BackColor = Surface;
            actionPanel.Controls.Add(hint);

            _carEditorSelected = MakeBodyLabel("Selected car: none", 0, 54, 858, 26);
            _carEditorSelected.BackColor = Surface;
            actionPanel.Controls.Add(_carEditorSelected);

            _carEditorDrivetrainCombo = MakeEditorCombo(0, 0, 1);
            _carEditorEngineCombo = MakeEditorCombo(0, 0, 1);
            _carEditorAspirationCombo = MakeEditorCombo(0, 0, 1);

            var garageCard = MakeCard(_carEditorPage, 0, 254, ContentWidth, 600, "Owned Garage Cars", "Search your garage, select one car, choose a class, then press Apply Class.");
            garageCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            _carEditorSearch = new TextBox();
            _carEditorSearch.Location = new Point(18, 50);
            _carEditorSearch.Size = new Size(858, 28);
            _carEditorSearch.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            StyleTextBox(_carEditorSearch);
            _carEditorSearch.TextChanged += delegate { ApplyCarEditorFilter(); };
            garageCard.Controls.Add(_carEditorSearch);
            SetTranslatedToolTip(_carEditorSearch, "Search by ID, year, make, model, PI, class, or copies.");

            _carEditorGrid = BuildCarEditorGrid();
            _carEditorGrid.Location = new Point(18, 88);
            _carEditorGrid.Size = new Size(858, 494);
            _carEditorGrid.SelectionChanged += delegate { UpdateCarEditorSelection(); };
            garageCard.Controls.Add(_carEditorGrid);
        }

        private void BuildVehicleTunerPage()
        {
            _vehicleTunerPage.AutoScroll = true;
            AddPageHeader(_vehicleTunerPage, "Database Vehicle Tuner", "Physics and performance values from the database. Load Garage, select a car, then apply.");

            var status = new Label();
            status.Text = "Ready";
            status.Font = new Font("Segoe UI Semibold", 9F);
            status.ForeColor = AccentBlue;
            status.BackColor = AppBackground;
            status.Location = new Point(690, 12);
            status.Size = new Size(210, 24);
            status.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            status.TextAlign = ContentAlignment.MiddleRight;
            _vehicleTunerPage.Controls.Add(status);

            var actions = MakeCard(_vehicleTunerPage, 0, 72, ContentWidth, 166, "Vehicle Tuner Actions", "Selected-car values use backups so red rows can restore defaults later.");
            actions.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var actionPanel = new Panel();
            actionPanel.Location = new Point(18, 58);
            actionPanel.Size = new Size(858, 92);
            actionPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            actionPanel.BackColor = Surface;
            actions.Controls.Add(actionPanel);

            var back = MakeButton("Back", 0, 0, 92, 38);
            back.Click += delegate { ShowProfilePage(); };
            actionPanel.Controls.Add(back);
            SetTranslatedToolTip(back, "Return to Database Tools.");

            var load = MakeButton("Load Garage", 108, 0, 132, 38);
            MakeAccentButton(load, AccentBlue);
            load.Click += delegate { RunWorker("Load Vehicle Tuner Garage", LoadVehicleTunerGarage, load); };
            actionPanel.Controls.Add(load);
            SetTranslatedToolTip(load, "Loads owned cars for selected-car tuning.");

            var apply = MakeButton("Apply Tuner", 256, 0, 132, 38);
            MakeAccentButton(apply, AccentGreen);
            apply.Click += delegate
            {
                var inputs = CaptureVehicleTunerInputs();
                RunWorker("Apply Vehicle Tuner", delegate { ApplyVehicleTunerValues(inputs); }, apply);
            };
            actionPanel.Controls.Add(apply);
            SetTranslatedToolTip(apply, "Green rows are applied. Red rows restore their Luna backup when available.");

            var restore = MakeButton("Restore Defaults", 404, 0, 144, 38);
            restore.Click += delegate { RunWorker("Restore Vehicle Tuner", RestoreVehicleTunerDefaults, restore); };
            actionPanel.Controls.Add(restore);
            SetTranslatedToolTip(restore, "Restores vehicle tuner backups for supported columns.");

            var hint = MakeBodyLabel("Select a car first, then choose Traction, Torque, or Reduce Drag.", 568, 1, 290, 40);
            hint.BackColor = Surface;
            actionPanel.Controls.Add(hint);

            _vehicleTunerSelected = MakeBodyLabel("Selected car: none", 0, 54, 858, 26);
            _vehicleTunerSelected.BackColor = Surface;
            actionPanel.Controls.Add(_vehicleTunerSelected);

            var options = MakeCard(_vehicleTunerPage, 0, 258, ContentWidth, 316, "Physics & Performance", "Click the dot to turn a row green, edit the value if needed, then press Apply Tuner.");
            options.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var currentBox = new ModernPanel();
            currentBox.Location = new Point(18, 56);
            currentBox.Size = new Size(420, 46);
            currentBox.FillColor = SurfaceAlt;
            currentBox.BorderColor = Blend(Border, AccentBlue, 0.28F);
            currentBox.CornerRadius = 12;
            currentBox.BackColor = Surface;
            options.Controls.Add(currentBox);

            _vehicleTunerCurrentSummary = new Label();
            _vehicleTunerCurrentSummary.Text = "Current: load garage and select a car";
            _vehicleTunerCurrentSummary.Location = new Point(12, 9);
            _vehicleTunerCurrentSummary.Size = new Size(396, 28);
            _vehicleTunerCurrentSummary.BackColor = currentBox.FillColor;
            _vehicleTunerCurrentSummary.ForeColor = TextPrimary;
            _vehicleTunerCurrentSummary.Font = new Font("Segoe UI Semibold", 9F);
            _vehicleTunerCurrentSummary.TextAlign = ContentAlignment.MiddleCenter;
            currentBox.Controls.Add(_vehicleTunerCurrentSummary);

            var defaultBox = new ModernPanel();
            defaultBox.Location = new Point(456, 56);
            defaultBox.Size = new Size(420, 46);
            defaultBox.FillColor = SurfaceAlt;
            defaultBox.BorderColor = Blend(Border, AccentGreen, 0.28F);
            defaultBox.CornerRadius = 12;
            defaultBox.BackColor = Surface;
            options.Controls.Add(defaultBox);

            _vehicleTunerDefaultSummary = new Label();
            _vehicleTunerDefaultSummary.Text = "Defaults: traction x10, torque x2, drag x0.5";
            _vehicleTunerDefaultSummary.Location = new Point(12, 9);
            _vehicleTunerDefaultSummary.Size = new Size(396, 28);
            _vehicleTunerDefaultSummary.BackColor = defaultBox.FillColor;
            _vehicleTunerDefaultSummary.ForeColor = TextMuted;
            _vehicleTunerDefaultSummary.Font = new Font("Segoe UI Semibold", 9F);
            _vehicleTunerDefaultSummary.TextAlign = ContentAlignment.MiddleCenter;
            defaultBox.Controls.Add(_vehicleTunerDefaultSummary);

            _vehicleTunerOptionsGrid = BuildVehicleTunerOptionsGrid();
            _vehicleTunerOptionsGrid.Location = new Point(18, 118);
            _vehicleTunerOptionsGrid.Size = new Size(858, 178);
            options.Controls.Add(_vehicleTunerOptionsGrid);

            var garageCard = MakeCard(_vehicleTunerPage, 0, 594, ContentWidth, 328, "Owned Garage Cars", "Search your garage, select one car, then apply the selected tuner rows.");
            garageCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            _vehicleTunerSearch = new TextBox();
            _vehicleTunerSearch.Location = new Point(18, 50);
            _vehicleTunerSearch.Size = new Size(858, 28);
            _vehicleTunerSearch.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            StyleTextBox(_vehicleTunerSearch);
            _vehicleTunerSearch.TextChanged += delegate { ApplyVehicleTunerFilter(); };
            garageCard.Controls.Add(_vehicleTunerSearch);

            _vehicleTunerGarageGrid = BuildCarEditorGrid();
            _vehicleTunerGarageGrid.Location = new Point(18, 88);
            _vehicleTunerGarageGrid.Size = new Size(858, 222);
            _vehicleTunerGarageGrid.SelectionChanged += delegate { UpdateVehicleTunerSelection(); };
            garageCard.Controls.Add(_vehicleTunerGarageGrid);
        }

        private void BuildDatabaseTuningPage()
        {
            _databaseTuningPage.AutoScroll = true;
            AddPageHeader(_databaseTuningPage, "Database Tuning", "Advanced selected-car database editor for tuning values, installed parts, performance, physics, and flags.");

            var status = new Label();
            status.Text = "Ready";
            status.Font = new Font("Segoe UI Semibold", 9F);
            status.ForeColor = AccentBlue;
            status.BackColor = AppBackground;
            status.Location = new Point(690, 12);
            status.Size = new Size(210, 24);
            status.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            status.TextAlign = ContentAlignment.MiddleRight;
            _databaseTuningPage.Controls.Add(status);
            ResizePageHeader(_databaseTuningPage, TwoColumnContentWidth, status);

            var actions = MakeCard(_databaseTuningPage, 0, 72, TwoColumnContentWidth, 228, "Database Tuning Actions", "Select an owned car, edit checked rows, then apply. Restore uses Luna backups for the selected car.");
            actions.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var actionPanel = new Panel();
            actionPanel.Location = new Point(18, 58);
            actionPanel.Size = new Size(TwoColumnContentWidth - 36, 166);
            actionPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            actionPanel.BackColor = Surface;
            actions.Controls.Add(actionPanel);

            _databaseTuningSelected = MakeBodyLabel("Selected car: none", 0, 0, 640, 26);
            _databaseTuningSelected.BackColor = Surface;
            actionPanel.Controls.Add(_databaseTuningSelected);

            _databaseTuningSummary = MakeBodyLabel("Fields: load garage and select a car", 760, 0, 658, 26);
            _databaseTuningSummary.BackColor = Surface;
            _databaseTuningSummary.TextAlign = ContentAlignment.MiddleRight;
            actionPanel.Controls.Add(_databaseTuningSummary);

            var load = MakeButton("Load Garage", 0, 40, 132, 38);
            MakeAccentButton(load, AccentBlue);
            load.Click += delegate { RunWorker("Load Database Tuning Garage", LoadDatabaseTuningGarage, load); };
            actionPanel.Controls.Add(load);
            SetTranslatedToolTip(load, "Loads owned cars and prepares the advanced database tuning editor.");

            var apply = MakeButton("Apply Selected", 144, 40, 142, 38);
            MakeAccentButton(apply, AccentGreen);
            apply.Click += delegate { RunWorker("Apply Database Tuning", ApplyDatabaseTuningSelected, apply); };
            actionPanel.Controls.Add(apply);
            SetTranslatedToolTip(apply, "Applies checked rows or edited values to the selected car.");

            var restore = MakeButton("Restore Selected", 298, 40, 150, 38);
            restore.Click += delegate { RunWorker("Restore Database Tuning", RestoreDatabaseTuningSelectedCar, restore); };
            actionPanel.Controls.Add(restore);
            SetTranslatedToolTip(restore, "Restores the selected car from Database Tuning backups.");

            var pick = MakeButton("Pick Value", 460, 40, 118, 38);
            pick.Click += delegate { ShowSelectedDatabaseTuningValueOrPartPicker(); };
            actionPanel.Controls.Add(pick);
            SetTranslatedToolTip(pick, "Open the safe picker for the selected value or installed part row.");

            var backup = MakeButton("Backup Save", 590, 40, 122, 38);
            backup.Click += delegate { RunWorker("Backup Save", BackupPgsSave, backup); };
            actionPanel.Controls.Add(backup);
            SetTranslatedToolTip(backup, "Backs up the current FH6 PGS save folder into Luna's save_backups folder.");

            var back = MakeButton("Database Tools", 724, 40, 134, 38);
            back.Click += delegate { ShowProfilePage(); };
            actionPanel.Controls.Add(back);
            SetTranslatedToolTip(back, "Return to Database Tools.");

            var saveConfig = MakeButton("Save Config", 0, 88, 132, 34);
            MakeAccentButton(saveConfig, AccentBlue);
            saveConfig.Click += delegate { SaveDatabaseTuningConfig(); };
            actionPanel.Controls.Add(saveConfig);
            SetTranslatedToolTip(saveConfig, "Save the selected car's Database Tuning configuration.");

            var loadConfig = MakeButton("Load Config", 144, 88, 132, 34);
            MakeAccentButton(loadConfig, AccentBlue);
            loadConfig.Click += delegate { LoadDatabaseTuningConfig(); };
            actionPanel.Controls.Add(loadConfig);
            SetTranslatedToolTip(loadConfig, "Load a Database Tuning configuration for the selected owned car.");

            var hint = MakeBodyLabel("Tip: Pick Part highlights safe parts in yellow and blocks crash-prone parts in red.", 292, 92, 920, 26);
            hint.BackColor = Surface;
            hint.ForeColor = TextMuted;
            actionPanel.Controls.Add(hint);

            var garageCard = MakeCard(_databaseTuningPage, 0, 320, TwoColumnWidth, 560, "Owned Garage Cars", "Search your garage, select one car, then edit its tuning data below.");
            garageCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var garageSearchFrame = new ModernPanel();
            garageSearchFrame.Location = new Point(18, 60);
            garageSearchFrame.Size = new Size(TwoColumnWidth - 36, 38);
            garageSearchFrame.FillColor = SurfaceAlt;
            garageSearchFrame.BorderColor = Border;
            garageSearchFrame.CornerRadius = 8;
            garageSearchFrame.Tag = "SearchField";
            garageCard.Controls.Add(garageSearchFrame);

            _databaseTuningGarageSearch = new TextBox();
            _databaseTuningGarageSearch.Location = new Point(12, 9);
            _databaseTuningGarageSearch.Size = new Size(TwoColumnWidth - 60, 22);
            _databaseTuningGarageSearch.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            _databaseTuningGarageSearch.BorderStyle = BorderStyle.None;
            _databaseTuningGarageSearch.BackColor = SurfaceAlt;
            _databaseTuningGarageSearch.ForeColor = TextPrimary;
            _databaseTuningGarageSearch.Font = new Font("Segoe UI", 9.5F);
            _databaseTuningGarageSearch.TextChanged += delegate { ApplyDatabaseTuningGarageFilter(); };
            garageSearchFrame.Controls.Add(_databaseTuningGarageSearch);
            SetTranslatedToolTip(_databaseTuningGarageSearch, "Search owned cars by ID, year, make, model, class, PI, or copies.");

            _databaseTuningGarageGrid = BuildCarEditorGrid();
            _databaseTuningGarageGrid.Location = new Point(18, 112);
            _databaseTuningGarageGrid.Size = new Size(TwoColumnWidth - 36, 430);
            _databaseTuningGarageGrid.SelectionChanged += delegate { UpdateDatabaseTuningSelection(); };
            garageCard.Controls.Add(_databaseTuningGarageGrid);

            var editorCard = MakeCard(_databaseTuningPage, TwoColumnWidth + TwoColumnGap, 320, TwoColumnWidth, 560, "Selected Car Editor", "Search, pick parts, apply selected values, and restore backups for the chosen car.");
            editorCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var editorSearchFrame = new ModernPanel();
            editorSearchFrame.Location = new Point(18, 60);
            editorSearchFrame.Size = new Size(320, 38);
            editorSearchFrame.FillColor = SurfaceAlt;
            editorSearchFrame.BorderColor = Border;
            editorSearchFrame.CornerRadius = 8;
            editorSearchFrame.Tag = "SearchField";
            editorCard.Controls.Add(editorSearchFrame);

            _databaseTuningFieldSearch = new TextBox();
            _databaseTuningFieldSearch.Location = new Point(12, 9);
            _databaseTuningFieldSearch.Size = new Size(296, 22);
            _databaseTuningFieldSearch.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            _databaseTuningFieldSearch.BorderStyle = BorderStyle.None;
            _databaseTuningFieldSearch.BackColor = SurfaceAlt;
            _databaseTuningFieldSearch.ForeColor = TextPrimary;
            _databaseTuningFieldSearch.Font = new Font("Segoe UI", 9.5F);
            _databaseTuningFieldSearch.TextChanged += delegate { ApplyDatabaseTuningFieldFilter(); };
            editorSearchFrame.Controls.Add(_databaseTuningFieldSearch);
            SetTranslatedToolTip(_databaseTuningFieldSearch, "Search settings by section, name, source table, or description.");

            var sortOptions = MakeButton("Sort Options", 352, 62, 110, 34);
            sortOptions.Click += delegate { ShowDatabaseTuningSortOptions(); };
            editorCard.Controls.Add(sortOptions);
            SetTranslatedToolTip(sortOptions, "Open preset filters for common selected-car tuning workflows.");

            var removeFilter = MakeButton("Remove Filter", 474, 62, 116, 34);
            removeFilter.Click += delegate { ClearDatabaseTuningFilterPreset(); };
            editorCard.Controls.Add(removeFilter);
            SetTranslatedToolTip(removeFilter, "Remove the selected-car editor preset filter.");

            _databaseTuningFilterLabel = MakeBodyLabel("Filter: All options", 600, 65, 102, 28);
            _databaseTuningFilterLabel.BackColor = Surface;
            _databaseTuningFilterLabel.ForeColor = TextMuted;
            _databaseTuningFilterLabel.TextAlign = ContentAlignment.MiddleRight;
            _databaseTuningFilterLabel.AutoEllipsis = true;
            editorCard.Controls.Add(_databaseTuningFilterLabel);

            _databaseTuningGrid = BuildDatabaseTuningGrid();
            _databaseTuningGrid.Location = new Point(18, 112);
            _databaseTuningGrid.Size = new Size(TwoColumnWidth - 36, 430);
            _databaseTuningGrid.CellBeginEdit += delegate(object sender, DataGridViewCellCancelEventArgs e)
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0 || _databaseTuningGrid.Columns[e.ColumnIndex].Name != "NewValue")
                    return;

                e.Cancel = true;
                BeginInvoke((Action)ShowSelectedDatabaseTuningValueOrPartPicker);
            };
            _databaseTuningGrid.CellDoubleClick += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex >= 0)
                    ShowSelectedDatabaseTuningValueOrPartPicker();
            };
            editorCard.Controls.Add(_databaseTuningGrid);

            _databaseTuningPage.AutoScrollMinSize = new Size(TwoColumnContentWidth, 912);
        }

        private static DataGridView BuildDatabaseTuningGrid()
        {
            var grid = BuildReadOnlyGrid();
            grid.ReadOnly = false;
            grid.MultiSelect = false;
            grid.EditMode = DataGridViewEditMode.EditOnEnter;
            grid.ScrollBars = ScrollBars.Both;

            var applyColumn = new DataGridViewCheckBoxColumn();
            applyColumn.Name = "Apply";
            applyColumn.HeaderText = "Apply";
            applyColumn.FalseValue = false;
            applyColumn.TrueValue = true;
            applyColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            applyColumn.Width = 52;
            grid.Columns.Add(applyColumn);
            grid.Columns.Add("Section", "Section");
            grid.Columns.Add("Setting", "Setting");
            grid.Columns.Add("Current", "Current");
            grid.Columns.Add("NewValue", "New Value");
            grid.Columns.Add("Default", "Default");
            grid.Columns.Add("Source", "Source");
            grid.Columns.Add("Description", "What it does");

            foreach (DataGridViewColumn column in grid.Columns)
                column.ReadOnly = true;
            grid.Columns["Apply"].ReadOnly = false;
            grid.Columns["NewValue"].ReadOnly = false;

            grid.Columns["Section"].FillWeight = 16F;
            grid.Columns["Setting"].FillWeight = 22F;
            grid.Columns["Current"].FillWeight = 14F;
            grid.Columns["NewValue"].FillWeight = 14F;
            grid.Columns["Default"].FillWeight = 14F;
            grid.Columns["Source"].FillWeight = 12F;
            grid.Columns["Description"].FillWeight = 28F;
            grid.CurrentCellDirtyStateChanged += delegate
            {
                if (grid.IsCurrentCellDirty)
                    grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            return grid;
        }

        private void BuildTuningPage()
        {
            _tuningPage.AutoScroll = true;
            AddPageHeader(_tuningPage, "Driving Tuning", "Live tune values for the current car. Start scan, edit values, then apply selected rows.");
            _liveTuningCurrentButtons.Clear();
            _liveTuningValueButtons.Clear();
            _liveTuningStatusLabels.Clear();
            _liveTuningToggleControls.Clear();

            var status = new Label();
            status.Text = "Ready";
            status.Font = new Font("Segoe UI Semibold", 9F);
            status.ForeColor = AccentBlue;
            status.BackColor = AppBackground;
            status.Location = new Point(690, 12);
            status.Size = new Size(210, 24);
            status.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            status.TextAlign = ContentAlignment.MiddleRight;
            _tuningPage.Controls.Add(status);
            _tuningPage.Tag = status;
            ResizePageHeader(_tuningPage, TwoColumnContentWidth, status);

            var actions = MakeCard(_tuningPage, 0, 72, TwoColumnContentWidth, 154, "Driving Tuning Actions", "Keep live scan running while you edit. Luna updates the values below if the game changes.");
            actions.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var actionPanel = new Panel();
            actionPanel.Location = new Point(18, 58);
            actionPanel.Size = new Size(TwoColumnContentWidth - 36, 80);
            actionPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            actionPanel.BackColor = Surface;
            actions.Controls.Add(actionPanel);

            var scan = MakeButton("Start Live Scan", 0, 0, 136, 38);
            _tuningLiveScanButton = scan;
            MakeAccentButton(scan, AccentBlue);
            scan.Click += delegate { ToggleTuningLiveScan(scan); };
            actionPanel.Controls.Add(scan);
            SetTranslatedToolTip(scan, "Keeps checking live tuning values until you stop it.");

            var load = MakeButton("Load Current", 152, 0, 126, 38);
            MakeAccentButton(load, AccentBlue);
            load.Click += delegate { RunWorker("Load Tuning Current", LoadLiveTuningCurrent, load); };
            actionPanel.Controls.Add(load);
            SetTranslatedToolTip(load, "Reads every live tuning value Luna can currently resolve.");

            var apply = MakeButton("Apply Selected", 294, 0, 138, 38);
            MakeAccentButton(apply, AccentGreen);
            apply.Click += delegate
            {
                RunWorker("Apply Tuning", delegate
                {
                    var inputs = CaptureLiveTuningInputs();
                    ApplyLiveTuningInputs(inputs);
                }, apply);
            };
            actionPanel.Controls.Add(apply);
            SetTranslatedToolTip(apply, "Applies only the green-dot tuning rows.");

            var restore = MakeButton("Restore Snapshot", 448, 0, 148, 38);
            restore.Click += delegate { RunWorker("Restore Tuning", RestoreLiveTuningSnapshot, restore); };
            actionPanel.Controls.Add(restore);
            SetTranslatedToolTip(restore, "Restores the original bytes Luna captured before applying tuning rows.");

            var hint = MakeBodyLabel("Live scan stays active until you stop it. Edited values remain while current values refresh.", 0, 50, 610, 22);
            hint.BackColor = Surface;
            hint.ForeColor = TextMuted;
            hint.TextAlign = ContentAlignment.MiddleLeft;
            actionPanel.Controls.Add(hint);

            _tuningSelected = MakeBodyLabel("Start Live Scan, then edit and apply the rows you want.", 746, 50, TwoColumnContentWidth - 806, 22);
            _tuningSelected.BackColor = Surface;
            _tuningSelected.ForeColor = TextMuted;
            _tuningSelected.TextAlign = ContentAlignment.MiddleRight;
            actionPanel.Controls.Add(_tuningSelected);

            _tuningGrid = BuildTuningGrid();
            _tuningGrid.Visible = false;
            _tuningPage.Controls.Add(_tuningGrid);
            _tuningGrid.SendToBack();
            _tuningGrid.CellValueChanged += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex >= 0)
                    SyncLiveTuningRowControl(_tuningGrid.Rows[e.RowIndex]);
            };

            var leftY = 246;
            var rightY = 246;
            foreach (var category in LiveTuningFields.GroupBy(field => field.Category))
            {
                if (leftY <= rightY)
                    leftY = AddLiveTuningCategory(category.Key, category.ToList(), 0, leftY);
                else
                    rightY = AddLiveTuningCategory(category.Key, category.ToList(), TwoColumnWidth + TwoColumnGap, rightY);
            }
            var sectionY = Math.Max(leftY, rightY);

            foreach (DataGridViewRow row in _tuningGrid.Rows)
                SyncLiveTuningRowControl(row);

            _tuningPage.AutoScrollMinSize = new Size(TwoColumnContentWidth, sectionY + 28);
        }

        private Label MakeLiveTuningSectionHeader(string text, int y)
        {
            var header = new Label();
            SetUiText(header, (text ?? string.Empty).ToUpperInvariant());
            header.UseMnemonic = false;
            header.Font = new Font("Segoe UI Semibold", 9F);
            header.ForeColor = TextMuted;
            header.BackColor = AppBackground;
            header.Location = new Point(6, y);
            header.Size = new Size(500, 18);
            return header;
        }

        private int AddLiveTuningCategory(string title, List<LiveTuningField> fields, int x, int y)
        {
            if (fields == null || fields.Count == 0 || _tuningGrid == null)
                return y;

            var header = MakeLiveTuningSectionHeader(title, y);
            header.Left = x + 6;
            header.Width = TwoColumnWidth - 12;
            _tuningPage.Controls.Add(header);
            y += 26;
            const int rowHeight = 58;
            var cardHeight = (fields.Count * rowHeight) + 12;
            var card = MakeCard(_tuningPage, x, y, TwoColumnWidth, cardHeight, string.Empty, string.Empty);
            card.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            for (var index = 0; index < fields.Count; index++)
            {
                var field = fields[index];
                var row = _tuningGrid.Rows.Cast<DataGridViewRow>().First(item =>
                {
                    var rowField = item.Tag as LiveTuningField;
                    return rowField != null && string.Equals(rowField.Key, field.Key, StringComparison.OrdinalIgnoreCase);
                });
                AddLiveTuningCardRow(card, field, row, 6 + (index * rowHeight));
            }

            y += cardHeight + 18;
            return y;
        }

        private void AddLiveTuningCardRow(Control parent, LiveTuningField field, DataGridViewRow row, int y)
        {
            var name = MakeLabel(field.DisplayName, 18, y + 10);
            SetUiText(name, field.DisplayName);
            name.Size = new Size(142, 38);
            name.AutoSize = false;
            name.AutoEllipsis = true;
            name.TextAlign = ContentAlignment.MiddleLeft;
            parent.Controls.Add(name);
            SetTranslatedToolTip(name, field.Description);

            var description = MakeBodyLabel(field.Description, 168, y + 7, 252, 44);
            SetUiText(description, field.Description);
            description.BackColor = Surface;
            description.ForeColor = TextMuted;
            description.AutoEllipsis = true;
            description.TextAlign = ContentAlignment.MiddleLeft;
            parent.Controls.Add(description);
            SetTranslatedToolTip(description, field.Description);

            var status = new Label();
            status.Location = new Point(430, y + 17);
            status.Size = new Size(76, 24);
            status.Font = new Font("Segoe UI Semibold", 8.5F);
            status.BackColor = Surface;
            status.TextAlign = ContentAlignment.MiddleCenter;
            status.AutoEllipsis = true;
            parent.Controls.Add(status);
            _liveTuningStatusLabels[field.Key] = status;
            SetTranslatedToolTip(status, "Found means Luna can currently read and write this live tuning row.");

            var currentText = Convert.ToString(row.Cells["Current"].Value, CultureInfo.InvariantCulture) ?? string.Empty;
            var current = MakeButton(currentText, 514, y + 13, 62, 32);
            current.Tag = "TuningCurrent";
            current.Cursor = Cursors.Default;
            current.TabStop = false;
            parent.Controls.Add(current);
            _liveTuningCurrentButtons[field.Key] = current;
            SetTranslatedToolTip(current, "Current value read from the attached game.");

            var valueText = Convert.ToString(row.Cells["Value"].Value, CultureInfo.InvariantCulture) ?? string.Empty;
            var value = MakeButton(string.IsNullOrWhiteSpace(valueText) ? "Set Value" : valueText, 584, y + 13, 78, 32);
            value.Tag = "TuningValue";
            value.Click += delegate
            {
                var currentValue = Convert.ToString(row.Cells["Value"].Value, CultureInfo.InvariantCulture) ?? string.Empty;
                if (string.IsNullOrWhiteSpace(currentValue))
                {
                    var liveValue = Convert.ToString(row.Cells["Current"].Value, CultureInfo.InvariantCulture) ?? string.Empty;
                    float parsed;
                    if (float.TryParse(liveValue, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                        currentValue = liveValue;
                }

                var selected = ShowLiveTuningValuePrompt(field, currentValue);
                if (selected == null)
                    return;
                row.Cells["Value"].Value = selected;
                SyncLiveTuningRowControl(row);
            };
            parent.Controls.Add(value);
            _liveTuningValueButtons[field.Key] = value;
            SetTranslatedToolTip(value, "Click to edit the value that will be written when this row is green.");

            var toggle = new StatusDotToggle();
            toggle.SetBounds(670, y + 18, 40, 22);
            toggle.Checked = row.Cells["Enabled"].Value is bool && (bool)row.Cells["Enabled"].Value;
            toggle.CheckedChanged += delegate
            {
                if (_syncingLiveTuningControls)
                    return;
                row.Cells["Enabled"].Value = toggle.Checked;
            };
            parent.Controls.Add(toggle);
            _liveTuningToggleControls[field.Key] = toggle;
            SetTranslatedToolTip(toggle, "Green applies this row. Red skips it.");

            if (y > 6)
            {
                var divider = new Panel();
                divider.Location = new Point(18, y - 1);
                divider.Size = new Size(TwoColumnWidth - 36, 1);
                divider.BackColor = Border;
                parent.Controls.Add(divider);
            }
        }

        private void SyncLiveTuningRowControl(DataGridViewRow row)
        {
            if (row == null)
                return;

            var field = row.Tag as LiveTuningField;
            if (field == null)
                return;

            _syncingLiveTuningControls = true;
            try
            {
                var statusText = Convert.ToString(row.Cells["Address"].Value, CultureInfo.InvariantCulture) ?? "Not Found";
                var found = string.Equals(statusText, "Found", StringComparison.OrdinalIgnoreCase);

                Label status;
                if (_liveTuningStatusLabels.TryGetValue(field.Key, out status) && status != null && !status.IsDisposed)
                {
                    status.Text = "\u25CF  " + TranslateDynamicUi(statusText);
                    status.ForeColor = found ? AccentGreen : AccentRed;
                }

                Button current;
                if (_liveTuningCurrentButtons.TryGetValue(field.Key, out current) && current != null && !current.IsDisposed)
                {
                    current.Text = Convert.ToString(row.Cells["Current"].Value, CultureInfo.InvariantCulture) ?? string.Empty;
                    current.ForeColor = found ? TextPrimary : AccentRed;
                }

                Button value;
                if (_liveTuningValueButtons.TryGetValue(field.Key, out value) && value != null && !value.IsDisposed)
                {
                    var raw = Convert.ToString(row.Cells["Value"].Value, CultureInfo.InvariantCulture) ?? string.Empty;
                    value.Text = string.IsNullOrWhiteSpace(raw) ? TranslateDynamicUi("Set Value") : raw;
                }

                StatusDotToggle toggle;
                if (_liveTuningToggleControls.TryGetValue(field.Key, out toggle) && toggle != null && !toggle.IsDisposed)
                    toggle.Checked = row.Cells["Enabled"].Value is bool && (bool)row.Cells["Enabled"].Value;
            }
            finally
            {
                _syncingLiveTuningControls = false;
            }
        }

        private string ShowLiveTuningValuePrompt(LiveTuningField field, string currentValue)
        {
            if (InvokeRequired)
                return (string)Invoke(new Func<LiveTuningField, string, string>(ShowLiveTuningValuePrompt), field, currentValue);

            using (var dialog = new Form())
            {
                dialog.Text = field.DisplayName + " Value";
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.ClientSize = new Size(560, 252);
                dialog.BackColor = AppBackground;
                dialog.ForeColor = TextPrimary;
                dialog.Font = new Font("Segoe UI", 9F);

                var title = MakeLabel(field.DisplayName, 18, 16);
                title.Size = new Size(520, 26);
                title.Font = new Font("Segoe UI Semibold", 11F);
                title.BackColor = AppBackground;
                dialog.Controls.Add(title);

                var body = MakeBodyLabel(field.Description, 18, 48, 520, 54);
                body.BackColor = AppBackground;
                dialog.Controls.Add(body);

                var valueLabel = MakeLabel("Value", 18, 126);
                valueLabel.Size = new Size(120, 24);
                valueLabel.BackColor = AppBackground;
                dialog.Controls.Add(valueLabel);

                var valueBox = new TextBox();
                valueBox.Text = currentValue ?? string.Empty;
                valueBox.Location = new Point(144, 124);
                valueBox.Size = new Size(130, 28);
                valueBox.TextAlign = HorizontalAlignment.Right;
                StyleTextBox(valueBox);
                dialog.Controls.Add(valueBox);

                var hint = MakeBodyLabel(
                    "Allowed range: " + FormatFloat(field.SaneMin) + " to " + FormatFloat(field.SaneMax) + ".",
                    18, 170, 520, 24);
                hint.BackColor = AppBackground;
                dialog.Controls.Add(hint);

                string result = null;
                var apply = MakeButton("Use Value", 350, 206, 96, 30);
                MakeAccentButton(apply, AccentBlue);
                apply.Click += delegate
                {
                    try
                    {
                        var parsed = ParseFloat(valueBox.Text, field.DisplayName);
                        ValidateLiveTuningInputValue(field, parsed);
                        result = FormatFloat(parsed);
                        dialog.DialogResult = DialogResult.OK;
                        dialog.Close();
                    }
                    catch (Exception ex)
                    {
                        ShowTranslatedMessageBox(dialog, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                };
                dialog.Controls.Add(apply);

                var cancel = MakeButton("Cancel", 456, 206, 82, 30);
                cancel.DialogResult = DialogResult.Cancel;
                dialog.Controls.Add(cancel);

                dialog.AcceptButton = apply;
                dialog.CancelButton = cancel;
                PrepareDialogForLanguage(dialog);
                return dialog.ShowDialog(this) == DialogResult.OK ? result : null;
            }
        }

        private DataGridView BuildTuningGrid()
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
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = SurfaceAlt;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F);
            grid.DefaultCellStyle.BackColor = Surface;
            grid.DefaultCellStyle.ForeColor = TextPrimary;
            grid.DefaultCellStyle.SelectionBackColor = AccentBlueSoft;
            grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(28, 34, 46);
            grid.Font = new Font("Segoe UI", 9F);
            grid.RowTemplate.Height = 34;
            grid.ColumnHeadersHeight = 34;
            grid.ScrollBars = ScrollBars.Vertical;

            grid.Columns.Add("Category", "Category");
            grid.Columns.Add("Setting", "Setting");
            grid.Columns.Add("Address", "Status");
            grid.Columns.Add("Current", "Current");
            grid.Columns.Add("Value", "Value");
            grid.Columns.Add("Description", "What it does");
            var enabledColumn = new DataGridViewTextBoxColumn();
            enabledColumn.Name = "Enabled";
            enabledColumn.HeaderText = "Toggle";
            grid.Columns.Add(enabledColumn);

            grid.Columns["Category"].FillWeight = 14F;
            grid.Columns["Setting"].FillWeight = 18F;
            grid.Columns["Address"].FillWeight = 15F;
            grid.Columns["Current"].FillWeight = 12F;
            grid.Columns["Value"].FillWeight = 12F;
            grid.Columns["Description"].FillWeight = 22F;
            grid.Columns["Enabled"].FillWeight = 7F;
            grid.Columns["Category"].ReadOnly = true;
            grid.Columns["Setting"].ReadOnly = true;
            grid.Columns["Address"].ReadOnly = true;
            grid.Columns["Current"].ReadOnly = true;
            grid.Columns["Description"].ReadOnly = true;
            grid.Columns["Value"].ReadOnly = false;
            grid.Columns["Enabled"].ReadOnly = true;

            grid.CellFormatting += delegate(object sender, DataGridViewCellFormattingEventArgs e)
            {
                if (e.RowIndex < 0 || e.ColumnIndex != grid.Columns["Enabled"].Index)
                    return;

                var on = e.Value is bool && (bool)e.Value;
                e.Value = "\u25CF";
                e.CellStyle.ForeColor = on ? AccentGreen : AccentRed;
                e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
                e.CellStyle.Font = ToggleDotFont;
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                e.FormattingApplied = true;
            };
            grid.CellClick += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex < 0 || e.ColumnIndex != grid.Columns["Enabled"].Index)
                    return;

                var cell = grid.Rows[e.RowIndex].Cells["Enabled"];
                var on = cell.Value is bool && (bool)cell.Value;
                cell.Value = !on;
                grid.InvalidateCell(cell);
            };

            foreach (var field in LiveTuningFields)
            {
                var rowIndex = grid.Rows.Add(field.Category, field.DisplayName, "Not Found", "Load", "", field.Description, false);
                var row = grid.Rows[rowIndex];
                row.Tag = field;
                row.Cells["Setting"].ToolTipText = field.Description;
                row.Cells["Address"].ToolTipText = "Found means Luna can read and write this live tuning row.";
                row.Cells["Current"].ToolTipText = "Current value read from the attached game.";
                row.Cells["Value"].ToolTipText = "Type the value to write, then turn the dot green.";
                row.Cells["Enabled"].ToolTipText = "Green applies this row. Red skips it.";
                StyleLiveTuningRow(row, "Not scanned");
            }

            return grid;
        }

        private void BuildGarageFavoritesPage()
        {
            _garageFavoritesPage.AutoScroll = true;
            AddPageHeader(_garageFavoritesPage, "Garage Favorites", "Load your garage, select cars, then favorite or unfavorite them.");

            var status = new Label();
            status.Text = "Ready";
            status.Font = new Font("Segoe UI Semibold", 9F);
            status.ForeColor = AccentBlue;
            status.BackColor = AppBackground;
            status.Location = new Point(690, 12);
            status.Size = new Size(210, 24);
            status.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            status.TextAlign = ContentAlignment.MiddleRight;
            _garageFavoritesPage.Controls.Add(status);

            var actions = MakeCard(_garageFavoritesPage, 0, 72, ContentWidth, 206, "Favorite Actions", "Selected cars use the in-game favorite flag saved on the garage row.");
            actions.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var actionPanel = new Panel();
            actionPanel.Location = new Point(18, 58);
            actionPanel.Size = new Size(858, 132);
            actionPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            actionPanel.BackColor = Surface;
            actions.Controls.Add(actionPanel);

            var back = MakeButton("Back", 0, 0, 92, 38);
            back.Click += delegate { ShowProfilePage(); };
            actionPanel.Controls.Add(back);

            var load = MakeButton("Load Garage", 108, 0, 132, 38);
            MakeAccentButton(load, AccentBlue);
            load.Click += delegate { RunWorker("Load Garage Favorites", LoadGarageFavorites, load); };
            actionPanel.Controls.Add(load);
            SetTranslatedToolTip(load, "Reads your owned cars and shows which ones are currently favorites.");

            var add = MakeButton("Favorite Selected", 256, 0, 154, 38);
            MakeAccentButton(add, AccentGreen);
            add.Click += delegate
            {
                var ids = GetSelectedDatabaseCarIds(_garageFavoritesGrid);
                RunWorker("Favorite Selected Cars", delegate { SetSelectedGarageFavorites(ids, true); }, add);
            };
            actionPanel.Controls.Add(add);

            var remove = MakeButton("Unfavorite Selected", 426, 0, 162, 38);
            remove.Click += delegate
            {
                var ids = GetSelectedDatabaseCarIds(_garageFavoritesGrid);
                RunWorker("Unfavorite Selected Cars", delegate { SetSelectedGarageFavorites(ids, false); }, remove);
            };
            actionPanel.Controls.Add(remove);

            var currentRemove = MakeButton("Unfavorite Current", 604, 0, 154, 38);
            currentRemove.Click += delegate
            {
                var ids = GetSelectedDatabaseCarIds(_garageFavoritesCurrentGrid);
                RunWorker("Unfavorite Current Cars", delegate { SetSelectedGarageFavorites(ids, false); }, currentRemove);
            };
            actionPanel.Controls.Add(currentRemove);

            var clear = MakeButton("Remove All Favorites", 0, 46, 166, 36);
            clear.Click += delegate { RunWorker("Remove All Favorites", RemoveAllGarageFavorites, clear); };
            actionPanel.Controls.Add(clear);

            _garageFavoritesSelected = MakeBodyLabel("Selected: none", 0, 96, 858, 26);
            _garageFavoritesSelected.BackColor = Surface;
            actionPanel.Controls.Add(_garageFavoritesSelected);

            var currentCard = MakeCard(_garageFavoritesPage, 0, 298, ContentWidth, 210, "Current Favorites", "Cars currently marked favorite in your garage.");
            currentCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            _garageFavoritesCurrentGrid = BuildDatabaseCarGrid("Favorite");
            _garageFavoritesCurrentGrid.Location = new Point(18, 56);
            _garageFavoritesCurrentGrid.Size = new Size(858, 136);
            WireDatabaseCarSelectionLabel(_garageFavoritesCurrentGrid, _garageFavoritesSelected);
            currentCard.Controls.Add(_garageFavoritesCurrentGrid);

            var garageCard = MakeCard(_garageFavoritesPage, 0, 528, ContentWidth, 470, "Owned Garage Cars", "Search, select one or more rows, then press the favorite button you want.");
            garageCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            _garageFavoritesSearch = new TextBox();
            _garageFavoritesSearch.Location = new Point(18, 50);
            _garageFavoritesSearch.Size = new Size(858, 28);
            _garageFavoritesSearch.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            StyleTextBox(_garageFavoritesSearch);
            _garageFavoritesSearch.TextChanged += delegate { ApplyGarageFavoritesFilter(); };
            garageCard.Controls.Add(_garageFavoritesSearch);

            _garageFavoritesGrid = BuildDatabaseCarGrid("Favorite");
            _garageFavoritesGrid.Location = new Point(18, 88);
            _garageFavoritesGrid.Size = new Size(858, 364);
            WireDatabaseCarSelectionLabel(_garageFavoritesGrid, _garageFavoritesSelected);
            garageCard.Controls.Add(_garageFavoritesGrid);
        }

        private void BuildTrafficEditorPage()
        {
            _trafficEditorPage.AutoScroll = true;
            AddPageHeader(_trafficEditorPage, "Traffic Car Editor", "Choose which cars can appear in traffic lists.");

            var status = new Label();
            status.Text = "Ready";
            status.Font = new Font("Segoe UI Semibold", 9F);
            status.ForeColor = AccentBlue;
            status.BackColor = AppBackground;
            status.Location = new Point(690, 12);
            status.Size = new Size(210, 24);
            status.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            status.TextAlign = ContentAlignment.MiddleRight;
            _trafficEditorPage.Controls.Add(status);

            var actions = MakeCard(_trafficEditorPage, 0, 72, ContentWidth, 206, "Traffic Actions", "TrafficCars is backed up before Luna edits the traffic list.");
            actions.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var actionPanel = new Panel();
            actionPanel.Location = new Point(18, 58);
            actionPanel.Size = new Size(858, 132);
            actionPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            actionPanel.BackColor = Surface;
            actions.Controls.Add(actionPanel);

            var back = MakeButton("Back", 0, 0, 92, 36);
            back.Click += delegate { ShowProfilePage(); };
            actionPanel.Controls.Add(back);

            var load = MakeButton("Load Cars", 108, 0, 116, 36);
            MakeAccentButton(load, AccentBlue);
            load.Click += delegate { RunWorker("Load Traffic Cars", LoadTrafficEditorCars, load); };
            actionPanel.Controls.Add(load);

            var add = MakeButton("Add Selected", 240, 0, 128, 36);
            MakeAccentButton(add, AccentGreen);
            add.Click += delegate
            {
                var ids = GetSelectedDatabaseCarIds(_trafficEditorGrid);
                RunWorker("Add Traffic Cars", delegate { SetSelectedTrafficCars(ids, true); }, add);
            };
            actionPanel.Controls.Add(add);

            var remove = MakeButton("Remove Selected", 384, 0, 144, 36);
            remove.Click += delegate
            {
                var ids = GetSelectedDatabaseCarIds(_trafficEditorGrid);
                RunWorker("Remove Traffic Cars", delegate { SetSelectedTrafficCars(ids, false); }, remove);
            };
            actionPanel.Controls.Add(remove);

            var clear = MakeButton("Clear All", 544, 0, 98, 36);
            clear.Click += delegate { RunWorker("Clear Traffic Cars", ClearTrafficCars, clear); };
            actionPanel.Controls.Add(clear);

            var removeCurrent = MakeButton("Remove Current Selected", 0, 46, 190, 36);
            removeCurrent.Click += delegate
            {
                var ids = GetSelectedDatabaseCarIds(_trafficCurrentGrid);
                RunWorker("Remove Current Traffic Cars", delegate { SetSelectedTrafficCars(ids, false); }, removeCurrent);
            };
            actionPanel.Controls.Add(removeCurrent);

            var drive = MakeButton("Make Drivable", 206, 46, 132, 36);
            drive.Click += delegate { RunWorker("Drive Traffic Cars", DriveTrafficCars, drive); };
            actionPanel.Controls.Add(drive);

            var hint = MakeBodyLabel("Check rows in either table. Add or remove selected cars, then reload FH6 traffic.", 354, 40, 504, 50);
            hint.BackColor = Surface;
            actionPanel.Controls.Add(hint);

            _trafficEditorSelected = MakeBodyLabel("Selected: none", 0, 96, 858, 26);
            _trafficEditorSelected.BackColor = Surface;
            actionPanel.Controls.Add(_trafficEditorSelected);

            var currentCard = MakeCard(_trafficEditorPage, 0, 298, ContentWidth, 210, "Current Traffic Cars", "Cars currently stored in TrafficCars.");
            currentCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            _trafficCurrentGrid = BuildDatabaseCarGrid("Traffic");
            _trafficCurrentGrid.Location = new Point(18, 56);
            _trafficCurrentGrid.Size = new Size(858, 136);
            WireDatabaseCarSelectionLabel(_trafficCurrentGrid, _trafficEditorSelected);
            currentCard.Controls.Add(_trafficCurrentGrid);

            var gridCard = MakeCard(_trafficEditorPage, 0, 528, ContentWidth, 526, "Car List", "Search, select one or more rows, then edit the traffic list.");
            gridCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            _trafficEditorSearch = new TextBox();
            _trafficEditorSearch.Location = new Point(18, 50);
            _trafficEditorSearch.Size = new Size(858, 28);
            _trafficEditorSearch.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            StyleTextBox(_trafficEditorSearch);
            _trafficEditorSearch.TextChanged += delegate { ApplyTrafficEditorFilter(); };
            gridCard.Controls.Add(_trafficEditorSearch);

            _trafficEditorGrid = BuildDatabaseCarGrid("Traffic");
            _trafficEditorGrid.Location = new Point(18, 88);
            _trafficEditorGrid.Size = new Size(858, 420);
            WireDatabaseCarSelectionLabel(_trafficEditorGrid, _trafficEditorSelected);
            gridCard.Controls.Add(_trafficEditorGrid);
        }

        private void BuildProfileCosmeticsPage()
        {
            BuildGenericDatabaseEditorPage(
                _profileCosmeticsPage,
                "Player / Profile Cosmetics",
                "Browse and edit player names, character headshots, team colors, and AI player colors.",
                _profileCosmeticsTableCombo = new ComboBox(),
                out _profileCosmeticsSearch,
                out _profileCosmeticsGrid,
                GetProfileCosmeticTableNames(),
                LoadProfileCosmeticsRows,
                ApplySelectedProfileCosmeticsRows,
                ApplyAllProfileCosmeticsRows,
                RestoreProfileCosmeticsRows);
            _profileCosmeticsSearch.TextChanged += delegate { ApplyProfileCosmeticsFilter(); };
        }

        private void BuildAiBehaviorPage()
        {
            BuildGenericDatabaseEditorPage(
                _aiBehaviorPage,
                "AI Behavior Editor",
                "Browse and edit AI temperament, mistake, observation, and racing-line tables.",
                _aiBehaviorTableCombo = new ComboBox(),
                out _aiBehaviorSearch,
                out _aiBehaviorGrid,
                GetAiBehaviorTableNames(),
                LoadAiBehaviorRows,
                ApplySelectedAiBehaviorRows,
                ApplyAllAiBehaviorRows,
                RestoreAiBehaviorRows);
            _aiBehaviorSearch.TextChanged += delegate { ApplyAiBehaviorFilter(); };
        }

        private void BuildGenericDatabaseEditorPage(
            Panel page,
            string title,
            string subtitle,
            ComboBox tableCombo,
            out TextBox searchBox,
            out DataGridView grid,
            string[] tableNames,
            Action loadAction,
            Action applySelectedAction,
            Action applyAllAction,
            Action restoreAction)
        {
            page.AutoScroll = true;
            AddPageHeader(page, title, subtitle);

            var status = new Label();
            status.Text = "Ready";
            status.Font = new Font("Segoe UI Semibold", 9F);
            status.ForeColor = AccentBlue;
            status.BackColor = AppBackground;
            status.Location = new Point(690, 12);
            status.Size = new Size(210, 24);
            status.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            status.TextAlign = ContentAlignment.MiddleRight;
            page.Controls.Add(status);

            var actions = MakeCard(page, 0, 72, ContentWidth, 206, title + " Actions", "Load a table, edit New Value, then apply checked rows.");
            actions.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var actionPanel = new Panel();
            actionPanel.Location = new Point(18, 58);
            actionPanel.Size = new Size(858, 132);
            actionPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            actionPanel.BackColor = Surface;
            actions.Controls.Add(actionPanel);

            var back = MakeButton("Back", 0, 0, 92, 36);
            back.Click += delegate { ShowProfilePage(); };
            actionPanel.Controls.Add(back);

            tableCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            tableCombo.Location = new Point(108, 2);
            tableCombo.Size = new Size(230, 30);
            tableCombo.BackColor = SurfaceAlt;
            tableCombo.ForeColor = TextPrimary;
            tableCombo.FlatStyle = FlatStyle.Flat;
            tableCombo.Items.Add("All Supported");
            foreach (var tableName in tableNames)
                tableCombo.Items.Add(tableName);
            tableCombo.SelectedIndex = 0;
            actionPanel.Controls.Add(tableCombo);

            var load = MakeButton("Load Table", 354, 0, 118, 36);
            MakeAccentButton(load, AccentBlue);
            load.Click += delegate { RunWorker("Load " + title, loadAction, load); };
            actionPanel.Controls.Add(load);

            var applySelected = MakeButton("Apply Selected", 488, 0, 140, 36);
            MakeAccentButton(applySelected, AccentGreen);
            applySelected.Click += delegate { RunWorker("Apply " + title, applySelectedAction, applySelected); };
            actionPanel.Controls.Add(applySelected);

            var applyAll = MakeButton("Apply All", 644, 0, 98, 36);
            applyAll.Click += delegate { RunWorker("Apply All " + title, applyAllAction, applyAll); };
            actionPanel.Controls.Add(applyAll);

            var restore = MakeButton("Restore", 758, 0, 100, 36);
            restore.Click += delegate { RunWorker("Restore " + title, restoreAction, restore); };
            actionPanel.Controls.Add(restore);

            searchBox = new TextBox();
            searchBox.Location = new Point(0, 52);
            searchBox.Size = new Size(858, 28);
            searchBox.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            StyleTextBox(searchBox);
            actionPanel.Controls.Add(searchBox);

            var hint = MakeBodyLabel("Checked rows apply. Leave New Value equal to Current to skip. Restore uses Luna's backup for these tables.", 0, 92, 858, 30);
            hint.BackColor = Surface;
            hint.ForeColor = TextMuted;
            actionPanel.Controls.Add(hint);

            var gridCard = MakeCard(page, 0, 298, ContentWidth, 620, title + " Table", "Rows are grouped by table row and editable field.");
            gridCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            grid = BuildGenericDatabaseEditGrid();
            grid.Location = new Point(18, 56);
            grid.Size = new Size(858, 546);
            gridCard.Controls.Add(grid);
        }

        private static DataGridView BuildGenericDatabaseEditGrid()
        {
            var grid = BuildReadOnlyGrid();
            grid.ReadOnly = false;
            grid.MultiSelect = true;
            grid.Columns.Clear();

            var pick = new DataGridViewCheckBoxColumn();
            pick.Name = "Pick";
            pick.HeaderText = "All";
            pick.FalseValue = false;
            pick.TrueValue = true;
            pick.Width = 48;
            pick.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            pick.SortMode = DataGridViewColumnSortMode.NotSortable;
            grid.Columns.Add(pick);
            grid.Columns.Add("Table", "Table");
            grid.Columns.Add("Row", "Row");
            grid.Columns.Add("Field", "Field");
            grid.Columns.Add("Current", "Current");
            grid.Columns.Add("NewValue", "New Value");
            grid.Columns.Add("Info", "Info");

            foreach (DataGridViewColumn column in grid.Columns)
                column.ReadOnly = true;
            grid.Columns["Pick"].ReadOnly = false;
            grid.Columns["NewValue"].ReadOnly = false;
            grid.Columns["Pick"].FillWeight = 5F;
            grid.Columns["Table"].FillWeight = 16F;
            grid.Columns["Row"].FillWeight = 16F;
            grid.Columns["Field"].FillWeight = 18F;
            grid.Columns["Current"].FillWeight = 16F;
            grid.Columns["NewValue"].FillWeight = 16F;
            grid.Columns["Info"].FillWeight = 13F;
            grid.ColumnHeaderMouseClick += delegate(object sender, DataGridViewCellMouseEventArgs e)
            {
                if (e.ColumnIndex >= 0 && string.Equals(grid.Columns[e.ColumnIndex].Name, "Pick", StringComparison.Ordinal))
                    ToggleVisibleDatabaseGridChecks(grid);
            };
            grid.CellPainting += delegate(object sender, DataGridViewCellPaintingEventArgs e)
            {
                if (e.RowIndex == -1 &&
                    e.ColumnIndex >= 0 &&
                    string.Equals(grid.Columns[e.ColumnIndex].Name, "Pick", StringComparison.Ordinal))
                    PaintSelectAllHeader(e);
            };
            grid.CurrentCellDirtyStateChanged += delegate
            {
                if (grid.CurrentCell != null &&
                    grid.CurrentCell.OwningColumn != null &&
                    string.Equals(grid.CurrentCell.OwningColumn.Name, "Pick", StringComparison.Ordinal) &&
                    grid.IsCurrentCellDirty)
                    grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            return grid;
        }

        private void LoadProfileCosmeticsRows()
        {
            var db = RequireDatabase();
            var rows = QueryGenericDatabaseEditRows(db, GetSelectedGenericTableNames(_profileCosmeticsTableCombo, GetProfileCosmeticTableNames()));
            _profileCosmeticRows = rows ?? new List<GenericDatabaseEditRow>();
            SetGenericDatabaseRows(_profileCosmeticsGrid, _profileCosmeticRows, GetTextBoxValue(_profileCosmeticsSearch));
            SetProfileStatus("Profile cosmetics " + rows.Count.ToString(CultureInfo.InvariantCulture));
            Log("Profile Cosmetics loaded " + rows.Count.ToString(CultureInfo.InvariantCulture) + " editable row(s).");
        }

        private void LoadAiBehaviorRows()
        {
            var db = RequireDatabase();
            var rows = QueryGenericDatabaseEditRows(db, GetSelectedGenericTableNames(_aiBehaviorTableCombo, GetAiBehaviorTableNames()));
            _aiBehaviorRows = rows ?? new List<GenericDatabaseEditRow>();
            SetGenericDatabaseRows(_aiBehaviorGrid, _aiBehaviorRows, GetTextBoxValue(_aiBehaviorSearch));
            SetProfileStatus("AI behavior " + rows.Count.ToString(CultureInfo.InvariantCulture));
            Log("AI Behavior Editor loaded " + rows.Count.ToString(CultureInfo.InvariantCulture) + " editable row(s).");
        }

        private void ApplyProfileCosmeticsFilter()
        {
            SetGenericDatabaseRows(_profileCosmeticsGrid, _profileCosmeticRows, GetTextBoxValue(_profileCosmeticsSearch));
        }

        private void ApplyAiBehaviorFilter()
        {
            SetGenericDatabaseRows(_aiBehaviorGrid, _aiBehaviorRows, GetTextBoxValue(_aiBehaviorSearch));
        }

        private void ApplySelectedProfileCosmeticsRows()
        {
            ApplyGenericDatabaseRows(_profileCosmeticsGrid, false);
            LoadProfileCosmeticsRows();
        }

        private void ApplyAllProfileCosmeticsRows()
        {
            ApplyGenericDatabaseRows(_profileCosmeticsGrid, true);
            LoadProfileCosmeticsRows();
        }

        private void ApplySelectedAiBehaviorRows()
        {
            ApplyGenericDatabaseRows(_aiBehaviorGrid, false);
            LoadAiBehaviorRows();
        }

        private void ApplyAllAiBehaviorRows()
        {
            ApplyGenericDatabaseRows(_aiBehaviorGrid, true);
            LoadAiBehaviorRows();
        }

        private void RestoreProfileCosmeticsRows()
        {
            RestoreGenericDatabaseEditorTables(GetProfileCosmeticTableNames(), "_backup_Database_Generic_");
            LoadProfileCosmeticsRows();
        }

        private void RestoreAiBehaviorRows()
        {
            RestoreGenericDatabaseEditorTables(GetAiBehaviorTableNames(), "_backup_Database_Generic_");
            LoadAiBehaviorRows();
        }

        private void SetGenericDatabaseRows(DataGridView grid, List<GenericDatabaseEditRow> rows, string filter)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(delegate { SetGenericDatabaseRows(grid, rows, filter); }));
                return;
            }

            rows = rows ?? new List<GenericDatabaseEditRow>();
            if (grid == null)
                return;

            grid.Rows.Clear();
            foreach (var row in rows.Where(item => item != null && item.Matches(filter ?? string.Empty)))
            {
                var index = grid.Rows.Add(row.Enabled, row.TableName, row.RowLabel, row.ColumnName, row.CurrentValue, row.NewValue, row.Description);
                grid.Rows[index].Tag = row;
                if (row.Numeric)
                    grid.Rows[index].Cells["NewValue"].ToolTipText = "Numeric value. Use a finite number.";
                else
                    grid.Rows[index].Cells["NewValue"].ToolTipText = "Text value. Type the exact text to write.";
            }
        }

        private void ApplyGenericDatabaseRows(DataGridView grid, bool allVisible)
        {
            if (grid == null)
                throw new InvalidOperationException("Load rows first.");
            if (grid.InvokeRequired)
            {
                grid.Invoke((Action)(delegate { grid.EndEdit(); }));
            }
            else
            {
                grid.EndEdit();
            }

            var edits = ReadGenericDatabaseEditsFromGrid(grid, allVisible);
            if (edits.Count == 0)
                throw new InvalidOperationException(allVisible ? "No visible changed rows to apply." : "Check or select one or more changed rows first.");

            var db = RequireDatabase();
            var touched = 0;
            foreach (var edit in edits)
            {
                if (edit == null || !db.TableExists(edit.TableName) || !db.ColumnExists(edit.TableName, edit.ColumnName))
                    continue;

                var backupName = "_backup_Database_Generic_" + SanitizeSqlName(edit.TableName);
                var tableSql = EscapeIdentifierForSql(edit.TableName);
                var backupSql = EscapeIdentifierForSql(backupName);
                var columnSql = EscapeIdentifierForSql(edit.ColumnName);
                var literal = BuildGenericDatabaseSqlLiteral(edit);
                ExecuteAutoshowSql(db, "CREATE TABLE IF NOT EXISTS " + backupSql + " AS SELECT rowid AS _rowid_, * FROM " + tableSql + ";");
                ExecuteAutoshowSql(db, "UPDATE " + tableSql + " SET " + columnSql + " = " + literal + " WHERE rowid = " + edit.RowId.ToString(CultureInfo.InvariantCulture) + ";");
                touched++;
            }

            if (touched == 0)
                throw new InvalidOperationException("No supported rows were applied.");

            Log("Generic database editor applied " + touched.ToString(CultureInfo.InvariantCulture) + " row value(s).");
            ShowInfo("Applied " + touched.ToString(CultureInfo.InvariantCulture) + " database value(s)." + Environment.NewLine + Environment.NewLine + "Please switch menus/cars or reload the affected screen if FH6 has cached the values.");
        }

        private List<GenericDatabaseEditRow> ReadGenericDatabaseEditsFromGrid(DataGridView grid, bool allVisible)
        {
            if (grid.InvokeRequired)
                return (List<GenericDatabaseEditRow>)grid.Invoke(new Func<DataGridView, bool, List<GenericDatabaseEditRow>>(ReadGenericDatabaseEditsFromGrid), grid, allVisible);

            var edits = new List<GenericDatabaseEditRow>();
            foreach (DataGridViewRow viewRow in grid.Rows)
            {
                if (viewRow == null || viewRow.IsNewRow || !viewRow.Visible)
                    continue;

                var edit = viewRow.Tag as GenericDatabaseEditRow;
                if (edit == null)
                    continue;

                var picked = viewRow.Cells["Pick"].Value is bool && (bool)viewRow.Cells["Pick"].Value;
                var selected = viewRow.Selected;
                if (!allVisible && !picked && !selected)
                    continue;

                var newValue = Convert.ToString(viewRow.Cells["NewValue"].Value, CultureInfo.InvariantCulture) ?? string.Empty;
                if (string.Equals(newValue, edit.CurrentValue, StringComparison.Ordinal))
                    continue;

                edit.NewValue = newValue;
                edit.Enabled = true;
                edits.Add(edit);
            }
            return edits;
        }

        private void RestoreGenericDatabaseEditorTables(IEnumerable<string> tableNames, string backupPrefix)
        {
            var db = RequireDatabase();
            var restored = 0;
            foreach (var tableName in tableNames ?? new string[0])
            {
                if (!db.TableExists(tableName))
                    continue;
                var backupName = (backupPrefix ?? string.Empty) + SanitizeSqlName(tableName);
                if (!db.TableExists(backupName))
                    continue;

                RestoreGenericFullTableBackup(db, tableName, backupName);
                restored++;
            }

            if (restored == 0)
                throw new InvalidOperationException("No Luna backup was found for these tables.");
            Log("Generic database editor restored " + restored.ToString(CultureInfo.InvariantCulture) + " table backup(s).");
        }

        private List<GenericDatabaseEditRow> QueryGenericDatabaseEditRows(RemoteDatabase db, IEnumerable<string> requestedTables)
        {
            var rows = new List<GenericDatabaseEditRow>();
            foreach (var tableName in requestedTables ?? new string[0])
            {
                if (string.IsNullOrWhiteSpace(tableName) || !db.TableExists(tableName))
                    continue;

                var columns = QueryGenericDatabaseColumnInfos(db, tableName);
                if (columns.Count == 0)
                    continue;

                var editableColumns = columns.Where(column => !column.IsPrimaryKey && !IsGenericReadonlyColumn(column.Name)).ToList();
                if (editableColumns.Count == 0)
                    continue;

                var tableSql = EscapeIdentifierForSql(tableName);
                var selectColumns = string.Join(", ", columns.Select(column => EscapeIdentifierForSql(column.Name)).ToArray());
                var result = db.Query("SELECT rowid, " + selectColumns + " FROM " + tableSql + " LIMIT 5000;");
                foreach (var resultRow in result.Rows)
                {
                    if (resultRow.Count < 1)
                        continue;

                    var rowId = ToLong(resultRow[0]);
                    if (rowId <= 0)
                        continue;

                    var rowLabel = BuildGenericDatabaseRowLabel(columns, resultRow);
                    foreach (var column in editableColumns)
                    {
                        var index = columns.FindIndex(item => string.Equals(item.Name, column.Name, StringComparison.OrdinalIgnoreCase));
                        if (index < 0 || resultRow.Count <= index + 1)
                            continue;

                        var value = ToDisplayString(resultRow[index + 1]);
                        rows.Add(new GenericDatabaseEditRow(
                            tableName,
                            rowId,
                            rowLabel,
                            column.Name,
                            column.Type,
                            value,
                            BuildGenericDatabaseDescription(tableName, column.Name),
                            IsGenericNumericColumn(column.Type, value)));
                    }
                }
            }
            return rows;
        }

        private static List<GenericDatabaseColumnInfo> QueryGenericDatabaseColumnInfos(RemoteDatabase db, string tableName)
        {
            var list = new List<GenericDatabaseColumnInfo>();
            var result = db.Query("PRAGMA table_info(" + EscapeIdentifierForSql(tableName) + ")");
            foreach (var row in result.Rows)
            {
                if (row.Count < 6)
                    continue;
                var name = Convert.ToString(row[1], CultureInfo.InvariantCulture);
                if (string.IsNullOrWhiteSpace(name))
                    continue;
                list.Add(new GenericDatabaseColumnInfo(
                    name,
                    Convert.ToString(row[2], CultureInfo.InvariantCulture),
                    ToLong(row[5]) > 0));
            }
            return list;
        }

        private static string BuildGenericDatabaseRowLabel(List<GenericDatabaseColumnInfo> columns, List<object> row)
        {
            var parts = new List<string>();
            for (var i = 0; i < columns.Count && i + 1 < row.Count; i++)
            {
                var column = columns[i];
                if (!column.IsPrimaryKey && parts.Count > 0)
                    continue;
                var value = ToDisplayString(row[i + 1]);
                if (!string.IsNullOrWhiteSpace(value))
                    parts.Add(column.Name + "=" + value);
            }

            if (parts.Count == 0 && row.Count > 1)
                parts.Add("rowid=" + ToDisplayString(row[0]));
            return string.Join(", ", parts.Take(3).ToArray());
        }

        private static string BuildGenericDatabaseSqlLiteral(GenericDatabaseEditRow row)
        {
            var value = row.NewValue ?? string.Empty;
            if (string.Equals(value.Trim(), "NULL", StringComparison.OrdinalIgnoreCase))
                return "NULL";

            if (row.Numeric)
            {
                double numeric;
                if (!double.TryParse(value.Replace(",", string.Empty).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out numeric) ||
                    double.IsNaN(numeric) ||
                    double.IsInfinity(numeric))
                    throw new InvalidOperationException(row.TableName + "." + row.ColumnName + " needs a finite number.");

                var type = (row.ColumnType ?? string.Empty).ToUpperInvariant();
                if (type.Contains("INT"))
                    return ((long)Math.Round(numeric)).ToString(CultureInfo.InvariantCulture);
                return numeric.ToString("0.########", CultureInfo.InvariantCulture);
            }

            if (value.Length > 512)
                throw new InvalidOperationException(row.TableName + "." + row.ColumnName + " text is too long.");
            return ToSqlText(value);
        }

        private static string BuildGenericDatabaseDescription(string tableName, string columnName)
        {
            if (string.Equals(tableName, "PlayerNames", StringComparison.OrdinalIgnoreCase))
                return "Player name browser field.";
            if (string.Equals(tableName, "PlayerCharacters", StringComparison.OrdinalIgnoreCase))
                return "Character/headshot selector field.";
            if (string.Equals(tableName, "TeamColors", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(tableName, "AIPlayerColors", StringComparison.OrdinalIgnoreCase))
                return "Color or material field.";
            if ((tableName ?? string.Empty).StartsWith("AI", StringComparison.OrdinalIgnoreCase))
                return "AI behavior tuning field.";
            return columnName;
        }

        private static bool IsGenericNumericColumn(string columnType, string currentValue)
        {
            var type = (columnType ?? string.Empty).ToUpperInvariant();
            if (type.Contains("INT") || type.Contains("REAL") || type.Contains("FLOA") || type.Contains("DOUB") || type.Contains("NUM"))
                return true;
            double ignored;
            return !string.IsNullOrWhiteSpace(currentValue) &&
                double.TryParse(currentValue, NumberStyles.Float, CultureInfo.InvariantCulture, out ignored);
        }

        private static bool IsGenericReadonlyColumn(string columnName)
        {
            return string.Equals(columnName, "rowid", StringComparison.OrdinalIgnoreCase);
        }

        private static string[] GetSelectedGenericTableNames(ComboBox combo, string[] allTables)
        {
            if (combo != null && combo.InvokeRequired)
                return (string[])combo.Invoke(new Func<ComboBox, string[], string[]>(GetSelectedGenericTableNames), combo, allTables);
            if (combo == null || combo.SelectedIndex <= 0)
                return allTables;
            var selected = Convert.ToString(combo.SelectedItem, CultureInfo.InvariantCulture);
            return string.IsNullOrWhiteSpace(selected) ? allTables : new[] { selected };
        }

        private static string[] GetProfileCosmeticTableNames()
        {
            return new[]
            {
                "PlayerCharacters",
                "PlayerNames",
                "TeamColors",
                "AIPlayerColors"
            };
        }

        private static string[] GetAiBehaviorTableNames()
        {
            return new[]
            {
                "AITemperaments",
                "AIMistakeScales_CarClass",
                "AIMistakeScales_TurnType",
                "AIDrivingBehaviorObservationDefaults",
                "AIDrivingBehaviorObservationModelTR_Beliefs",
                "AIDrivingBehaviorObservationModelTR_Observations",
                "AIDrivingBehaviorObservationModel_Beliefs",
                "AIDrivingBehaviorObservationModel_Observations",
                "AILineChoices"
            };
        }

        private sealed class GenericDatabaseColumnInfo
        {
            public readonly string Name;
            public readonly string Type;
            public readonly bool IsPrimaryKey;

            public GenericDatabaseColumnInfo(string name, string type, bool isPrimaryKey)
            {
                Name = name ?? string.Empty;
                Type = type ?? string.Empty;
                IsPrimaryKey = isPrimaryKey;
            }
        }

        private void BuildPhotoCapturePage()
        {
            _photoCapturePage.AutoScroll = true;
            AddPageHeader(_photoCapturePage, "Photo Capture", "Check Horizon Promo captures and mark missing cars as photographed.");

            var status = new Label();
            status.Text = "Ready";
            status.Font = new Font("Segoe UI Semibold", 9F);
            status.ForeColor = AccentBlue;
            status.BackColor = AppBackground;
            status.Location = new Point(690, 12);
            status.Size = new Size(210, 24);
            status.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            status.TextAlign = ContentAlignment.MiddleRight;
            _photoCapturePage.Controls.Add(status);

            var actions = MakeCard(_photoCapturePage, 0, 72, ContentWidth, 126, "Photo Actions", "Load captures to verify, or capture all supported cars.");
            actions.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var actionPanel = new Panel();
            actionPanel.Location = new Point(18, 58);
            actionPanel.Size = new Size(858, 52);
            actionPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            actionPanel.BackColor = Surface;
            actions.Controls.Add(actionPanel);

            var back = MakeButton("Back", 0, 0, 92, 38);
            back.Click += delegate { ShowProfilePage(); };
            actionPanel.Controls.Add(back);

            var load = MakeButton("Load Captures", 108, 0, 136, 38);
            MakeAccentButton(load, AccentBlue);
            load.Click += delegate { RunWorker("Load Photo Captures", LoadPhotoCaptureRows, load); };
            actionPanel.Controls.Add(load);

            var capture = MakeButton("Capture All", 260, 0, 126, 38);
            MakeAccentButton(capture, AccentGreen);
            capture.Click += delegate { RunWorker("Photo Capture All Cars", PhotoCaptureAllCars, capture); };
            actionPanel.Controls.Add(capture);

            _photoCaptureSearch = new TextBox();
            _photoCaptureSearch.Location = new Point(404, 5);
            _photoCaptureSearch.Size = new Size(454, 28);
            _photoCaptureSearch.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            StyleTextBox(_photoCaptureSearch);
            _photoCaptureSearch.TextChanged += delegate { ApplyPhotoCaptureFilter(); };
            actionPanel.Controls.Add(_photoCaptureSearch);

            var capturedCard = MakeCard(_photoCapturePage, 0, 218, ContentWidth, 330, "Current Captured Cars", "These cars are already in the PhotoCaptures table.");
            capturedCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            _photoCapturedGrid = BuildDatabaseCarGrid("Photo");
            _photoCapturedGrid.Location = new Point(18, 56);
            _photoCapturedGrid.Size = new Size(858, 256);
            capturedCard.Controls.Add(_photoCapturedGrid);

            var missingCard = MakeCard(_photoCapturePage, 0, 568, ContentWidth, 330, "Missing Captures", "These cars are not marked captured yet.");
            missingCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            _photoMissingGrid = BuildDatabaseCarGrid("Photo");
            _photoMissingGrid.Location = new Point(18, 56);
            _photoMissingGrid.Size = new Size(858, 256);
            missingCard.Controls.Add(_photoMissingGrid);
        }

        private void BuildWheelspinOddsPage()
        {
            _wheelspinOddsPage.AutoScroll = true;
            AddPageHeader(_wheelspinOddsPage, "Wheelspin Odds", "Load rarity rows, then turn common odds on or restore the backup.");

            var actions = MakeCard(_wheelspinOddsPage, 0, 72, ContentWidth, 166, "Odds Actions", "Green means common odds are active from Luna's verified write.");
            actions.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            var actionPanel = new Panel();
            actionPanel.Location = new Point(18, 58);
            actionPanel.Size = new Size(858, 92);
            actionPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            actionPanel.BackColor = Surface;
            actions.Controls.Add(actionPanel);

            var back = MakeButton("Back", 0, 0, 92, 38);
            back.Click += delegate { ShowProfilePage(); };
            actionPanel.Controls.Add(back);

            var load = MakeButton("Load Odds", 108, 0, 126, 38);
            MakeAccentButton(load, AccentBlue);
            load.Click += delegate { RunWorker("Load Wheelspin Odds", LoadWheelspinOddsRows, load); };
            actionPanel.Controls.Add(load);

            _wheelspinOddsSwitchButton = MakeButton("Odds OFF", 250, 0, 126, 38);
            _wheelspinOddsSwitchButton.Click += delegate { RunWorker("Toggle Wheelspin Odds", ToggleBestWheelspinOdds, _wheelspinOddsSwitchButton); };
            actionPanel.Controls.Add(_wheelspinOddsSwitchButton);
            UpdateWheelspinOddsSwitchButton();

            var selected = MakeButton("Selected Common", 392, 0, 144, 38);
            MakeAccentButton(selected, AccentGreen);
            selected.Click += delegate
            {
                var ids = GetSelectedDatabaseCarIds(_wheelspinOddsGrid);
                RunWorker("Wheelspin Selected Common", delegate { SetSelectedWheelspinOddsCommon(ids); }, selected);
            };
            actionPanel.Controls.Add(selected);

            var restore = MakeButton("Restore Backup", 552, 0, 138, 38);
            restore.Click += delegate { RunWorker("Restore Wheelspin Odds", RestoreWheelspinOddsFromPage, restore); };
            actionPanel.Controls.Add(restore);

            var addCars = MakeButton("Add All Wheelspin Cars", 706, 0, 152, 38);
            MakeAccentButton(addCars, AccentPurple);
            addCars.Click += delegate { AddWheelspinOddsCarsWithPreview(addCars); };
            actionPanel.Controls.Add(addCars);
            SetTranslatedToolTip(addCars, "Preview all wheelspin-table cars, warn about cars already owned, then queue one grant for each.");

            _wheelspinOddsSelected = MakeBodyLabel("Selected: none", 0, 54, 858, 26);
            _wheelspinOddsSelected.BackColor = Surface;
            actionPanel.Controls.Add(_wheelspinOddsSelected);

            var gridCard = MakeCard(_wheelspinOddsPage, 0, 258, ContentWidth, 596, "Rarity Table", "Normal means common. Higher rarity rows can be selected and set to common.");
            gridCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            _wheelspinOddsSearch = new TextBox();
            _wheelspinOddsSearch.Location = new Point(18, 50);
            _wheelspinOddsSearch.Size = new Size(858, 28);
            _wheelspinOddsSearch.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            StyleTextBox(_wheelspinOddsSearch);
            _wheelspinOddsSearch.TextChanged += delegate { ApplyWheelspinOddsFilter(); };
            gridCard.Controls.Add(_wheelspinOddsSearch);

            _wheelspinOddsGrid = BuildDatabaseCarGrid("Odds");
            ConfigureDatabaseCarGridHeaders(_wheelspinOddsGrid, "Car ID", "Year", "Make", "Model", "PI", "Class", "Copies", "Odds State");
            _wheelspinOddsGrid.Location = new Point(18, 88);
            _wheelspinOddsGrid.Size = new Size(858, 490);
            WireDatabaseCarSelectionLabel(_wheelspinOddsGrid, _wheelspinOddsSelected);
            gridCard.Controls.Add(_wheelspinOddsGrid);
        }

        private void BuildRoutesPage()
        {
            _routesPage.AutoScroll = true;
            AddPageHeader(_routesPage, "Routes", "Load open and locked routes, then unlock selected or all.");

            var actions = MakeCard(_routesPage, 0, 72, ContentWidth, 166, "Route Actions", "Route unlocks verify track flags, map visibility, and content gates.");
            actions.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            var actionPanel = new Panel();
            actionPanel.Location = new Point(18, 58);
            actionPanel.Size = new Size(858, 92);
            actionPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            actionPanel.BackColor = Surface;
            actions.Controls.Add(actionPanel);

            var back = MakeButton("Back", 0, 0, 92, 38);
            back.Click += delegate { ShowProfilePage(); };
            actionPanel.Controls.Add(back);
            var load = MakeButton("Load Routes", 108, 0, 126, 38);
            MakeAccentButton(load, AccentBlue);
            load.Click += delegate { RunWorker("Load Routes", LoadRouteRows, load); };
            actionPanel.Controls.Add(load);
            var unlockSelected = MakeButton("Unlock Selected", 250, 0, 146, 38);
            MakeAccentButton(unlockSelected, AccentGreen);
            unlockSelected.Click += delegate
            {
                var ids = GetSelectedDatabaseCarIds(_routesLockedGrid);
                RunWorker("Unlock Selected Routes", delegate { UnlockSelectedRoutes(ids); }, unlockSelected);
            };
            actionPanel.Controls.Add(unlockSelected);
            var unlockAll = MakeButton("Unlock All Routes", 412, 0, 146, 38);
            MakeAccentButton(unlockAll, AccentGreen);
            unlockAll.Click += delegate { RunWorker("Unlock All Routes", UnlockRoutesAndTracks, unlockAll); };
            actionPanel.Controls.Add(unlockAll);
            _routesSelected = MakeBodyLabel("Selected: none", 0, 54, 858, 26);
            _routesSelected.BackColor = Surface;
            actionPanel.Controls.Add(_routesSelected);

            _routesSearch = new TextBox();
            _routesSearch.Location = new Point(576, 5);
            _routesSearch.Size = new Size(282, 28);
            StyleTextBox(_routesSearch);
            _routesSearch.TextChanged += delegate { ApplyRoutesFilter(); };
            actionPanel.Controls.Add(_routesSearch);

            var openCard = MakeCard(_routesPage, 0, 258, ContentWidth, 300, "Current Open Routes", "Routes already marked open.");
            _routesOpenGrid = BuildDatabaseCarGrid("Route");
            ConfigureDatabaseCarGridHeaders(_routesOpenGrid, "Route ID", "Route ID", "Type", "Route Name", "Length", "Env", "Media", "Status");
            _routesOpenGrid.Location = new Point(18, 56);
            _routesOpenGrid.Size = new Size(858, 226);
            openCard.Controls.Add(_routesOpenGrid);

            var lockedCard = MakeCard(_routesPage, 0, 578, ContentWidth, 360, "Routes Not Open", "Select locked routes, then press Unlock Selected.");
            _routesLockedGrid = BuildDatabaseCarGrid("Route");
            ConfigureDatabaseCarGridHeaders(_routesLockedGrid, "Route ID", "Route ID", "Type", "Route Name", "Length", "Env", "Media", "Status");
            _routesLockedGrid.Location = new Point(18, 56);
            _routesLockedGrid.Size = new Size(858, 286);
            WireDatabaseCarSelectionLabel(_routesLockedGrid, _routesSelected);
            lockedCard.Controls.Add(_routesLockedGrid);
        }

        private void BuildRacesPage()
        {
            _racesPage.AutoScroll = true;
            AddPageHeader(_racesPage, "Races", "Load race rows and route state. Complete real progress columns when FH6 exposes them.");

            var actions = MakeCard(_racesPage, 0, 72, ContentWidth, 166, "Race Actions", "If no completion-save columns exist, Luna uses verified route unlocks instead.");
            var actionPanel = new Panel();
            actionPanel.Location = new Point(18, 58);
            actionPanel.Size = new Size(858, 92);
            actionPanel.BackColor = Surface;
            actions.Controls.Add(actionPanel);
            var back = MakeButton("Back", 0, 0, 92, 38);
            back.Click += delegate { ShowProfilePage(); };
            actionPanel.Controls.Add(back);
            var load = MakeButton("Load Races", 108, 0, 126, 38);
            MakeAccentButton(load, AccentBlue);
            load.Click += delegate { RunWorker("Load Races", LoadRaceRows, load); };
            actionPanel.Controls.Add(load);
            var completeSelected = MakeButton("Complete Selected", 250, 0, 154, 38);
            MakeAccentButton(completeSelected, AccentGreen);
            completeSelected.Click += delegate
            {
                var ids = GetSelectedDatabaseCarIds(_racesOpenGrid);
                RunWorker("Complete Selected Races", delegate { CompleteSelectedRaceRows(ids); }, completeSelected);
            };
            actionPanel.Controls.Add(completeSelected);
            var completeAll = MakeButton("Complete All", 420, 0, 126, 38);
            MakeAccentButton(completeAll, AccentGreen);
            completeAll.Click += delegate { RunWorker("Complete All Races", CompleteAllCareerRaces, completeAll); };
            actionPanel.Controls.Add(completeAll);
            _racesSelected = MakeBodyLabel("Selected: none", 0, 54, 858, 26);
            _racesSelected.BackColor = Surface;
            actionPanel.Controls.Add(_racesSelected);
            _racesSearch = new TextBox();
            _racesSearch.Location = new Point(564, 5);
            _racesSearch.Size = new Size(294, 28);
            StyleTextBox(_racesSearch);
            _racesSearch.TextChanged += delegate { ApplyRacesFilter(); };
            actionPanel.Controls.Add(_racesSearch);

            var doneCard = MakeCard(_racesPage, 0, 258, ContentWidth, 300, "Completed / Open Routes", "Rows Luna sees as already open or completed.");
            _racesCompletedGrid = BuildDatabaseCarGrid("Race");
            ConfigureDatabaseCarGridHeaders(_racesCompletedGrid, "Race ID", "Race ID", "Type", "Race / Route Name", "Length", "Env", "Media", "Status");
            _racesCompletedGrid.Location = new Point(18, 56);
            _racesCompletedGrid.Size = new Size(858, 226);
            doneCard.Controls.Add(_racesCompletedGrid);
            var openCard = MakeCard(_racesPage, 0, 578, ContentWidth, 360, "Not Completed / Not Open", "Select rows, then complete or unlock.");
            _racesOpenGrid = BuildDatabaseCarGrid("Race");
            ConfigureDatabaseCarGridHeaders(_racesOpenGrid, "Race ID", "Race ID", "Type", "Race / Route Name", "Length", "Env", "Media", "Status");
            _racesOpenGrid.Location = new Point(18, 56);
            _racesOpenGrid.Size = new Size(858, 286);
            WireDatabaseCarSelectionLabel(_racesOpenGrid, _racesSelected);
            openCard.Controls.Add(_racesOpenGrid);
        }

        private void BuildFreePricesPage()
        {
            _freePricesPage.AutoScroll = true;
            AddPageHeader(_freePricesPage, "Free Car Prices", "Load prices, make selected cars free, or make all supported prices free.");

            var actions = MakeCard(_freePricesPage, 0, 72, ContentWidth, 166, "Price Actions", "Selected rows update Data_Car and car content offer price columns when present.");
            var actionPanel = new Panel();
            actionPanel.Location = new Point(18, 58);
            actionPanel.Size = new Size(858, 92);
            actionPanel.BackColor = Surface;
            actions.Controls.Add(actionPanel);
            var back = MakeButton("Back", 0, 0, 92, 38);
            back.Click += delegate { ShowProfilePage(); };
            actionPanel.Controls.Add(back);
            var load = MakeButton("Load Prices", 108, 0, 126, 38);
            MakeAccentButton(load, AccentBlue);
            load.Click += delegate { RunWorker("Load Free Prices", LoadFreePriceRows, load); };
            actionPanel.Controls.Add(load);
            var selected = MakeButton("Free Selected", 250, 0, 132, 38);
            MakeAccentButton(selected, AccentGreen);
            selected.Click += delegate
            {
                var ids = GetSelectedDatabaseCarIds(_freePricesPaidGrid);
                RunWorker("Free Selected Prices", delegate { ApplySelectedFreeCarPrices(ids); }, selected);
            };
            actionPanel.Controls.Add(selected);
            var all = MakeButton("Make All Free", 398, 0, 132, 38);
            MakeAccentButton(all, AccentGreen);
            all.Click += delegate { RunWorker("Make All Free Prices", ApplyFreeAutoshowPrices, all); };
            actionPanel.Controls.Add(all);
            _freePricesSelected = MakeBodyLabel("Selected: none", 0, 54, 858, 26);
            _freePricesSelected.BackColor = Surface;
            actionPanel.Controls.Add(_freePricesSelected);
            _freePricesSearch = new TextBox();
            _freePricesSearch.Location = new Point(548, 5);
            _freePricesSearch.Size = new Size(310, 28);
            StyleTextBox(_freePricesSearch);
            _freePricesSearch.TextChanged += delegate { ApplyFreePricesFilter(); };
            actionPanel.Controls.Add(_freePricesSearch);

            var freeCard = MakeCard(_freePricesPage, 0, 258, ContentWidth, 300, "Current Free Cars", "Cars already showing a zero supported price.");
            _freePricesFreeGrid = BuildDatabaseCarGrid("Price");
            ConfigureDatabaseCarGridHeaders(_freePricesFreeGrid, "Car ID", "Year", "Make", "Model", "PI", "Class", "Price", "Status");
            _freePricesFreeGrid.Location = new Point(18, 56);
            _freePricesFreeGrid.Size = new Size(858, 226);
            freeCard.Controls.Add(_freePricesFreeGrid);
            var paidCard = MakeCard(_freePricesPage, 0, 578, ContentWidth, 360, "Cars With Price", "Select rows, then press Free Selected.");
            _freePricesPaidGrid = BuildDatabaseCarGrid("Price");
            ConfigureDatabaseCarGridHeaders(_freePricesPaidGrid, "Car ID", "Year", "Make", "Model", "PI", "Class", "Price", "Status");
            _freePricesPaidGrid.Location = new Point(18, 56);
            _freePricesPaidGrid.Size = new Size(858, 286);
            WireDatabaseCarSelectionLabel(_freePricesPaidGrid, _freePricesSelected);
            paidCard.Controls.Add(_freePricesPaidGrid);
        }

        private void BuildDriveTrafficPage()
        {
            BuildDatabaseStatusPage(
                _driveTrafficPage,
                "Drive Traffic",
                "See which TrafficCars entries are ready to drive before applying the fix.",
                "Traffic Actions",
                "Load traffic cars, then make every traffic entry installed, purchased, visible, and drivable.",
                "Load Traffic",
                "Make Drivable",
                LoadDriveTrafficRows,
                ApplyDriveTrafficFromPage,
                ref _driveTrafficSearch,
                ref _driveTrafficReadyGrid,
                ref _driveTrafficNeedsGrid,
                "Already Drivable",
                "Traffic cars that already have the needed flags.",
                "Needs Fix",
                "Traffic cars missing one or more drivable flags.",
                new DatabaseGridHeaders("Car ID", "Year", "Make", "Model", "PI", "Class", "Traffic", "Status"));
        }

        private void BuildBarnFindsPage()
        {
            BuildDatabaseStatusPage(
                _barnFindsPage,
                "Barn Finds",
                "See completed and missing barn finds before applying completion.",
                "Barn Find Actions",
                "Load barn-find rows, then complete them and grant the cars.",
                "Load Barn Finds",
                "Complete All",
                LoadBarnFindRows,
                ApplyBarnFindsFromPage,
                ref _barnFindsSearch,
                ref _barnFindsCompleteGrid,
                ref _barnFindsNeedsGrid,
                "Completed",
                "Barn-find cars Luna sees as restored and owned.",
                "Needs Completion",
                "Barn-find cars still missing completion, VIN, or garage ownership.",
                new DatabaseGridHeaders("Car ID", "Year", "Make", "Model", "State", "Garage", "VIN", "Status"));
        }

        private void BuildDlcGatesPage()
        {
            _dlcGatesPage.AutoScroll = true;
            AddPageHeader(_dlcGatesPage, "DLC Gates", "Load blocked content offers, pick what you want, then add selected or add all.");

            var actions = MakeCard(_dlcGatesPage, 0, 72, ContentWidth, 166, "DLC Gate Actions", "Blocked content uses readable mapped names when Luna can resolve them from the live database.");
            actions.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            var actionPanel = new Panel();
            actionPanel.Location = new Point(18, 58);
            actionPanel.Size = new Size(858, 92);
            actionPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            actionPanel.BackColor = Surface;
            actions.Controls.Add(actionPanel);

            var back = MakeButton("Back", 0, 0, 92, 38);
            back.Click += delegate { ShowProfilePage(); };
            actionPanel.Controls.Add(back);

            var load = MakeButton("Load Gates", 108, 0, 132, 38);
            MakeAccentButton(load, AccentBlue);
            load.Click += delegate { RunWorker("Load DLC Gates", LoadDlcGateRows, load); };
            actionPanel.Controls.Add(load);

            var addSelected = MakeButton("Add Selected Gates", 256, 0, 154, 38);
            MakeAccentButton(addSelected, AccentGreen);
            addSelected.Click += delegate { RunWorker("Add Selected DLC Gates", ApplySelectedDlcGatesFromPage, addSelected); };
            actionPanel.Controls.Add(addSelected);

            var addAll = MakeButton("Open All Gates", 426, 0, 132, 38);
            MakeAccentButton(addAll, AccentPurple);
            addAll.Click += delegate { RunWorker("Open All DLC Gates", ApplyAllDlcGatesFromPage, addAll); };
            actionPanel.Controls.Add(addAll);

            _dlcGatesSearch = new TextBox();
            _dlcGatesSearch.Location = new Point(574, 5);
            _dlcGatesSearch.Size = new Size(284, 28);
            StyleTextBox(_dlcGatesSearch);
            _dlcGatesSearch.TextChanged += delegate { ApplyDatabaseStatusPageFilter(_dlcGatesPage); };
            actionPanel.Controls.Add(_dlcGatesSearch);

            _dlcGatesSelected = MakeBodyLabel("Selected: none", 0, 54, 240, 26);
            _dlcGatesSelected.BackColor = Surface;
            actionPanel.Controls.Add(_dlcGatesSelected);

            var hint = MakeBodyLabel("Use the All checkbox on Blocked Content to select everything visible, then press Add Selected Gates.", 250, 54, 608, 26);
            hint.BackColor = Surface;
            actionPanel.Controls.Add(hint);

            var openCard = MakeCard(_dlcGatesPage, 0, 258, ContentWidth, 300, "Open Content", "Content offer rows that already look open.");
            _dlcGatesOpenGrid = BuildDatabaseCarGrid("Status");
            ConfigureDatabaseCarGridHeaders(_dlcGatesOpenGrid, new DatabaseGridHeaders("Offer ID", "Type", "Name", "Package", "Offer Flags", "Mappings", "Release", "Status"));
            _dlcGatesOpenGrid.Location = new Point(18, 56);
            _dlcGatesOpenGrid.Size = new Size(858, 226);
            openCard.Controls.Add(_dlcGatesOpenGrid);

            var blockedCard = MakeCard(_dlcGatesPage, 0, 578, ContentWidth, 360, "Blocked Content", "Select DLC/content rows that are hidden, unpaid, unreleased, or upsell-gated.");
            _dlcGatesBlockedGrid = BuildDatabaseCarGrid("Status");
            ConfigureDatabaseCarGridHeaders(_dlcGatesBlockedGrid, new DatabaseGridHeaders("Offer ID", "Type", "Name", "Package", "Offer Flags", "Mappings", "Release", "Status"));
            _dlcGatesBlockedGrid.Location = new Point(18, 56);
            _dlcGatesBlockedGrid.Size = new Size(858, 286);
            WireDatabaseCarSelectionLabel(_dlcGatesBlockedGrid, _dlcGatesSelected);
            blockedCard.Controls.Add(_dlcGatesBlockedGrid);
        }

        private void BuildUnobtainablePage()
        {
            _unobtainablePage.AutoScroll = true;
            AddPageHeader(_unobtainablePage, "Unobtainable Gate", "See hidden-car gate rows before opening them.");

            var actions = MakeCard(_unobtainablePage, 0, 72, ContentWidth, 166, "Unobtainable Actions", "Load hidden-car gates, then open selected gates or all supported gates.");
            actions.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            var actionPanel = new Panel();
            actionPanel.Location = new Point(18, 58);
            actionPanel.Size = new Size(858, 92);
            actionPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            actionPanel.BackColor = Surface;
            actions.Controls.Add(actionPanel);

            var back = MakeButton("Back", 0, 0, 92, 38);
            back.Click += delegate { ShowProfilePage(); };
            actionPanel.Controls.Add(back);

            var load = MakeButton("Load Gates", 108, 0, 132, 38);
            MakeAccentButton(load, AccentBlue);
            load.Click += delegate { RunWorker("Load Gates", LoadUnobtainableRows, load); };
            actionPanel.Controls.Add(load);

            var addSelected = MakeButton("Add Selected Gates", 256, 0, 154, 38);
            MakeAccentButton(addSelected, AccentGreen);
            addSelected.Click += delegate { RunWorker("Add Selected Gates", ApplySelectedUnobtainableFromPage, addSelected); };
            actionPanel.Controls.Add(addSelected);

            var openAll = MakeButton("Open All Gates", 426, 0, 132, 38);
            MakeAccentButton(openAll, AccentPurple);
            openAll.Click += delegate { RunWorker("Open All Gates", ApplyUnobtainableFromPage, openAll); };
            actionPanel.Controls.Add(openAll);

            _unobtainableSearch = new TextBox();
            _unobtainableSearch.Location = new Point(574, 5);
            _unobtainableSearch.Size = new Size(284, 28);
            StyleTextBox(_unobtainableSearch);
            _unobtainableSearch.TextChanged += delegate { ApplyDatabaseStatusPageFilter(_unobtainablePage); };
            actionPanel.Controls.Add(_unobtainableSearch);

            _unobtainableSelected = MakeBodyLabel("Selected: none", 0, 54, 240, 26);
            _unobtainableSelected.BackColor = Surface;
            actionPanel.Controls.Add(_unobtainableSelected);

            var hint = MakeBodyLabel("Use the All checkbox on Blocked to select everything visible, then press Add Selected Gates.", 250, 54, 608, 26);
            hint.BackColor = Surface;
            actionPanel.Controls.Add(hint);

            var openCard = MakeCard(_unobtainablePage, 0, 258, ContentWidth, 300, "Open", "Hidden-car gates already moved out of the positive range.");
            _unobtainableOpenGrid = BuildDatabaseCarGrid("Status");
            ConfigureDatabaseCarGridHeaders(_unobtainableOpenGrid, new DatabaseGridHeaders("Car ID", "Gate", "Make", "Model", "PI", "Class", "Ordinal", "Status"));
            _unobtainableOpenGrid.Location = new Point(18, 56);
            _unobtainableOpenGrid.Size = new Size(858, 226);
            openCard.Controls.Add(_unobtainableOpenGrid);

            var blockedCard = MakeCard(_unobtainablePage, 0, 578, ContentWidth, 360, "Blocked", "Positive gate rows that still block cars.");
            _unobtainableBlockedGrid = BuildDatabaseCarGrid("Status");
            ConfigureDatabaseCarGridHeaders(_unobtainableBlockedGrid, new DatabaseGridHeaders("Car ID", "Gate", "Make", "Model", "PI", "Class", "Ordinal", "Status"));
            _unobtainableBlockedGrid.Location = new Point(18, 56);
            _unobtainableBlockedGrid.Size = new Size(858, 286);
            WireDatabaseCarSelectionLabel(_unobtainableBlockedGrid, _unobtainableSelected);
            blockedCard.Controls.Add(_unobtainableBlockedGrid);
        }

        private void BuildUnlockPresetsPage()
        {
            BuildDatabaseStatusPage(
                _unlockPresetsPage,
                "Unlock Presets",
                "See upgrade preset packages that are buyable or still hidden.",
                "Preset Actions",
                "Load preset packages, then make supported hidden presets purchasable.",
                "Load Presets",
                "Unlock Presets",
                LoadUnlockPresetRows,
                ApplyUnlockPresetsFromPage,
                ref _unlockPresetsSearch,
                ref _unlockPresetsOpenGrid,
                ref _unlockPresetsLockedGrid,
                "Purchasable",
                "Preset packages already buyable.",
                "Hidden",
                "Preset packages not purchasable yet.",
                new DatabaseGridHeaders("Preset ID", "Ordinal", "Title", "Description", "Release", "Crumbs", "Thumbnail", "Status"));
        }

        private void BuildNewTagsPage()
        {
            BuildDatabaseStatusPage(
                _newTagsPage,
                "New Tags",
                "See garage cars with new-car badges before clearing them.",
                "New Tag Actions",
                "Load garage tags, then mark supported rows as viewed.",
                "Load Tags",
                "Clear Tags",
                LoadNewTagRows,
                ApplyNewTagsFromPage,
                ref _newTagsSearch,
                ref _newTagsClearGrid,
                ref _newTagsTaggedGrid,
                "Already Clear",
                "Garage cars with the new tag already cleared.",
                "Has New Tag",
                "Garage cars still marked as new.",
                new DatabaseGridHeaders("Car ID", "Year", "Make", "Model", "PI", "Class", "Copies", "Status"));
        }

        private void BuildInstallFlagsPage()
        {
            _installFlagsPage.AutoScroll = true;
            AddPageHeader(_installFlagsPage, "Install Flags", "See cars that are installed/drivable and cars that still need flags.");

            var actions = MakeCard(_installFlagsPage, 0, 72, ContentWidth, 166, "Install Flag Actions", "Load install flags, then mark selected cars or all supported cars as installed, purchased, and drivable.");
            actions.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            var actionPanel = new Panel();
            actionPanel.Location = new Point(18, 58);
            actionPanel.Size = new Size(858, 92);
            actionPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            actionPanel.BackColor = Surface;
            actions.Controls.Add(actionPanel);

            var back = MakeButton("Back", 0, 0, 92, 38);
            back.Click += delegate { ShowProfilePage(); };
            actionPanel.Controls.Add(back);

            var load = MakeButton("Load Flags", 108, 0, 132, 38);
            MakeAccentButton(load, AccentBlue);
            load.Click += delegate { RunWorker("Load Flags", LoadInstallFlagRows, load); };
            actionPanel.Controls.Add(load);

            var applySelected = MakeButton("Apply Selected Flags", 256, 0, 166, 38);
            MakeAccentButton(applySelected, AccentGreen);
            applySelected.Click += delegate { RunWorker("Apply Selected Flags", ApplySelectedInstallFlagsFromPage, applySelected); };
            actionPanel.Controls.Add(applySelected);

            var applyAll = MakeButton("Apply All Flags", 438, 0, 132, 38);
            MakeAccentButton(applyAll, AccentPurple);
            applyAll.Click += delegate { RunWorker("Apply All Flags", ApplyInstallFlagsFromPage, applyAll); };
            actionPanel.Controls.Add(applyAll);

            _installFlagsSearch = new TextBox();
            _installFlagsSearch.Location = new Point(586, 5);
            _installFlagsSearch.Size = new Size(272, 28);
            StyleTextBox(_installFlagsSearch);
            _installFlagsSearch.TextChanged += delegate { ApplyDatabaseStatusPageFilter(_installFlagsPage); };
            actionPanel.Controls.Add(_installFlagsSearch);

            _installFlagsSelected = MakeBodyLabel("Selected: none", 0, 54, 240, 26);
            _installFlagsSelected.BackColor = Surface;
            actionPanel.Controls.Add(_installFlagsSelected);

            var hint = MakeBodyLabel("Select cars in Needs Flags, then press Apply Selected Flags.", 250, 54, 608, 26);
            hint.BackColor = Surface;
            actionPanel.Controls.Add(hint);

            var readyCard = MakeCard(_installFlagsPage, 0, 258, ContentWidth, 300, "Already Ready", "Cars that already look installed, purchased, and drivable.");
            _installFlagsReadyGrid = BuildDatabaseCarGrid("Status");
            ConfigureDatabaseCarGridHeaders(_installFlagsReadyGrid, new DatabaseGridHeaders("Car ID", "Year", "Make", "Model", "PI", "Class", "Flags", "Status"));
            _installFlagsReadyGrid.Location = new Point(18, 56);
            _installFlagsReadyGrid.Size = new Size(858, 226);
            readyCard.Controls.Add(_installFlagsReadyGrid);

            var needsCard = MakeCard(_installFlagsPage, 0, 578, ContentWidth, 360, "Needs Flags", "Cars missing one or more install flags.");
            _installFlagsNeedsGrid = BuildDatabaseCarGrid("Status");
            ConfigureDatabaseCarGridHeaders(_installFlagsNeedsGrid, new DatabaseGridHeaders("Car ID", "Year", "Make", "Model", "PI", "Class", "Flags", "Status"));
            _installFlagsNeedsGrid.Location = new Point(18, 56);
            _installFlagsNeedsGrid.Size = new Size(858, 286);
            WireDatabaseCarSelectionLabel(_installFlagsNeedsGrid, _installFlagsSelected);
            needsCard.Controls.Add(_installFlagsNeedsGrid);
        }

        private void BuildDatabaseStatusPage(
            Panel page,
            string title,
            string subtitle,
            string actionTitle,
            string actionSubtitle,
            string loadText,
            string applyText,
            Action loadAction,
            Action applyAction,
            ref TextBox searchBox,
            ref DataGridView readyGrid,
            ref DataGridView needsGrid,
            string readyTitle,
            string readySubtitle,
            string needsTitle,
            string needsSubtitle,
            DatabaseGridHeaders headers)
        {
            page.AutoScroll = true;
            AddPageHeader(page, title, subtitle);

            var actions = MakeCard(page, 0, 72, ContentWidth, 166, actionTitle, actionSubtitle);
            actions.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            var actionPanel = new Panel();
            actionPanel.Location = new Point(18, 58);
            actionPanel.Size = new Size(858, 92);
            actionPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            actionPanel.BackColor = Surface;
            actions.Controls.Add(actionPanel);

            var back = MakeButton("Back", 0, 0, 92, 38);
            back.Click += delegate { ShowProfilePage(); };
            actionPanel.Controls.Add(back);

            var load = MakeButton(loadText, 108, 0, 132, 38);
            MakeAccentButton(load, AccentBlue);
            load.Click += delegate { RunWorker(loadText, loadAction, load); };
            actionPanel.Controls.Add(load);

            var apply = MakeButton(applyText, 256, 0, 142, 38);
            MakeAccentButton(apply, AccentGreen);
            apply.Click += delegate { RunWorker(applyText, applyAction, apply); };
            actionPanel.Controls.Add(apply);

            searchBox = new TextBox();
            searchBox.Location = new Point(418, 5);
            searchBox.Size = new Size(440, 28);
            StyleTextBox(searchBox);
            searchBox.TextChanged += delegate { ApplyDatabaseStatusPageFilter(page); };
            actionPanel.Controls.Add(searchBox);

            var hint = MakeBodyLabel("Load first, review the two tables, then apply when ready.", 0, 54, 858, 26);
            hint.BackColor = Surface;
            actionPanel.Controls.Add(hint);

            var readyCard = MakeCard(page, 0, 258, ContentWidth, 300, readyTitle, readySubtitle);
            readyGrid = BuildDatabaseCarGrid(headers == null ? "Status" : headers.StateHeader);
            ConfigureDatabaseCarGridHeaders(readyGrid, headers);
            readyGrid.Location = new Point(18, 56);
            readyGrid.Size = new Size(858, 226);
            readyCard.Controls.Add(readyGrid);

            var needsCard = MakeCard(page, 0, 578, ContentWidth, 360, needsTitle, needsSubtitle);
            needsGrid = BuildDatabaseCarGrid(headers == null ? "Status" : headers.StateHeader);
            ConfigureDatabaseCarGridHeaders(needsGrid, headers);
            needsGrid.Location = new Point(18, 56);
            needsGrid.Size = new Size(858, 286);
            needsCard.Controls.Add(needsGrid);
        }

        private void BuildProfilePage()
        {
            _profilePage.AutoScroll = true;
            AddPageHeader(_profilePage, "Database Tools", "Buttons for garage fixes, unlocks, prices, and backups.");

            var status = new Label();
            status.Text = "Ready";
            status.Font = new Font("Segoe UI Semibold", 9F);
            status.ForeColor = AccentBlue;
            status.BackColor = AppBackground;
            status.Location = new Point(806, 12);
            status.Size = new Size(190, 24);
            status.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            status.TextAlign = ContentAlignment.MiddleRight;
            _profilePage.Controls.Add(status);
            _profilePage.Tag = status;

            var toolsHeight = 708;
            var tools = MakeCard(_profilePage, 0, 72, ContentWidth + 96, toolsHeight, "Database Actions", "Choose one action, let it finish, then check the game.");
            tools.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            _profileToolsPanel = new FlowLayoutPanel();
            _profileToolsPanel.Location = new Point(18, 58);
            _profileToolsPanel.Size = new Size(ContentWidth + 60, toolsHeight - 78);
            _profileToolsPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            _profileToolsPanel.WrapContents = true;
            _profileToolsPanel.AutoScroll = false;
            _profileToolsPanel.Padding = new Padding(5, 0, 5, 0);
            _profileToolsPanel.BackColor = Surface;
            tools.Controls.Add(_profileToolsPanel);

            AddProfileToolSectionLabel("Editors");
            var carClass = AddProfileNavigationButton("Car Class Panel", 0, 0, 166, ShowCarEditorPage, "Open the car class changer.");
            var statsEditor = AddProfileNavigationButton("Stats Editor", 0, 0, 166, ShowSkillPage, "Open saved car and profile stats.");
            var vehicleTuner = AddProfileNavigationButton("Vehicle Tuner", 0, 0, 166, ShowVehicleTunerPage, "Open physics and performance database tuning.");
            var garageFavorites = AddProfileNavigationButton("Garage Favorites", 0, 0, 166, ShowGarageFavoritesPage, "Favorite or unfavorite selected owned garage cars.");
            var trafficEditor = AddProfileNavigationButton("Traffic Editor", 0, 0, 166, ShowTrafficEditorPage, "Choose which cars can appear in the traffic list.");
            _profileToolsPanel.SetFlowBreak(trafficEditor, true);
            AddProfileNavigationButton("Profile Cosmetics", 0, 0, 166, ShowProfileCosmeticsPage, "Open player-name, character, and team-color tables.");
            var aiBehavior = AddProfileNavigationButton("AI Behavior Editor", 0, 0, 166, ShowAiBehaviorPage, "Open AI temperament, mistake, observation, and line-choice tables.");
            _profileToolsPanel.SetFlowBreak(aiBehavior, true);

            AddProfileToolSectionLabel("Tables & Unlocks");
            var wheelspinOdds = AddProfileNavigationButton("Wheelspin Odds", 0, 0, 166, ShowWheelspinOddsPage, "Open rarity odds table and on/off switch.");
            var freePrices = AddProfileNavigationButton("Free Prices", 0, 0, 166, ShowFreePricesPage, "Open car price table and apply free prices.");
            var driveTraffic = AddProfileNavigationButton("Drive Traffic", 0, 0, 166, ShowDriveTrafficPage, "Open traffic drivable status before applying.");
            var barnFinds = AddProfileNavigationButton("Barn Finds", 0, 0, 166, ShowBarnFindsPage, "Open completed and missing barn-find status.");
            var dlcGates = AddProfileNavigationButton("DLC Gates", 0, 0, 166, ShowDlcGatesPage, "Open DLC/content gate status.");
            _profileToolsPanel.SetFlowBreak(dlcGates, true);
            var installFlags = AddProfileNavigationButton("Install Flags", 0, 0, 166, ShowInstallFlagsPage, "Open install/drivable flag status.");
            var newTags = AddProfileNavigationButton("New Tags", 0, 0, 166, ShowNewTagsPage, "Open new-car tag status.");
            var unlockPresets = AddProfileNavigationButton("Unlock Presets", 0, 0, 166, ShowUnlockPresetsPage, "Open hidden upgrade preset status.");
            var unobtainableGate = AddProfileNavigationButton("Unobtainable Gate", 0, 0, 166, ShowUnobtainablePage, "Open hidden-car gate status.");
            _profileToolsPanel.SetFlowBreak(unobtainableGate, true);

            AddProfileToolSectionLabel("Quick Actions - Save & Database");
            AddProfileToolButton("Refresh Summary", 0, 0, 166, RefreshProfileSummary, "Shows simple counts in the log.");
            AddProfileToolButton("Autoshow DB", 0, 0, 166, ApplyAutoshowDatabaseTool, "Makes hidden Autoshow cars show up.");
            _dump = AddProfileToolButton("Dump", 0, 0, 166, Dump, "Saves database info for debugging.");
            AddProfileToolButton("Backup Save", 0, 0, 166, BackupPgsSave, "Backs up the current FH6 PGS save folder into Luna's save_backups folder.");
            _persist = AddProfileToolButton("Reapply Unlock", 0, 0, 166, Persist, "Runs the Autoshow unlock again.");
            _profileToolsPanel.SetFlowBreak(_persist, true);

            AddProfileToolSectionLabel("Quick Actions - Garage & Cars");
            AddProfileToolButton("Add All Grants", 0, 0, 166, AddAllCars, "Adds only cars missing from the garage using Luna's grant method.");
            AddProfileToolButton("Remove Dupe Cars", 0, 0, 166, RemoveDuplicateGarageCars, "Keeps one copy of each duplicate car.");
            AddProfileToolButton("Restore All Car Classes", 0, 0, 166, FixAllGarageClasses, "Repairs PI class values so garage cards do not show question marks.");
            AddProfileToolButton("Fix Thumbnails", 0, 0, 166, FixGarageThumbnails, "Refreshes garage pictures.");
            var freeUpgrades = AddProfileToolButton("Free Upgrades", 0, 0, 166, ApplyFreeUpgradeParts, "Makes supported upgrades free.");
            _profileToolsPanel.SetFlowBreak(freeUpgrades, true);
            AddProfileToolButton("Decal Unlocker", 0, 0, 166, ApplyDecalUnlocker, "Unlocks missing livery decals for every supported car make.");
            var auctionAll = AddProfileToolButtonDirect("Auction All Cars", 0, 0, 166, StartAuctionHouseAllCars, "Preview cars that are not auctionable yet, deselect any you want to skip, then make the selected cars auctionable.");
            _profileToolsPanel.SetFlowBreak(auctionAll, true);

            AddProfileToolSectionLabel("Quick Actions - Live Switches");
            _classRestrictionSwitchButton = AddProfileToolButtonDirect("Class Restrictions: OFF", 0, 0, 166, ToggleClassRestrictions, "Pick one class for every car. Turning it off restores classes from PI.");
            UpdateClassRestrictionSwitchButton(false);
            _disableDamageSwitchButton = AddProfileToolButton("Disable Damage: OFF", 0, 0, 166, ToggleDisableDamage, "Turns cosmetic/mechanical damage off or restores it. Switch cars and switch back after changing it.");
            UpdateDisableDamageSwitchButton(false);
            _removeTrafficSwitchButton = AddProfileToolButtonDirect("Remove Traffic: OFF", 0, 0, 166, ToggleRemoveTrafficSwitch, "Backs up TrafficCars and race traffic flags, clears them, and restores them when turned off. You may need to enter and exit a race or teleport after toggling before traffic fully refreshes.");
            UpdateRemoveTrafficSwitchButton(false);
            _freezeAiTrafficSwitchButton = AddProfileToolButtonDirect("Freeze AI Traffic: OFF", 0, 0, 166, ToggleFreezeAiTrafficSwitch, "Like Remove Traffic, but instead of removing free-roam civilian traffic it holds every nearby traffic car at zero speed. Drive your car for a second first so Luna learns it and leaves it free. Turn off to let traffic move again.");
            UpdateFreezeAiTrafficSwitchButton(false);
            _profileToolsPanel.SetFlowBreak(_freezeAiTrafficSwitchButton, true);

            AddProfileToolSectionLabel("Internal Actions");
            var photoCapture = AddProfileNavigationButton("Internal Photo Capture", 0, 0, 166, delegate
            {
                ShowInternalDatabasePageWithWarning(
                    "Internal Photo Capture",
                    "You understand that Internal Photo Capture is an internal database helper, not the actual Horizon Promo photo capture screen in game.",
                    ShowPhotoCapturePage);
            }, "Open photographed and missing Horizon Promo cars.");
            var routes = AddProfileNavigationButton("Internal Routes", 0, 0, 166, delegate
            {
                ShowInternalDatabasePageWithWarning(
                    "Internal Routes",
                    "You understand that Internal Routes is an internal database helper, not the actual routes shown on the in-game map.",
                    ShowRoutesPage);
            }, "Open routes/tracks that are unlocked or still locked.");
            var races = AddProfileNavigationButton("Internal Races", 0, 0, 166, delegate
            {
                ShowInternalDatabasePageWithWarning(
                    "Internal Races",
                    "You understand that Internal Races is an internal database helper, not the actual race menu or route flow in game.",
                    ShowRacesPage);
            }, "Open race completion and route helper table.");
            _profileToolsPanel.SetFlowBreak(races, true);

            _profilePage.AutoScrollMinSize = new Size(ContentWidth + 96, toolsHeight + 112);
        }

        private void AddProfileToolSectionLabel(string text)
        {
            if (_profileToolsPanel == null)
                return;

            var label = new Label();
            SetUiText(label, text);
            label.UseMnemonic = false;
            label.Size = new Size(950, 24);
            label.Margin = new Padding(0, 2, 0, 8);
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.BackColor = Surface;
            label.ForeColor = AccentBlue;
            label.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            _profileToolsPanel.Controls.Add(label);
            _profileToolsPanel.SetFlowBreak(label, true);
        }

        private Button AddProfileToolButton(string text, int x, int y, int width, Action action, string tooltip)
        {
            var button = MakeButton(text, x, y, 176, 36);
            SetUiText(button, text);
            button.Margin = new Padding(0, 0, 14, 10);
            button.Click += delegate { RunWorker(text, action, button); };
            if (_profileToolsPanel != null)
            {
                _profileToolsPanel.Controls.Add(button);
            }
            else
            {
                _profilePage.Controls.Add(button);
            }
            SetTranslatedToolTip(button, tooltip);
            return button;
        }

        private Button AddProfileToolButtonDirect(string text, int x, int y, int width, Action<Button> action, string tooltip)
        {
            var button = MakeButton(text, x, y, 176, 36);
            SetUiText(button, text);
            button.Margin = new Padding(0, 0, 14, 10);
            button.Click += delegate { action(button); };
            if (_profileToolsPanel != null)
            {
                _profileToolsPanel.Controls.Add(button);
            }
            else
            {
                _profilePage.Controls.Add(button);
            }
            SetTranslatedToolTip(button, tooltip);
            return button;
        }

        private Button AddProfileNavigationButton(string text, int x, int y, int width, Action action, string tooltip)
        {
            var button = MakeButton(text, x, y, 176, 36);
            SetUiText(button, text);
            button.Margin = new Padding(0, 0, 14, 10);
            button.Click += delegate { action(); };
            if (_profileToolsPanel != null)
            {
                _profileToolsPanel.Controls.Add(button);
            }
            else
            {
                _profilePage.Controls.Add(button);
            }
            SetTranslatedToolTip(button, tooltip);
            return button;
        }

        private void BackupPgsSave()
        {
            const string saveRoot = @"C:\XboxGames\GameSave\pgs";
            if (!Directory.Exists(saveRoot))
                throw new InvalidOperationException("PGS save folder was not found: " + saveRoot);

            var backupDir = Path.Combine(_resultsDir, "save_backups");
            Directory.CreateDirectory(backupDir);

            var baseName = "Luna Save Backup " + DateTime.Now.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
            var destination = Path.Combine(backupDir, baseName);
            if (Directory.Exists(destination))
            {
                destination = Path.Combine(backupDir, baseName + " " + DateTime.Now.ToString("HHmmss", CultureInfo.InvariantCulture));
                var counter = 2;
                while (Directory.Exists(destination))
                {
                    destination = Path.Combine(backupDir, baseName + " " + DateTime.Now.ToString("HHmmss", CultureInfo.InvariantCulture) + " " + counter.ToString(CultureInfo.InvariantCulture));
                    counter++;
                }
            }

            var skippedFiles = new List<string>();
            var copiedFiles = CopyDirectoryTree(saveRoot, Path.Combine(destination, "pgs"), skippedFiles);
            Log("Save backup copied " + copiedFiles.ToString(CultureInfo.InvariantCulture) + " file(s) from pgs to " + Path.GetFileName(destination) + ".");
            if (skippedFiles.Count > 0)
                Log("Save backup skipped " + skippedFiles.Count.ToString(CultureInfo.InvariantCulture) + " locked/unreadable item(s).");
            ShowInfo("Save backup created." + Environment.NewLine + Environment.NewLine +
                Path.GetFileName(destination) + Environment.NewLine +
                "Location: " + destination + Environment.NewLine + Environment.NewLine +
                (skippedFiles.Count > 0 ? "Some locked helper files were skipped, but the backup folder was created." + Environment.NewLine + Environment.NewLine : string.Empty) +
                "To restore manually, copy the backed up pgs folder contents back into:" + Environment.NewLine +
                saveRoot);
        }

        private static int CopyDirectoryTree(string sourceDirectory, string destinationDirectory, List<string> skippedItems)
        {
            Directory.CreateDirectory(destinationDirectory);
            foreach (var directory in EnumerateDirectoriesSafe(sourceDirectory, skippedItems))
            {
                var relative = GetRelativePath(sourceDirectory, directory);
                try
                {
                    Directory.CreateDirectory(Path.Combine(destinationDirectory, relative));
                }
                catch (Exception ex)
                {
                    if (skippedItems != null)
                        skippedItems.Add(relative + ": " + ex.Message);
                }
            }

            var copied = 0;
            foreach (var file in EnumerateFilesSafe(sourceDirectory, skippedItems))
            {
                var relative = GetRelativePath(sourceDirectory, file);
                var target = Path.Combine(destinationDirectory, relative);
                try
                {
                    var targetDir = Path.GetDirectoryName(target);
                    if (!string.IsNullOrEmpty(targetDir))
                        Directory.CreateDirectory(targetDir);
                    File.Copy(file, target, true);
                    copied++;
                }
                catch (Exception ex)
                {
                    if (skippedItems != null)
                        skippedItems.Add(relative + ": " + ex.Message);
                }
            }

            return copied;
        }

        private static IEnumerable<string> EnumerateDirectoriesSafe(string root, List<string> skippedItems)
        {
            var pending = new Stack<string>();
            pending.Push(root);
            while (pending.Count > 0)
            {
                var current = pending.Pop();
                string[] directories;
                try
                {
                    directories = Directory.GetDirectories(current);
                }
                catch (Exception ex)
                {
                    if (skippedItems != null)
                        skippedItems.Add(current + ": " + ex.Message);
                    continue;
                }

                foreach (var directory in directories)
                {
                    yield return directory;
                    pending.Push(directory);
                }
            }
        }

        private static IEnumerable<string> EnumerateFilesSafe(string root, List<string> skippedItems)
        {
            var pending = new Stack<string>();
            pending.Push(root);
            while (pending.Count > 0)
            {
                var current = pending.Pop();
                string[] directories;
                try
                {
                    directories = Directory.GetDirectories(current);
                }
                catch (Exception ex)
                {
                    if (skippedItems != null)
                        skippedItems.Add(current + ": " + ex.Message);
                    directories = new string[0];
                }

                string[] files;
                try
                {
                    files = Directory.GetFiles(current);
                }
                catch (Exception ex)
                {
                    if (skippedItems != null)
                        skippedItems.Add(current + ": " + ex.Message);
                    files = new string[0];
                }

                foreach (var file in files)
                    yield return file;
                foreach (var directory in directories)
                    pending.Push(directory);
            }
        }

        private static string GetRelativePath(string root, string path)
        {
            var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var pathFull = Path.GetFullPath(path);
            if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
                return Path.GetFileName(pathFull);
            return pathFull.Substring(rootFull.Length);
        }

        private void ShowProfilePage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowProfilePage);
                return;
            }
            HidePages();
            _profilePage.Visible = true;
            _profilePage.BringToFront();
            SetStatus("Database");
            SetProfileStatus("Ready");
            SetDrivingStatus("Ready");
            UpdateNavigationState(_profileEditor);
        }

        private bool _databaseWarningAcknowledged;

        private bool ConfirmDatabaseToolsOnce(string title, string message)
        {
            if (_databaseWarningAcknowledged)
                return true;
            if (!ShowProceedDialog(title, message))
                return false;
            _databaseWarningAcknowledged = true;
            return true;
        }

        private void ShowProfilePageWithDatabaseWarning()
        {
            if (!ConfirmDatabaseToolsOnce(
                "Database Tools",
                "You do understand that depending on the game version, these database tools may or may not work."))
            {
                return;
            }

            ShowProfilePage();
        }

        private void ShowInternalDatabasePageWithWarning(string title, string message, Action showPage)
        {
            if (showPage == null)
                return;
            if (!ConfirmDatabaseToolsOnce(title, message))
                return;

            showPage();
        }

        private void ShowRarityPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowRarityPage);
                return;
            }
            HidePages();
            _rarityPage.Visible = true;
            _rarityPage.BringToFront();
            SetStatus("Car Rarity");
            SetRarityStatus("Ready");
            UpdateNavigationState(_profileEditor);
        }

        private void ShowSkillPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowSkillPage);
                return;
            }
            HidePages();
            _skillPage.Visible = true;
            _skillPage.BringToFront();
            SetStatus("Stats Editor");
            SetSkillStatus("Ready");
            UpdateNavigationState(_profileEditor);
        }

        private void ShowCarEditorPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowCarEditorPage);
                return;
            }
            HidePages();
            _carEditorPage.Visible = true;
            _carEditorPage.BringToFront();
            SetStatus("Car Class");
            SetCarEditorStatus("Ready");
            UpdateNavigationState(_profileEditor);
        }

        private void ShowVehicleTunerPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowVehicleTunerPage);
                return;
            }
            HidePages();
            _vehicleTunerPage.Visible = true;
            _vehicleTunerPage.BringToFront();
            SetStatus("Vehicle Tuner");
            SetProfileStatus("Vehicle Tuner");
            UpdateNavigationState(_profileEditor);
        }

        private void ShowDatabaseTuningPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowDatabaseTuningPage);
                return;
            }
            HidePages();
            _databaseTuningPage.Visible = true;
            _databaseTuningPage.BringToFront();
            SetStatus("Database Tuning");
            SetProfileStatus("Database Tuning");
            UpdateNavigationState(_navDatabaseTuning);
        }

        private void ShowTuningPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowTuningPage);
                return;
            }
            HidePages();
            _tuningPage.Visible = true;
            _tuningPage.BringToFront();
            SetStatus("Driving Tuning");
            SetProfileStatus("Driving Tuning");
            UpdateNavigationState(_navTuning);
        }

        private void ShowGarageFavoritesPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowGarageFavoritesPage);
                return;
            }
            HidePages();
            _garageFavoritesPage.Visible = true;
            _garageFavoritesPage.BringToFront();
            SetStatus("Garage Favorites");
            SetProfileStatus("Garage Favorites");
            UpdateNavigationState(_profileEditor);
        }

        private void ShowTrafficEditorPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowTrafficEditorPage);
                return;
            }
            HidePages();
            _trafficEditorPage.Visible = true;
            _trafficEditorPage.BringToFront();
            SetStatus("Traffic Editor");
            SetProfileStatus("Traffic Editor");
            UpdateNavigationState(_profileEditor);
        }

        private void ShowProfileCosmeticsPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowProfileCosmeticsPage);
                return;
            }
            HidePages();
            _profileCosmeticsPage.Visible = true;
            _profileCosmeticsPage.BringToFront();
            SetStatus("Profile Cosmetics");
            SetProfileStatus("Profile Cosmetics");
            UpdateNavigationState(_profileEditor);
        }

        private void ShowAiBehaviorPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowAiBehaviorPage);
                return;
            }
            HidePages();
            _aiBehaviorPage.Visible = true;
            _aiBehaviorPage.BringToFront();
            SetStatus("AI Behavior");
            SetProfileStatus("AI Behavior");
            UpdateNavigationState(_profileEditor);
        }

        private void ShowPhotoCapturePage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowPhotoCapturePage);
                return;
            }
            HidePages();
            _photoCapturePage.Visible = true;
            _photoCapturePage.BringToFront();
            SetStatus("Photo Capture");
            SetProfileStatus("Photo Capture");
            UpdateNavigationState(_profileEditor);
        }

        private void ShowWheelspinOddsPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowWheelspinOddsPage);
                return;
            }
            HidePages();
            _wheelspinOddsPage.Visible = true;
            _wheelspinOddsPage.BringToFront();
            SetStatus("Wheelspin Odds");
            SetProfileStatus("Wheelspin Odds");
            UpdateWheelspinOddsSwitchButton();
            UpdateNavigationState(_profileEditor);
        }

        private void ShowRoutesPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowRoutesPage);
                return;
            }
            HidePages();
            _routesPage.Visible = true;
            _routesPage.BringToFront();
            SetStatus("Routes");
            SetProfileStatus("Routes");
            UpdateNavigationState(_profileEditor);
        }

        private void ShowRacesPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowRacesPage);
                return;
            }
            HidePages();
            _racesPage.Visible = true;
            _racesPage.BringToFront();
            SetStatus("Races");
            SetProfileStatus("Races");
            UpdateNavigationState(_profileEditor);
        }

        private void ShowFreePricesPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowFreePricesPage);
                return;
            }
            HidePages();
            _freePricesPage.Visible = true;
            _freePricesPage.BringToFront();
            SetStatus("Free Prices");
            SetProfileStatus("Free Prices");
            UpdateNavigationState(_profileEditor);
        }

        private void ShowDriveTrafficPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowDriveTrafficPage);
                return;
            }
            HidePages();
            _driveTrafficPage.Visible = true;
            _driveTrafficPage.BringToFront();
            SetStatus("Drive Traffic");
            SetProfileStatus("Drive Traffic");
            UpdateNavigationState(_profileEditor);
        }

        private void ShowBarnFindsPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowBarnFindsPage);
                return;
            }
            HidePages();
            _barnFindsPage.Visible = true;
            _barnFindsPage.BringToFront();
            SetStatus("Barn Finds");
            SetProfileStatus("Barn Finds");
            UpdateNavigationState(_profileEditor);
        }

        private void ShowDlcGatesPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowDlcGatesPage);
                return;
            }
            if (!ShowDlcGatePlatformNotice())
                return;

            HidePages();
            _dlcGatesPage.Visible = true;
            _dlcGatesPage.BringToFront();
            SetStatus("DLC Gates");
            SetProfileStatus("DLC Gates");
            UpdateNavigationState(_profileEditor);
            AutoLoadDatabaseRows("Load DLC Gates", LoadDlcGateRows, _dlcGateRows);
        }

        private bool ShowDlcGatePlatformNotice()
        {
            if (InvokeRequired)
                return (bool)Invoke(new Func<bool>(ShowDlcGatePlatformNotice));

            using (var dialog = new Form())
            {
                dialog.Text = "DLC Gates";
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.BackColor = AppBackground;
                dialog.ForeColor = TextPrimary;
                dialog.Font = new Font("Segoe UI", 9F);
                dialog.ClientSize = new Size(520, 292);

                var card = new ModernPanel();
                card.Location = new Point(14, 14);
                card.Size = new Size(492, 264);
                card.FillColor = Surface;
                card.BorderColor = Border;
                card.CornerRadius = 14;
                dialog.Controls.Add(card);

                var title = new Label();
                title.Text = "DLC Gates";
                title.Font = new Font("Segoe UI Semibold", 13F);
                title.ForeColor = TextPrimary;
                title.BackColor = Surface;
                title.Location = new Point(18, 14);
                title.Size = new Size(456, 28);
                title.TextAlign = ContentAlignment.MiddleCenter;
                card.Controls.Add(title);

                var message = new Label();
                message.Text = "DLC Gates works however not for everyone please use the two selection below for your platform.";
                message.Font = new Font("Segoe UI", 9.5F);
                message.ForeColor = TextMuted;
                message.BackColor = Surface;
                message.Location = new Point(34, 54);
                message.Size = new Size(424, 54);
                message.TextAlign = ContentAlignment.MiddleCenter;
                card.Controls.Add(message);

                var steam = MakeButton("Steam", 80, 122, 154, 48);
                ConfigureDlcGatePlatformButton(steam, "steam-icon.png", AccentBlue);
                steam.Click += delegate { OpenDlcGatePlatformLink("https://shorturl.at/5tKEU"); };
                card.Controls.Add(steam);

                var xbox = MakeButton("Xbox", 258, 122, 154, 48);
                ConfigureDlcGatePlatformButton(xbox, "xbox-icon.png", AccentGreen);
                xbox.Click += delegate { OpenDlcGatePlatformLink("https://shorturl.at/RSzLa"); };
                card.Controls.Add(xbox);
                dialog.FormClosed += delegate
                {
                    DisposeDlcGatePlatformButtonIcon(steam);
                    DisposeDlcGatePlatformButtonIcon(xbox);
                };

                var continueButton = MakeButton("Continue", 184, 204, 124, 34);
                MakeAccentButton(continueButton, AccentGreen);
                continueButton.Click += delegate
                {
                    dialog.DialogResult = DialogResult.OK;
                    dialog.Close();
                };
                card.Controls.Add(continueButton);
                dialog.AcceptButton = continueButton;

                PrepareDialogForLanguage(dialog);
                return dialog.ShowDialog(this) == DialogResult.OK;
            }
        }

        private static void ConfigureDlcGatePlatformButton(Button button, string iconResource, Color accent)
        {
            MakeAccentButton(button, accent);
            var modern = button as ModernButton;
            if (modern == null)
                return;

            modern.IconImage = LoadLocalAssetImage(iconResource);
            modern.CenterContent = true;
            modern.CornerRadius = 10;
        }

        private static void DisposeDlcGatePlatformButtonIcon(Button button)
        {
            var modern = button as ModernButton;
            if (modern == null || modern.IconImage == null)
                return;

            var image = modern.IconImage;
            modern.IconImage = null;
            image.Dispose();
        }

        private void OpenDlcGatePlatformLink(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                ShowInfo("Could not open platform link." + Environment.NewLine + Environment.NewLine + ex.Message);
            }
        }

        private void ShowUnobtainablePage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowUnobtainablePage);
                return;
            }
            HidePages();
            _unobtainablePage.Visible = true;
            _unobtainablePage.BringToFront();
            SetStatus("Unobtainable Gate");
            SetProfileStatus("Unobtainable Gate");
            UpdateNavigationState(_profileEditor);
            AutoLoadDatabaseRows("Load Gates", LoadUnobtainableRows, _unobtainableRows);
        }

        private void ShowUnlockPresetsPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowUnlockPresetsPage);
                return;
            }
            HidePages();
            _unlockPresetsPage.Visible = true;
            _unlockPresetsPage.BringToFront();
            SetStatus("Unlock Presets");
            SetProfileStatus("Unlock Presets");
            UpdateNavigationState(_profileEditor);
        }

        private void ShowNewTagsPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowNewTagsPage);
                return;
            }
            HidePages();
            _newTagsPage.Visible = true;
            _newTagsPage.BringToFront();
            SetStatus("Clear New Tags");
            SetProfileStatus("Clear New Tags");
            UpdateNavigationState(_profileEditor);
        }

        private void ShowInstallFlagsPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowInstallFlagsPage);
                return;
            }
            HidePages();
            _installFlagsPage.Visible = true;
            _installFlagsPage.BringToFront();
            SetStatus("Install Flags");
            SetProfileStatus("Install Flags");
            UpdateNavigationState(_profileEditor);
            AutoLoadDatabaseRows("Load Flags", LoadInstallFlagRows, _installFlagRows);
        }

        private void AutoLoadDatabaseRows(string actionName, Action loadAction, List<DatabaseCarRow> currentRows)
        {
            if (loadAction == null)
                return;
            if (currentRows != null && currentRows.Count > 0)
                return;

            RunWorker(actionName, loadAction);
        }

        private void ApplyDecalUnlocker()
        {
            var db = RequireDatabase();
            ValidateDecalUnlockerSchema(db);
            EnsureDecalUnlockerIndexes(db);

            var missingBefore = CountMissingDecalMakeMappings(db);
            var sortRowsBefore = CountDecalSortRows(db);
            var invalidMappingsBefore = CountInvalidDecalMappings(db);
            var wingBlocksBefore = CountDecalUnlockerWhere(db, "CarExceptions", BuildDecalUnlockerNonZeroWhere(db, "CarExceptions", "NoDecalsWingStock", "NoDecalsWingAftermarket"));
            var vinylSortBefore = CountDecalUnlockerWhere(db, "Livery_VinylsDecals", "[SortOrder] IS NULL OR [SortOrder] = 0");
            var categorySortBefore = CountDecalUnlockerWhere(db, "Livery_Categories", "[sortOrder] IS NULL OR [sortOrder] = 0");
            var materialReleaseBefore = CountDecalUnlockerWhere(db, "List_LiveryMaterials", BuildDecalUnlockerReleaseWhere(db, "List_LiveryMaterials"));
            var paintGroupReleaseBefore = CountDecalUnlockerWhere(db, "PaintableGroups", BuildDecalUnlockerReleaseWhere(db, "PaintableGroups"));
            var stripeReleaseBefore = CountDecalUnlockerWhere(db, "LiveryStripes", BuildDecalUnlockerReleaseWhere(db, "LiveryStripes"));
            var colorBlocksBefore = CountDecalUnlockerWhere(db, "CarColorTypeExceptions", BuildDecalUnlockerColorExceptionWhere(db));

            var touched = 0;
            var activeSavepoint = false;
            try
            {
                ExecuteAutoshowSql(db, "SAVEPOINT luna_decal_unlocker;");
                activeSavepoint = true;

                touched += ApplyDecalMakeMappings(db);
                touched += FixDecalSortAndReleaseRows(db);
                touched += UnlockDecalCarExceptions(db);

                var missingAfter = CountMissingDecalMakeMappings(db);
                var sortRowsAfter = CountDecalSortRows(db);
                var invalidMappingsAfter = CountInvalidDecalMappings(db);
                var wingBlocksAfter = CountDecalUnlockerWhere(db, "CarExceptions", BuildDecalUnlockerNonZeroWhere(db, "CarExceptions", "NoDecalsWingStock", "NoDecalsWingAftermarket"));
                var vinylSortAfter = CountDecalUnlockerWhere(db, "Livery_VinylsDecals", "[SortOrder] IS NULL OR [SortOrder] = 0");
                var categorySortAfter = CountDecalUnlockerWhere(db, "Livery_Categories", "[sortOrder] IS NULL OR [sortOrder] = 0");
                var materialReleaseAfter = CountDecalUnlockerWhere(db, "List_LiveryMaterials", BuildDecalUnlockerReleaseWhere(db, "List_LiveryMaterials"));
                var paintGroupReleaseAfter = CountDecalUnlockerWhere(db, "PaintableGroups", BuildDecalUnlockerReleaseWhere(db, "PaintableGroups"));
                var stripeReleaseAfter = CountDecalUnlockerWhere(db, "LiveryStripes", BuildDecalUnlockerReleaseWhere(db, "LiveryStripes"));
                var colorBlocksAfter = CountDecalUnlockerWhere(db, "CarColorTypeExceptions", BuildDecalUnlockerColorExceptionWhere(db));

                var failures = new List<string>();
                if (missingAfter > 0)
                    Log("Decal Unlocker warning: " + missingAfter.ToString(CultureInfo.InvariantCulture) + " supported make/decal mapping row(s) still look locked after the live apply.");
                if (wingBlocksAfter > 0) failures.Add(wingBlocksAfter.ToString(CultureInfo.InvariantCulture) + " wing decal exception row(s)");
                if (vinylSortAfter > 0) failures.Add(vinylSortAfter.ToString(CultureInfo.InvariantCulture) + " vinyl/decal sort row(s)");
                if (categorySortAfter > 0) failures.Add(categorySortAfter.ToString(CultureInfo.InvariantCulture) + " livery category sort row(s)");
                if (materialReleaseAfter > 0) failures.Add(materialReleaseAfter.ToString(CultureInfo.InvariantCulture) + " material release row(s)");
                if (paintGroupReleaseAfter > 0) failures.Add(paintGroupReleaseAfter.ToString(CultureInfo.InvariantCulture) + " paint group release row(s)");
                if (stripeReleaseAfter > 0) failures.Add(stripeReleaseAfter.ToString(CultureInfo.InvariantCulture) + " stripe release row(s)");
                if (colorBlocksAfter > 0) failures.Add(colorBlocksAfter.ToString(CultureInfo.InvariantCulture) + " color exception row(s)");
                if (failures.Count > 0)
                    throw new InvalidOperationException("Decal Unlocker still sees locked supported row(s): " + string.Join(", ", failures.ToArray()) + ".");

                ExecuteAutoshowSql(db, "RELEASE SAVEPOINT luna_decal_unlocker;");
                activeSavepoint = false;

                var addedMappings = Math.Max(0L, missingBefore - missingAfter);
                var repairedMappings = Math.Max(Math.Max(0L, sortRowsBefore - sortRowsAfter), Math.Max(0L, invalidMappingsBefore - invalidMappingsAfter));
                var fixedRows =
                    repairedMappings +
                    Math.Max(0L, wingBlocksBefore - wingBlocksAfter) +
                    Math.Max(0L, vinylSortBefore - vinylSortAfter) +
                    Math.Max(0L, categorySortBefore - categorySortAfter) +
                    Math.Max(0L, materialReleaseBefore - materialReleaseAfter) +
                    Math.Max(0L, paintGroupReleaseBefore - paintGroupReleaseAfter) +
                    Math.Max(0L, stripeReleaseBefore - stripeReleaseAfter) +
                    Math.Max(0L, colorBlocksBefore - colorBlocksAfter);
                var total = db.QueryScalarLong("SELECT COUNT(*) FROM Livery_DecalsSortOrder") ?? 0;
                Log("Decal Unlocker verified. Added " + addedMappings.ToString(CultureInfo.InvariantCulture) +
                    " supported make/decal mapping row(s), checked " + repairedMappings.ToString(CultureInfo.InvariantCulture) +
                    " existing decal row(s), fixed " + fixedRows.ToString(CultureInfo.InvariantCulture) +
                    " supported decal/livery gate row(s), total sort rows: " + total.ToString(CultureInfo.InvariantCulture) + ".");
                SetProfileStatus("Decals unlocked");
                ShowInfo("Decal Unlocker completed." + Environment.NewLine + Environment.NewLine +
                    "Added " + addedMappings.ToString(CultureInfo.InvariantCulture) + " missing supported make/decal row(s)." + Environment.NewLine +
                    "Checked " + repairedMappings.ToString(CultureInfo.InvariantCulture) + " existing decal row(s)." + Environment.NewLine +
                    "Fixed " + fixedRows.ToString(CultureInfo.InvariantCulture) + " supported decal/livery gate row(s)." + Environment.NewLine + Environment.NewLine +
                    "Reopen the paint, decal, vinyl, or garage editor so FH6 refreshes the list.");
            }
            catch
            {
                if (activeSavepoint)
                {
                    try { ExecuteAutoshowSql(db, "ROLLBACK TO SAVEPOINT luna_decal_unlocker;"); }
                    catch (Exception ex) { Log("Decal Unlocker rollback warning: " + ex.Message); }
                    try { ExecuteAutoshowSql(db, "RELEASE SAVEPOINT luna_decal_unlocker;"); }
                    catch (Exception ex) { Log("Decal Unlocker rollback cleanup warning: " + ex.Message); }
                }
                throw;
            }

            if (touched == 0)
                Log("Decal Unlocker found everything already open.");
        }

        private void ValidateDecalUnlockerSchema(RemoteDatabase db)
        {
            var requiredColumns = new[]
            {
                new[] { "List_CarMake", "ID" },
                new[] { "Livery_DecalsSortOrder", "ID" },
                new[] { "Livery_DecalsSortOrder", "MakeID" },
                new[] { "Livery_DecalsSortOrder", "Sequence" },
                new[] { "Livery_DecalsSortOrder", "Livery_DecalID" },
                new[] { "Livery_Decals", "ID" }
            };

            foreach (var item in requiredColumns)
            {
                if (!db.TableExists(item[0]) || !db.ColumnExists(item[0], item[1]))
                    throw new InvalidOperationException(item[0] + "." + item[1] + " was not found in this FH6 database.");
            }

            if (!db.TableExists("List_CarMake") || !db.ColumnExists("List_CarMake", "ID"))
                throw new InvalidOperationException("List_CarMake.ID was not found in this FH6 database.");
        }

        private int ApplyDecalMakeMappings(RemoteDatabase db)
        {
            var missingBefore = CountMissingDecalMakeMappings(db);
            var sortRowsBefore = CountDecalSortRows(db);
            var invalidBefore = CountInvalidDecalMappings(db);
            ExecuteAutoshowSql(db, "CREATE TABLE IF NOT EXISTS _backup_Database_DecalUnlocker_Livery_DecalsSortOrder AS SELECT * FROM Livery_DecalsSortOrder;");
            ExecuteKnownWorkingDecalInsert(db);

            var firstPassMissing = CountMissingDecalMakeMappings(db);
            var firstPassRows = CountDecalSortRows(db);
            if (firstPassMissing > 0)
            {
                var firstPassAdded = Math.Max(0L, firstPassRows - sortRowsBefore);
                Log("Decal Unlocker: first insert added " + firstPassAdded.ToString(CultureInfo.InvariantCulture) +
                    " row(s); retrying remaining " + firstPassMissing.ToString(CultureInfo.InvariantCulture) +
                    " row(s) with FH6-compatible insert.");
                ExecuteFallbackDecalInsert(db);
            }

            if (db.ColumnExists("Livery_DecalsSortOrder", "ID"))
                ExecuteAutoshowSql(db, "UPDATE Livery_DecalsSortOrder SET Sequence = COALESCE(NULLIF(Sequence, 0), Livery_DecalID, ID, rowid) WHERE Sequence IS NULL OR Sequence = 0;");
            else
                ExecuteAutoshowSql(db, "UPDATE Livery_DecalsSortOrder SET Sequence = COALESCE(NULLIF(Sequence, 0), Livery_DecalID, rowid) WHERE Sequence IS NULL OR Sequence = 0;");

            var missingAfter = CountMissingDecalMakeMappings(db);
            var sortRowsAfter = CountDecalSortRows(db);
            var invalidAfter = CountInvalidDecalMappings(db);
            return missingAfter < missingBefore || sortRowsAfter > sortRowsBefore || invalidAfter < invalidBefore ? 1 : 0;
        }

        private void ExecuteKnownWorkingDecalInsert(RemoteDatabase db)
        {
            ExecuteAutoshowSql(db,
                "INSERT INTO Livery_DecalsSortOrder (ID, MakeID, Sequence, Livery_DecalID) " +
                "SELECT " +
                "(SELECT IFNULL(MAX(ID), 0) FROM Livery_DecalsSortOrder) + ROW_NUMBER() OVER (ORDER BY m.ID, d.ID) AS ID, " +
                "m.ID AS MakeID, " +
                "(SELECT IFNULL(MAX(Sequence), 0) FROM Livery_DecalsSortOrder WHERE MakeID = m.ID) + " +
                "ROW_NUMBER() OVER (PARTITION BY m.ID ORDER BY d.ID) AS Sequence, " +
                "d.ID AS Livery_DecalID " +
                "FROM List_CarMake m " +
                "CROSS JOIN Livery_Decals d " +
                "LEFT JOIN Livery_DecalsSortOrder s ON s.MakeID = m.ID AND s.Livery_DecalID = d.ID " +
                "WHERE s.ID IS NULL;");
        }

        private void ExecuteFallbackDecalInsert(RemoteDatabase db)
        {
            ExecuteAutoshowSql(db,
                "INSERT INTO Livery_DecalsSortOrder (ID, MakeID, Sequence, Livery_DecalID) " +
                "SELECT " +
                "(SELECT IFNULL(MAX(ID), 0) FROM Livery_DecalsSortOrder) + (m.ID * 100000) + d.ID AS ID, " +
                "m.ID AS MakeID, " +
                "(SELECT IFNULL(MAX(Sequence), 0) FROM Livery_DecalsSortOrder WHERE MakeID = m.ID) + d.ID AS Sequence, " +
                "d.ID AS Livery_DecalID " +
                "FROM List_CarMake m " +
                "CROSS JOIN Livery_Decals d " +
                "LEFT JOIN Livery_DecalsSortOrder s ON s.MakeID = m.ID AND s.Livery_DecalID = d.ID " +
                "WHERE s.ID IS NULL;");
        }

        private int FixDecalSortAndReleaseRows(RemoteDatabase db)
        {
            var touched = 0;
            if (db.TableExists("Livery_VinylsDecals") && db.ColumnExists("Livery_VinylsDecals", "SortOrder"))
            {
                ExecuteAutoshowSql(db, "CREATE TABLE IF NOT EXISTS _backup_Database_DecalUnlocker_LiveryVinylsDecals AS SELECT * FROM Livery_VinylsDecals;");
                if (db.ColumnExists("Livery_VinylsDecals", "ID"))
                    ExecuteAutoshowSql(db, "UPDATE Livery_VinylsDecals SET SortOrder = COALESCE(NULLIF(SortOrder, 0), ID * 10, rowid * 10) WHERE SortOrder IS NULL OR SortOrder = 0;");
                else
                    ExecuteAutoshowSql(db, "UPDATE Livery_VinylsDecals SET SortOrder = COALESCE(NULLIF(SortOrder, 0), rowid * 10) WHERE SortOrder IS NULL OR SortOrder = 0;");
                touched++;
            }

            if (db.TableExists("Livery_Categories") && db.ColumnExists("Livery_Categories", "sortOrder"))
            {
                ExecuteAutoshowSql(db, "CREATE TABLE IF NOT EXISTS _backup_Database_DecalUnlocker_LiveryCategories AS SELECT * FROM Livery_Categories;");
                if (db.ColumnExists("Livery_Categories", "ID"))
                    ExecuteAutoshowSql(db, "UPDATE Livery_Categories SET sortOrder = COALESCE(NULLIF(sortOrder, 0), ID * 1000, rowid * 1000) WHERE sortOrder IS NULL OR sortOrder = 0;");
                else
                    ExecuteAutoshowSql(db, "UPDATE Livery_Categories SET sortOrder = COALESCE(NULLIF(sortOrder, 0), rowid * 1000) WHERE sortOrder IS NULL OR sortOrder = 0;");
                touched++;
            }

            touched += ApplyDecalUnlockerReleaseOpen(db, "List_LiveryMaterials", "_backup_Database_DecalUnlocker_LiveryMaterials");
            touched += ApplyDecalUnlockerReleaseOpen(db, "PaintableGroups", "_backup_Database_DecalUnlocker_PaintableGroups");
            touched += ApplyDecalUnlockerReleaseOpen(db, "LiveryStripes", "_backup_Database_DecalUnlocker_LiveryStripes");
            return touched;
        }

        private int ApplyDecalUnlockerReleaseOpen(RemoteDatabase db, string tableName, string backupName)
        {
            if (!db.TableExists(tableName))
                return 0;

            var touched = 0;
            var tableSql = EscapeIdentifierForSql(tableName);
            ExecuteAutoshowSql(db, "CREATE TABLE IF NOT EXISTS " + EscapeIdentifierForSql(backupName) + " AS SELECT * FROM " + tableSql + ";");
            if (db.ColumnExists(tableName, "releaseOrder"))
            {
                ExecuteAutoshowSql(db, "UPDATE " + tableSql + " SET releaseOrder = 0 WHERE releaseOrder IS NULL OR releaseOrder <> 0;");
                touched++;
            }
            if (db.ColumnExists(tableName, "SortOrder") && db.ColumnExists(tableName, "ID"))
            {
                ExecuteAutoshowSql(db, "UPDATE " + tableSql + " SET SortOrder = COALESCE(NULLIF(SortOrder, 0), ID * 1000) WHERE SortOrder IS NULL OR SortOrder = 0;");
                touched++;
            }
            return touched;
        }

        private int UnlockDecalCarExceptions(RemoteDatabase db)
        {
            var touched = 0;
            if (db.TableExists("CarExceptions"))
            {
                var columns = new[] { "NoDecalsWingStock", "NoDecalsWingAftermarket" }
                    .Where(column => db.ColumnExists("CarExceptions", column))
                    .ToList();
                if (columns.Count > 0)
                {
                    ExecuteAutoshowSql(db, "CREATE TABLE IF NOT EXISTS _backup_Database_DecalUnlocker_CarExceptions AS SELECT * FROM CarExceptions;");
                    ExecuteAutoshowSql(db, "UPDATE CarExceptions SET " + string.Join(", ", columns.Select(column => EscapeIdentifierForSql(column) + " = 0").ToArray()) +
                        " WHERE " + string.Join(" OR ", columns.Select(column => EscapeIdentifierForSql(column) + " IS NULL OR " + EscapeIdentifierForSql(column) + " <> 0").ToArray()) + ";");
                    touched++;
                }
            }

            if (db.TableExists("CarColorTypeExceptions"))
            {
                var sets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                AddColumnSetIfExists(db, "CarColorTypeExceptions", sets, "HideNormalColors", "0");
                AddColumnSetIfExists(db, "CarColorTypeExceptions", sets, "HideManufacturerColors", "0");
                AddColumnSetIfExists(db, "CarColorTypeExceptions", sets, "HideSpecialColors", "0");
                AddColumnSetIfExists(db, "CarColorTypeExceptions", sets, "AllowStockManufacturerColorsForWheels", "1");
                if (sets.Count > 0)
                {
                    ExecuteAutoshowSql(db, "CREATE TABLE IF NOT EXISTS _backup_Database_DecalUnlocker_CarColorTypeExceptions AS SELECT * FROM CarColorTypeExceptions;");
                    ExecuteAutoshowSql(db, "UPDATE CarColorTypeExceptions SET " + string.Join(", ", sets.Values.ToArray()) + ";");
                    touched++;
                }
            }

            return touched;
        }

        private long CountMissingDecalMakeMappings(RemoteDatabase db)
        {
            return db.QueryScalarLong(
                "SELECT COUNT(*) FROM List_CarMake m " +
                "CROSS JOIN Livery_Decals d " +
                "LEFT JOIN Livery_DecalsSortOrder s ON s.MakeID = m.ID AND s.Livery_DecalID = d.ID " +
                "WHERE s.ID IS NULL;") ?? 0;
        }

        private long CountDecalSortRows(RemoteDatabase db)
        {
            if (!db.TableExists("Livery_DecalsSortOrder"))
                return 0;
            return db.QueryScalarLong("SELECT COUNT(*) FROM Livery_DecalsSortOrder;") ?? 0;
        }

        private long CountInvalidDecalMappings(RemoteDatabase db)
        {
            if (!db.TableExists("Livery_DecalsSortOrder") || !db.TableExists("Livery_Decals") || !db.ColumnExists("Livery_Decals", "ID"))
                return 0;
            return db.QueryScalarLong("SELECT COUNT(*) FROM Livery_DecalsSortOrder WHERE Livery_DecalID IS NULL OR Livery_DecalID NOT IN (SELECT ID FROM Livery_Decals WHERE ID IS NOT NULL);") ?? 0;
        }

        private void EnsureDecalUnlockerIndexes(RemoteDatabase db)
        {
            try
            {
                ExecuteAutoshowSql(db, "CREATE INDEX IF NOT EXISTS i_Luna_DecalUnlocker_MakeDecal ON Livery_DecalsSortOrder(MakeID, Livery_DecalID);");
            }
            catch (Exception ex)
            {
                Log("Decal Unlocker index warning: " + ex.Message);
            }
        }

        private long CountDecalUnlockerWhere(RemoteDatabase db, string tableName, string where)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(where) || !db.TableExists(tableName))
                return 0;
            return db.QueryScalarLong("SELECT COUNT(*) FROM " + EscapeIdentifierForSql(tableName) + " WHERE " + where + ";") ?? 0;
        }

        private string BuildDecalUnlockerReleaseWhere(RemoteDatabase db, string tableName)
        {
            var checks = new List<string>();
            if (db.TableExists(tableName) && db.ColumnExists(tableName, "releaseOrder"))
                checks.Add("[releaseOrder] IS NULL OR [releaseOrder] <> 0");
            if (db.TableExists(tableName) && db.ColumnExists(tableName, "SortOrder"))
                checks.Add("[SortOrder] IS NULL OR [SortOrder] = 0");
            return string.Join(" OR ", checks.ToArray());
        }

        private string BuildDecalUnlockerNonZeroWhere(RemoteDatabase db, string tableName, params string[] columns)
        {
            if (!db.TableExists(tableName))
                return string.Empty;
            return string.Join(" OR ", (columns ?? new string[0])
                .Where(column => db.ColumnExists(tableName, column))
                .Select(column => EscapeIdentifierForSql(column) + " IS NULL OR " + EscapeIdentifierForSql(column) + " <> 0")
                .ToArray());
        }

        private string BuildDecalUnlockerColorExceptionWhere(RemoteDatabase db)
        {
            if (!db.TableExists("CarColorTypeExceptions"))
                return string.Empty;
            var checks = new List<string>();
            if (db.ColumnExists("CarColorTypeExceptions", "HideNormalColors"))
                checks.Add("[HideNormalColors] IS NULL OR [HideNormalColors] <> 0");
            if (db.ColumnExists("CarColorTypeExceptions", "HideManufacturerColors"))
                checks.Add("[HideManufacturerColors] IS NULL OR [HideManufacturerColors] <> 0");
            if (db.ColumnExists("CarColorTypeExceptions", "HideSpecialColors"))
                checks.Add("[HideSpecialColors] IS NULL OR [HideSpecialColors] <> 0");
            if (db.ColumnExists("CarColorTypeExceptions", "AllowStockManufacturerColorsForWheels"))
                checks.Add("[AllowStockManufacturerColorsForWheels] IS NULL OR [AllowStockManufacturerColorsForWheels] <> 1");
            return string.Join(" OR ", checks.ToArray());
        }

        private void FixAllGarageClasses()
        {
            var db = RequireDatabase();
            var touched = ApplyPerformanceClassRepair(db);
            if (touched == 0)
                throw new InvalidOperationException("No supported class columns were found.");

            try { LoadCarTablesFromDatabase(db); }
            catch { }

            SetProfileStatus("Classes fixed");
            ShowInfo("Garage classes repaired." + Environment.NewLine + Environment.NewLine +
                "Reload the garage screen so FH6 refreshes the class badges.");
        }

        private int ApplyPerformanceClassRepair(RemoteDatabase db)
        {
            if (!db.TableExists("CarClasses") ||
                !db.ColumnExists("CarClasses", "Id") ||
                !db.ColumnExists("CarClasses", "MaxPerformanceIndex"))
            {
                throw new InvalidOperationException("CarClasses thresholds were not found.");
            }

            var touched = 0;
            if (db.TableExists("Data_Car") &&
                db.ColumnExists("Data_Car", "ClassID") &&
                db.ColumnExists("Data_Car", "PerformanceIndex"))
            {
                ExecuteAutoshowSql(db, "CREATE TABLE IF NOT EXISTS _backup_Database_FixClasses_DataCar AS SELECT Id, ClassID FROM Data_Car;");
                var dataClass = BuildPerformanceClassExpression(db, "[PerformanceIndex]", "COALESCE([ClassID], 0)");
                ExecuteAutoshowSql(db, "UPDATE Data_Car SET ClassID = " + dataClass + " WHERE Id IS NOT NULL AND Id > 0 AND PerformanceIndex IS NOT NULL;");
                LogCount(db, "Data_Car classes still blank", "SELECT count(*) FROM Data_Car WHERE Id IS NOT NULL AND Id > 0 AND ClassID IS NULL");
                touched++;
            }

            touched += ApplyGaragePerformanceClassRepair(db, "Profile0_Career_Garage", "_backup_Database_FixClasses_Profile0Garage");
            touched += ApplyGaragePerformanceClassRepair(db, "NewProfile_Career_Garage", "_backup_Database_FixClasses_NewProfileGarage");
            return touched;
        }

        private int ApplyGaragePerformanceClassRepair(RemoteDatabase db, string tableName, string backupName)
        {
            if (!db.TableExists(tableName) ||
                !db.ColumnExists(tableName, "CarId") ||
                !db.ColumnExists(tableName, "ClassID"))
            {
                return 0;
            }

            var tableSql = EscapeIdentifierForSql(tableName);
            ExecuteAutoshowSql(db, "CREATE TABLE IF NOT EXISTS " + EscapeIdentifierForSql(backupName) + " AS SELECT rowid AS _rowid_, CarId, ClassID FROM " + tableSql + ";");

            var piExpr = db.ColumnExists(tableName, "PerformanceIndex")
                ? "COALESCE([PerformanceIndex], (SELECT data.[PerformanceIndex] FROM Data_Car data WHERE data.Id = [CarId]))"
                : "(SELECT data.[PerformanceIndex] FROM Data_Car data WHERE data.Id = [CarId])";
            var fallback = "COALESCE([ClassID], (SELECT data.[ClassID] FROM Data_Car data WHERE data.Id = [CarId]), 0)";
            var classExpr = BuildPerformanceClassExpression(db, piExpr, fallback);
            ExecuteAutoshowSql(db,
                "UPDATE " + tableSql + " SET ClassID = " + classExpr +
                " WHERE CarId IS NOT NULL AND CarId > 0 AND EXISTS (SELECT 1 FROM Data_Car data WHERE data.Id = " + tableSql + ".[CarId]);");
            LogCount(db, tableName + " classes still blank", "SELECT count(*) FROM " + tableSql + " WHERE CarId IS NOT NULL AND CarId > 0 AND ClassID IS NULL");
            return 1;
        }

        private void ToggleClassRestrictions(Button feedbackButton)
        {
            if (_classRestrictionsOn)
            {
                if (!ConfirmClassRestrictionRestore())
                    return;

                RunWorker("Restore All Car Classes", RestoreClassRestrictionsFromRepair, feedbackButton);
                return;
            }

            var classId = ShowClassRestrictionClassPrompt();
            if (!classId.HasValue)
                return;

            if (!ConfirmClassRestrictionApply(classId.Value))
                return;

            RunWorker("Class Restrictions", delegate { ApplyClassRestrictionSelection(classId.Value); }, feedbackButton);
        }

        private void ApplyClassRestrictionSelection(int classId)
        {
            var db = RequireDatabase();
            var touched = ApplyClassRestrictionClass(db, classId);
            if (touched == 0)
                throw new InvalidOperationException("No supported class restriction columns were found.");

            try { LoadCarTablesFromDatabase(db); }
            catch { }

            UpdateClassRestrictionSwitchButton(true);
            SetProfileStatus("Class restrictions " + FormatCarClassId(classId));
            ShowInfo("Class Restrictions turned on." + Environment.NewLine + Environment.NewLine +
                "All supported car class fields were set to " + FormatCarClassId(classId) + "." + Environment.NewLine +
                "Reload the garage or event menu so FH6 refreshes the rules.");
        }

        private void RestoreClassRestrictionsFromRepair()
        {
            var db = RequireDatabase();
            var touched = ApplyPerformanceClassRepair(db);
            if (touched == 0)
                throw new InvalidOperationException("No supported class columns were found.");

            try { LoadCarTablesFromDatabase(db); }
            catch { }

            UpdateClassRestrictionSwitchButton(false);
            SetProfileStatus("All classes restored");
            ShowInfo("Class Restrictions turned off." + Environment.NewLine + Environment.NewLine +
                "All car classes were restored from their performance rating.");
        }

        private void UpdateClassRestrictionSwitchButton(bool enabled)
        {
            _classRestrictionsOn = enabled;
            if (InvokeRequired)
            {
                BeginInvoke((Action)(delegate { UpdateClassRestrictionSwitchButton(enabled); }));
                return;
            }
            if (_classRestrictionSwitchButton == null || _classRestrictionSwitchButton.IsDisposed)
                return;

            _classRestrictionSwitchButton.Text = TranslateDynamicUi(enabled ? "Class Restrictions: ON" : "Class Restrictions: OFF");
            MakeAccentButton(_classRestrictionSwitchButton, enabled ? AccentGreen : AccentRed);
        }

        private int? ShowClassRestrictionClassPrompt()
        {
            if (InvokeRequired)
                return (int?)Invoke(new Func<int?>(ShowClassRestrictionClassPrompt));

            using (var dialog = new Form())
            {
                dialog.Text = "Class Restrictions";
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.BackColor = AppBackground;
                dialog.ForeColor = TextPrimary;
                dialog.Font = new Font("Segoe UI", 9F);
                dialog.ClientSize = new Size(420, 214);

                var card = new ModernPanel();
                card.Location = new Point(14, 14);
                card.Size = new Size(392, 186);
                card.FillColor = Surface;
                card.BorderColor = Border;
                card.CornerRadius = 14;
                dialog.Controls.Add(card);

                var title = new Label();
                title.Text = "Choose Class";
                title.Font = new Font("Segoe UI Semibold", 13F);
                title.ForeColor = TextPrimary;
                title.BackColor = Surface;
                title.Location = new Point(18, 14);
                title.Size = new Size(340, 26);
                card.Controls.Add(title);

                var note = new Label();
                note.Text = "Pick what class every supported car should show while Class Restrictions is on.";
                note.Font = new Font("Segoe UI", 9F);
                note.ForeColor = TextMuted;
                note.BackColor = Surface;
                note.Location = new Point(18, 48);
                note.Size = new Size(350, 42);
                card.Controls.Add(note);

                var combo = MakeEditorCombo(18, 98, 350);
                foreach (var option in GetCarClassOptions())
                    combo.Items.Add(option);
                SetComboSelectedById(combo, 3);
                card.Controls.Add(combo);

                var cancel = MakeButton("Cancel", 152, 142, 94, 30);
                cancel.DialogResult = DialogResult.Cancel;
                card.Controls.Add(cancel);

                var apply = MakeButton("Apply", 258, 142, 110, 30);
                MakeAccentButton(apply, AccentBlue);
                apply.Click += delegate
                {
                    if (combo.SelectedItem == null)
                    {
                        ShowTranslatedMessageBox(dialog, "Select a class first.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    dialog.DialogResult = DialogResult.OK;
                    dialog.Close();
                };
                card.Controls.Add(apply);

                dialog.AcceptButton = apply;
                dialog.CancelButton = cancel;

                PrepareDialogForLanguage(dialog);
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return null;

                var selected = combo.SelectedItem as NamedOption;
                return selected == null ? (int?)null : selected.Id;
            }
        }

        private bool ConfirmClassRestrictionApply(int classId)
        {
            if (InvokeRequired)
                return (bool)Invoke(new Func<int, bool>(ConfirmClassRestrictionApply), classId);

            var message = "Apply " + FormatCarClassId(classId) + " to every supported car class field?" + Environment.NewLine + Environment.NewLine +
                "Luna will keep a backup and you can turn this OFF to restore classes from PI.";
            return ShowTranslatedMessageBox(this, message, MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK;
        }

        private bool ConfirmClassRestrictionRestore()
        {
            if (InvokeRequired)
                return (bool)Invoke(new Func<bool>(ConfirmClassRestrictionRestore));

            var message = "Turn Class Restrictions OFF and restore every supported car class from PI?";
            return ShowTranslatedMessageBox(this, message, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK;
        }

        private int ApplyClassRestrictionClass(RemoteDatabase db, int classId)
        {
            var touched = 0;
            touched += SetAllCarClassColumn(db, "Data_Car", "Id", "_backup_Database_ClassRestrict_DataCar", classId);
            touched += SetAllCarClassColumn(db, "Profile0_Career_Garage", "CarId", "_backup_Database_ClassRestrict_Profile0Garage", classId);
            touched += SetAllCarClassColumn(db, "NewProfile_Career_Garage", "CarId", "_backup_Database_ClassRestrict_NewProfileGarage", classId);
            touched += ApplyClassRestrictionUnlocks(db);
            return touched;
        }

        private int ApplyClassRestrictionUnlocks(RemoteDatabase db)
        {
            var touched = 0;

            if (db.TableExists("Profile0_CareerRaceCollections") && db.ColumnExists("Profile0_CareerRaceCollections", "CarRestrictions"))
            {
                ExecuteAutoshowSql(db, "CREATE TABLE IF NOT EXISTS _backup_Database_RemoveClassRestrict_Profile0RaceCollections AS SELECT * FROM Profile0_CareerRaceCollections;");
                ExecuteAutoshowSql(db, "UPDATE Profile0_CareerRaceCollections SET CarRestrictions = NULL WHERE CarRestrictions IS NOT NULL AND CarRestrictions <> '';");
                touched++;
            }
            if (db.TableExists("NewProfile_CareerRaceCollections") && db.ColumnExists("NewProfile_CareerRaceCollections", "CarRestrictions"))
            {
                ExecuteAutoshowSql(db, "CREATE TABLE IF NOT EXISTS _backup_Database_RemoveClassRestrict_NewProfileRaceCollections AS SELECT * FROM NewProfile_CareerRaceCollections;");
                ExecuteAutoshowSql(db, "UPDATE NewProfile_CareerRaceCollections SET CarRestrictions = NULL WHERE CarRestrictions IS NOT NULL AND CarRestrictions <> '';");
                touched++;
            }
            if (db.TableExists("EventBlueprintFlyerRestrictions") && db.ColumnExists("EventBlueprintFlyerRestrictions", "RestrictionId"))
            {
                ExecuteAutoshowSql(db, "CREATE TABLE IF NOT EXISTS _backup_Database_RemoveClassRestrict_EventFlyerRestrictions AS SELECT * FROM EventBlueprintFlyerRestrictions;");
                ExecuteAutoshowSql(db, "UPDATE EventBlueprintFlyerRestrictions SET RestrictionId = -1 WHERE RestrictionId IS NOT NULL AND RestrictionId <> -1;");
                touched++;
                LogCount(db, "Event flyer restrictions still specific", "SELECT count(*) FROM EventBlueprintFlyerRestrictions WHERE RestrictionId IS NOT NULL AND RestrictionId <> -1");
            }
            return touched;
        }

        private int SetAllCarClassColumn(RemoteDatabase db, string tableName, string idColumnName, string backupName, int classId)
        {
            if (!db.TableExists(tableName))
                return 0;

            var sets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var classSql = classId.ToString(CultureInfo.InvariantCulture);
            AddColumnSetIfExists(db, tableName, sets, "ClassID", classSql);
            AddColumnSetIfExists(db, tableName, sets, "ClassId", classSql);
            if (sets.Count == 0)
                return 0;

            var tableSql = EscapeIdentifierForSql(tableName);
            ExecuteAutoshowSql(db, "CREATE TABLE IF NOT EXISTS " + EscapeIdentifierForSql(backupName) + " AS SELECT rowid AS _rowid_, * FROM " + tableSql + ";");

            var where = string.Empty;
            if (!string.IsNullOrWhiteSpace(idColumnName) && db.ColumnExists(tableName, idColumnName))
                where = " WHERE " + EscapeIdentifierForSql(idColumnName) + " IS NOT NULL AND " + EscapeIdentifierForSql(idColumnName) + " > 0";

            ExecuteAutoshowSql(db, "UPDATE " + tableSql + " SET " + string.Join(", ", sets.Values.ToArray()) + where + ";");
            LogCount(db, tableName + " class mismatch", "SELECT count(*) FROM " + tableSql + where + (where.Length == 0 ? " WHERE " : " AND ") + "(" + string.Join(" OR ", sets.Keys.Select(column => EscapeIdentifierForSql(column) + " IS NULL OR " + EscapeIdentifierForSql(column) + " <> " + classSql).ToArray()) + ")");
            return 1;
        }
    }
}
