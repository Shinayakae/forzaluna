using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace ForzaHorizon6AutoshowUnlocker
{
    public sealed partial class MainForm
    {
        private StatusDotToggle _rolePlayGasToggle;
        private ModernPanel _rolePlayGasModeField;
        private Label _rolePlayGasModeLabel;
        private TextBox _rolePlayGasAmountBox;
        private Button _rolePlayGasApplyButton;
        private Button _rolePlayGasResetButton;
        private Label _rolePlayGasStatusLabel;
        private GasOverlayForm _gasOverlayForm;
        private bool _rolePlayGasEnabled;
        private bool _rolePlayGasDrainByDistance = true;
        private float _rolePlayGasAmount = 100F;
        private float _rolePlayGasPercent = 100F;
        private Rectangle _gasOverlayBounds = Rectangle.Empty;

        private void BuildRolePlayPage()
        {
            _rolePlayPage.AutoScroll = true;
            AddPageHeader(_rolePlayPage, "Role Play", "Fuel simulation and role-play overlays.");

            var card = MakeCard(_rolePlayPage, 0, 108, ContentWidth, 268, "Gas Simulation", "Creates a fuel overlay and drains gas while FH6 is running.");
            card.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var panel = new ModernPanel();
            panel.SetBounds(18, 58, 858, 186);
            panel.FillColor = SurfaceAlt;
            panel.BorderColor = Border;
            panel.CornerRadius = 10;
            panel.BackColor = SurfaceAlt;
            card.Controls.Add(panel);

            var enableLabel = MakeLabel("Gas overlay", 18, 23);
            enableLabel.BackColor = Color.Transparent;
            enableLabel.AutoSize = false;
            enableLabel.Size = new Size(140, 22);
            panel.Controls.Add(enableLabel);

            _rolePlayGasToggle = new StatusDotToggle();
            _rolePlayGasToggle.Size = new Size(46, 24);
            _rolePlayGasToggle.Location = new Point(160, 22);
            _rolePlayGasToggle.BackColor = SurfaceAlt;
            _rolePlayGasToggle.Checked = false;
            _rolePlayGasToggle.CheckedChanged += delegate
            {
                var enabled = _rolePlayGasToggle.Checked;
                if (_rolePlayGasEnabled == enabled)
                    return;
                SetRolePlayGasEnabled(enabled, true);
            };
            panel.Controls.Add(_rolePlayGasToggle);
            SetTranslatedToolTip(_rolePlayGasToggle, "Turns the role-play fuel system and gas overlay on or off.");

            var modeLabel = MakeLabel("Drain mode", 236, 23);
            modeLabel.BackColor = Color.Transparent;
            modeLabel.AutoSize = false;
            modeLabel.Size = new Size(92, 22);
            panel.Controls.Add(modeLabel);

            _rolePlayGasModeField = MakeRoundedSelectField(panel, 334, 16, 150, 34, GetRolePlayGasModeText(), out _rolePlayGasModeLabel);
            _rolePlayGasModeField.Click += delegate { PickRolePlayGasMode(); };
            _rolePlayGasModeLabel.Click += delegate { PickRolePlayGasMode(); };
            SetTranslatedToolTip(_rolePlayGasModeField, "Distance drains by kilometers driven. Minutes drains by real time.");

            var amountLabel = MakeLabel("Full tank", 512, 23);
            amountLabel.BackColor = Color.Transparent;
            amountLabel.AutoSize = false;
            amountLabel.Size = new Size(82, 22);
            panel.Controls.Add(amountLabel);

            _rolePlayGasAmountBox = new TextBox();
            _rolePlayGasAmountBox.Text = FormatFloat(_rolePlayGasAmount);
            _rolePlayGasAmountBox.Location = new Point(598, 17);
            _rolePlayGasAmountBox.Size = new Size(92, 30);
            _rolePlayGasAmountBox.TextAlign = HorizontalAlignment.Center;
            StyleTextBox(_rolePlayGasAmountBox);
            panel.Controls.Add(_rolePlayGasAmountBox);
            SetTranslatedToolTip(_rolePlayGasAmountBox, "Distance mode uses kilometers. Minutes mode uses minutes.");

            _rolePlayGasApplyButton = MakeButton("Apply", 710, 16, 116, 34);
            MakeAccentButton(_rolePlayGasApplyButton, AccentGreen);
            _rolePlayGasApplyButton.Click += delegate { ApplyRolePlayGasFromUi(); };
            panel.Controls.Add(_rolePlayGasApplyButton);

            _rolePlayGasResetButton = MakeButton("Refuel", 18, 78, 132, 34);
            MakeAccentButton(_rolePlayGasResetButton, AccentBlue);
            _rolePlayGasResetButton.Click += delegate { RefuelRolePlayGasTank(); };
            panel.Controls.Add(_rolePlayGasResetButton);

            var note = MakeBodyLabel("Empty fuel holds the vehicle stopped by zeroing live velocity until you refuel or turn the feature off.", 172, 73, 650, 42);
            note.BackColor = Color.Transparent;
            note.ForeColor = TextMuted;
            panel.Controls.Add(note);

            _rolePlayGasStatusLabel = MakeBodyLabel("", 18, 132, 808, 32);
            _rolePlayGasStatusLabel.BackColor = Color.Transparent;
            _rolePlayGasStatusLabel.ForeColor = AccentBlue;
            panel.Controls.Add(_rolePlayGasStatusLabel);

            UpdateRolePlayGasUi();
            _rolePlayPage.AutoScrollMinSize = new Size(ContentWidth, 420);
        }

        private void ShowRolePlayPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowRolePlayPage);
                return;
            }
            HidePages();
            _rolePlayPage.Visible = true;
            _rolePlayPage.BringToFront();
            SetStatus("Role Play");
            UpdateRolePlayGasUi();
            UpdateNavigationState(_navRolePlay);
        }

        private void PickRolePlayGasMode()
        {
            var current = GetRolePlayGasModeText();
            using (var dialog = new Form())
            {
                dialog.Text = "Gas Drain Mode";
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.BackColor = AppBackground;
                dialog.ForeColor = TextPrimary;
                dialog.Font = new Font("Segoe UI", 9F);
                dialog.ClientSize = new Size(386, 108);

                string picked = null;
                var distance = (ModernButton)MakeButton("Distance", 18, 24, 160, 48);
                distance.CenterContent = true;
                distance.Selected = string.Equals(current, "Distance", StringComparison.OrdinalIgnoreCase);
                distance.Click += delegate { picked = "Distance"; dialog.DialogResult = DialogResult.OK; dialog.Close(); };
                dialog.Controls.Add(distance);

                var minutes = (ModernButton)MakeButton("Minutes", 206, 24, 160, 48);
                minutes.CenterContent = true;
                minutes.Selected = string.Equals(current, "Minutes", StringComparison.OrdinalIgnoreCase);
                minutes.Click += delegate { picked = "Minutes"; dialog.DialogResult = DialogResult.OK; dialog.Close(); };
                dialog.Controls.Add(minutes);

                PrepareDialogForLanguage(dialog);
                if (dialog.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(picked))
                    return;

                _rolePlayGasDrainByDistance = string.Equals(picked, "Distance", StringComparison.OrdinalIgnoreCase);
                if (_rolePlayGasDrainByDistance && _rolePlayGasAmount <= 0F)
                    _rolePlayGasAmount = 100F;
                if (!_rolePlayGasDrainByDistance && _rolePlayGasAmount <= 0F)
                    _rolePlayGasAmount = 30F;
                UpdateRolePlayGasUi();
                SaveAppSettings();
            }
        }

        private void ApplyRolePlayGasFromUi()
        {
            float amount;
            if (!TryParseRolePlayGasAmount(out amount))
                return;

            _rolePlayGasAmount = amount;
            UpdateRolePlayGasUi();
            SaveAppSettings();
            if (_rolePlayGasEnabled)
                SetRolePlayGasEnabled(true, false);
        }

        private bool TryParseRolePlayGasAmount(out float amount)
        {
            amount = 0F;
            var raw = _rolePlayGasAmountBox == null ? string.Empty : (_rolePlayGasAmountBox.Text ?? string.Empty).Trim();
            if (!float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out amount) ||
                float.IsNaN(amount) ||
                float.IsInfinity(amount) ||
                amount <= 0F)
            {
                ShowInfo("Enter a full tank value greater than 0." + Environment.NewLine + Environment.NewLine +
                    (_rolePlayGasDrainByDistance ? "Distance mode uses kilometers." : "Minutes mode uses minutes."));
                return false;
            }

            amount = Math.Max(0.1F, Math.Min(_rolePlayGasDrainByDistance ? 10000F : 10080F, amount));
            return true;
        }

        private void SetRolePlayGasEnabled(bool enabled, bool save)
        {
            try
            {
                if (enabled)
                {
                    float amount;
                    if (!TryParseRolePlayGasAmount(out amount))
                    {
                        if (_rolePlayGasToggle != null)
                            _rolePlayGasToggle.Checked = false;
                        return;
                    }

                    _rolePlayGasAmount = amount;
                    EnsureRolePlayGasBackendCallback();
                    if (_database == null || !_database.IsAlive)
                        throw new InvalidOperationException("Attach Luna to FH6 first.");
                    _database.SetRolePlayGasRuntimeHook(true, _rolePlayGasDrainByDistance, _rolePlayGasAmount);
                    _rolePlayGasEnabled = true;
                    _rolePlayGasPercent = 100F;
                    ShowGasOverlay();
                    Log("Role Play Gas ON.");
                }
                else
                {
                    if (_database != null && _database.IsAlive)
                        _database.SetRolePlayGasRuntimeHook(false, _rolePlayGasDrainByDistance, _rolePlayGasAmount);
                    _rolePlayGasEnabled = false;
                    HideGasOverlay(save);
                    Log("Role Play Gas OFF.");
                }

                UpdateRolePlayGasUi();
                if (save)
                    SaveAppSettings();
            }
            catch (Exception ex)
            {
                _rolePlayGasEnabled = false;
                if (_rolePlayGasToggle != null && _rolePlayGasToggle.Checked)
                    _rolePlayGasToggle.Checked = false;
                HideGasOverlay(false);
                Log("ERROR: Role Play Gas failed: " + ex.Message);
                ShowInfo("Role Play Gas could not start." + Environment.NewLine + Environment.NewLine + ex.Message);
            }
        }

        private void RefuelRolePlayGasTank()
        {
            _rolePlayGasPercent = 100F;
            if (_database != null && _database.IsAlive)
                _database.ResetRolePlayGasTank();
            RefreshGasOverlay("Refueled");
            UpdateRolePlayGasUi();
            Log("Role Play Gas refueled.");
        }

        private void EnsureRolePlayGasBackendCallback()
        {
            if (_database == null || !_database.IsAlive)
                return;
            _database.SetRolePlayGasStatusCallback(delegate(float percent, string mode, string detail)
            {
                if (IsDisposed)
                    return;
                try
                {
                    BeginInvoke((Action)(delegate
                    {
                        _rolePlayGasPercent = Math.Max(0F, Math.Min(100F, percent));
                        RefreshGasOverlay(detail);
                        UpdateRolePlayGasUi();
                    }));
                }
                catch
                {
                }
            });
        }

        private void UpdateRolePlayGasUi()
        {
            if (_rolePlayGasModeLabel != null)
                _rolePlayGasModeLabel.Text = GetRolePlayGasModeText();
            if (_rolePlayGasAmountBox != null && !_rolePlayGasAmountBox.Focused)
                _rolePlayGasAmountBox.Text = FormatFloat(_rolePlayGasAmount);
            if (_rolePlayGasToggle != null && _rolePlayGasToggle.Checked != _rolePlayGasEnabled)
                _rolePlayGasToggle.Checked = _rolePlayGasEnabled;

            var unit = _rolePlayGasDrainByDistance ? "km" : "min";
            if (_rolePlayGasStatusLabel != null)
            {
                _rolePlayGasStatusLabel.Text =
                    "Fuel " + _rolePlayGasPercent.ToString("0", CultureInfo.InvariantCulture) + "% | " +
                    GetRolePlayGasModeText() + " | Full tank " + FormatFloat(_rolePlayGasAmount) + " " + unit;
                _rolePlayGasStatusLabel.ForeColor = _rolePlayGasPercent <= 0F ? AccentRed : _rolePlayGasPercent < 20F ? Color.FromArgb(251, 191, 36) : AccentBlue;
            }
        }

        private string GetRolePlayGasModeText()
        {
            return _rolePlayGasDrainByDistance ? "Distance" : "Minutes";
        }

        private bool TryLoadRolePlaySetting(string key, string value)
        {
            if (string.Equals(key, "RolePlayGasMode", StringComparison.OrdinalIgnoreCase))
            {
                _rolePlayGasDrainByDistance = !string.Equals(value, "Minutes", StringComparison.OrdinalIgnoreCase);
                return true;
            }
            if (string.Equals(key, "RolePlayGasAmount", StringComparison.OrdinalIgnoreCase))
            {
                float parsed;
                if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) &&
                    parsed > 0F &&
                    !float.IsNaN(parsed) &&
                    !float.IsInfinity(parsed))
                {
                    _rolePlayGasAmount = Math.Max(0.1F, Math.Min(10000F, parsed));
                }
                return true;
            }
            if (string.Equals(key, "GasOverlayBounds", StringComparison.OrdinalIgnoreCase))
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
                        width = Math.Max(220, Math.Min(720, width));
                        height = Math.Max(110, Math.Min(320, height));
                        _gasOverlayBounds = new Rectangle(x, y, width, height);
                    }
                }
                return true;
            }
            return false;
        }

        private string FormatGasOverlayBoundsForSettings()
        {
            var bounds = _gasOverlayForm != null && !_gasOverlayForm.IsDisposed
                ? _gasOverlayForm.Bounds
                : _gasOverlayBounds;
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return string.Empty;
            return bounds.X.ToString(CultureInfo.InvariantCulture) + "," +
                   bounds.Y.ToString(CultureInfo.InvariantCulture) + "," +
                   bounds.Width.ToString(CultureInfo.InvariantCulture) + "," +
                   bounds.Height.ToString(CultureInfo.InvariantCulture);
        }

        private void ShowGasOverlay()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowGasOverlay);
                return;
            }

            if (_gasOverlayForm == null || _gasOverlayForm.IsDisposed)
            {
                _gasOverlayForm = new GasOverlayForm(
                    AppBackground,
                    Surface,
                    SurfaceAlt,
                    Border,
                    TextPrimary,
                    TextMuted,
                    AccentGreen,
                    AccentBlue,
                    AccentRed,
                    LoadEmbeddedImage("gas-station.png"),
                    delegate { SetRolePlayGasEnabled(false, true); },
                    delegate(Rectangle bounds)
                    {
                        _gasOverlayBounds = bounds;
                        if (_rolePlayGasEnabled)
                            SaveAppSettings(true);
                    });
                _gasOverlayForm.Bounds = NormalizeGasOverlayBounds(_gasOverlayBounds);
            }

            if (!_gasOverlayForm.Visible)
                _gasOverlayForm.Show(this);
            _gasOverlayForm.TopMost = true;
            _gasOverlayForm.ApplyPalette(AppBackground, Surface, SurfaceAlt, Border, TextPrimary, TextMuted, AccentGreen, AccentBlue, AccentRed);
            RefreshGasOverlay(null);
        }

        private void HideGasOverlay(bool saveBounds)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(delegate { HideGasOverlay(saveBounds); }));
                return;
            }

            if (_gasOverlayForm != null && !_gasOverlayForm.IsDisposed)
            {
                _gasOverlayBounds = _gasOverlayForm.Bounds;
                _gasOverlayForm.Close();
                _gasOverlayForm.Dispose();
                _gasOverlayForm = null;
            }

            if (saveBounds)
                SaveAppSettings();
        }

        private Rectangle NormalizeGasOverlayBounds(Rectangle requested)
        {
            var workingArea = Screen.PrimaryScreen == null ? new Rectangle(80, 80, 1280, 720) : Screen.PrimaryScreen.WorkingArea;
            var bounds = requested.Width > 0 && requested.Height > 0
                ? requested
                : new Rectangle(workingArea.Left + 64, workingArea.Bottom - 190, 300, 136);

            bounds.Width = Math.Max(220, Math.Min(720, bounds.Width));
            bounds.Height = Math.Max(110, Math.Min(320, bounds.Height));

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
                bounds.Location = new Point(workingArea.Left + 64, Math.Max(workingArea.Top, workingArea.Bottom - bounds.Height - 64));

            return bounds;
        }

        private void RefreshGasOverlay(string detail)
        {
            if (_gasOverlayForm == null || _gasOverlayForm.IsDisposed)
                return;

            var mode = GetRolePlayGasModeText();
            if (string.IsNullOrWhiteSpace(detail))
                detail = _rolePlayGasDrainByDistance
                    ? "Distance tank: " + FormatFloat(_rolePlayGasAmount) + " km"
                    : "Timed tank: " + FormatFloat(_rolePlayGasAmount) + " min";
            _gasOverlayForm.SetFuel(_rolePlayGasPercent, mode, detail);
        }

        private sealed class GasOverlayForm : Form
        {
            private readonly Action _closeAction;
            private readonly Action<Rectangle> _boundsChanged;
            private readonly Image _sourceIcon;
            private Image _tintedIcon;
            private Color _surface;
            private Color _surfaceAlt;
            private Color _border;
            private Color _textPrimary;
            private Color _textMuted;
            private Color _accentGreen;
            private Color _accentBlue;
            private Color _accentRed;
            private string _mode = "Distance";
            private string _detail = "";
            private float _percent = 100F;
            private bool _dragging;
            private bool _resizing;
            private Point _dragStart;
            private Rectangle _startBounds;

            public GasOverlayForm(
                Color background,
                Color surface,
                Color surfaceAlt,
                Color border,
                Color textPrimary,
                Color textMuted,
                Color accentGreen,
                Color accentBlue,
                Color accentRed,
                Image icon,
                Action closeAction,
                Action<Rectangle> boundsChanged)
            {
                _closeAction = closeAction;
                _boundsChanged = boundsChanged;
                _sourceIcon = icon;
                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.Manual;
                ShowInTaskbar = false;
                TopMost = true;
                DoubleBuffered = true;
                MinimumSize = new Size(220, 110);
                Size = new Size(300, 136);
                BackColor = background;
                ApplyPalette(background, surface, surfaceAlt, border, textPrimary, textMuted, accentGreen, accentBlue, accentRed);
            }

            public void ApplyPalette(Color background, Color surface, Color surfaceAlt, Color border, Color textPrimary, Color textMuted, Color accentGreen, Color accentBlue, Color accentRed)
            {
                BackColor = background;
                _surface = surface;
                _surfaceAlt = surfaceAlt;
                _border = border;
                _textPrimary = textPrimary;
                _textMuted = textMuted;
                _accentGreen = accentGreen;
                _accentBlue = accentBlue;
                _accentRed = accentRed;
                if (_tintedIcon != null)
                    _tintedIcon.Dispose();
                _tintedIcon = _sourceIcon == null ? null : TintIconImage(_sourceIcon, _accentBlue);
                Invalidate();
            }

            public void SetFuel(float percent, string mode, string detail)
            {
                _percent = Math.Max(0F, Math.Min(100F, percent));
                _mode = string.IsNullOrWhiteSpace(mode) ? "Distance" : mode;
                _detail = detail ?? string.Empty;
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                var rect = new Rectangle(0, 0, Width - 1, Height - 1);
                using (var path = CreateRoundPath(rect, 14))
                using (var fill = new SolidBrush(_surface))
                using (var pen = new Pen(_border, 1.2F))
                {
                    e.Graphics.FillPath(fill, path);
                    e.Graphics.DrawPath(pen, path);
                }

                using (var titleFont = new Font("Segoe UI Semibold", 11F))
                using (var bodyFont = new Font("Segoe UI", 8.5F))
                using (var closeFont = new Font("Segoe UI Semibold", 10F))
                using (var titleBrush = new SolidBrush(_textPrimary))
                using (var mutedBrush = new SolidBrush(_textMuted))
                using (var closeBrush = new SolidBrush(_accentRed))
                {
                    if (_tintedIcon != null)
                        e.Graphics.DrawImage(_tintedIcon, new Rectangle(18, 22, 48, 48));

                    e.Graphics.DrawString("Gas", titleFont, titleBrush, 78, 18);
                    e.Graphics.DrawString(_mode + " | " + _percent.ToString("0", CultureInfo.InvariantCulture) + "%", bodyFont, mutedBrush, 80, 42);
                    e.Graphics.DrawString(_detail, bodyFont, mutedBrush, new RectangleF(80, 62, Math.Max(40, Width - 108), 24));
                    e.Graphics.DrawString("x", closeFont, closeBrush, Width - 28, 12);
                }

                var barRect = new Rectangle(18, Math.Max(80, Height - 38), Math.Max(20, Width - 48), 14);
                using (var backPath = CreateRoundPath(barRect, 7))
                using (var backBrush = new SolidBrush(_surfaceAlt))
                    e.Graphics.FillPath(backBrush, backPath);

                var fillWidth = (int)Math.Round(barRect.Width * (_percent / 100F));
                if (fillWidth > 0)
                {
                    var fuelColor = _percent <= 0F ? _accentRed : _percent < 20F ? Color.FromArgb(251, 191, 36) : _accentGreen;
                    var fillRect = new Rectangle(barRect.X, barRect.Y, Math.Max(8, fillWidth), barRect.Height);
                    using (var fillPath = CreateRoundPath(fillRect, 7))
                    using (var fillBrush = new SolidBrush(fuelColor))
                        e.Graphics.FillPath(fillBrush, fillPath);
                }

                using (var gripBrush = new SolidBrush(_accentBlue))
                using (var gripFont = new Font("Segoe UI Symbol", 10F))
                    e.Graphics.DrawString("◢", gripFont, gripBrush, Width - 22, Height - 24);
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                if (e.Button != MouseButtons.Left)
                    return;

                if (e.X >= Width - 34 && e.Y <= 38)
                {
                    if (_closeAction != null)
                        _closeAction();
                    return;
                }

                if (e.X >= Width - 30 && e.Y >= Height - 30)
                {
                    _resizing = true;
                    _dragStart = Cursor.Position;
                    _startBounds = Bounds;
                    Capture = true;
                    return;
                }

                _dragging = true;
                _dragStart = Cursor.Position;
                _startBounds = Bounds;
                Capture = true;
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                if (_dragging)
                {
                    var delta = new Size(Cursor.Position.X - _dragStart.X, Cursor.Position.Y - _dragStart.Y);
                    Bounds = new Rectangle(_startBounds.Location + delta, _startBounds.Size);
                    return;
                }
                if (_resizing)
                {
                    var dx = Cursor.Position.X - _dragStart.X;
                    var dy = Cursor.Position.Y - _dragStart.Y;
                    Size = new Size(Math.Max(MinimumSize.Width, _startBounds.Width + dx), Math.Max(MinimumSize.Height, _startBounds.Height + dy));
                    return;
                }
                Cursor = e.X >= Width - 30 && e.Y >= Height - 30 ? Cursors.SizeNWSE : Cursors.Default;
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);
                if (_dragging || _resizing)
                {
                    _dragging = false;
                    _resizing = false;
                    Capture = false;
                    if (_boundsChanged != null)
                        _boundsChanged(Bounds);
                }
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                using (var path = CreateRoundPath(new Rectangle(0, 0, Width, Height), 14))
                    Region = new Region(path);
                Invalidate();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (_tintedIcon != null)
                        _tintedIcon.Dispose();
                    if (_sourceIcon != null)
                        _sourceIcon.Dispose();
                }
                base.Dispose(disposing);
            }

            private static GraphicsPath CreateRoundPath(Rectangle rect, int radius)
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
        }
    }
}
