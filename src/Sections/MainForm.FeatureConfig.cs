using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ForzaHorizon6AutoshowUnlocker
{
    public sealed partial class MainForm
    {
        private string FeatureConfigDir
        {
            get
            {
                var dir = Path.Combine(_resultsDir, "feature_configs");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        private string FeatureAutosavePath
        {
            get { return Path.Combine(FeatureConfigDir, "autosave.txt"); }
        }

        private List<RuntimeProfileFeature> GetConfigurableFeatures()
        {
            var list = new List<RuntimeProfileFeature>(_runtimeFeatureToggles.Keys);
            list.Sort(delegate(RuntimeProfileFeature a, RuntimeProfileFeature b) { return ((int)a).CompareTo((int)b); });
            return list;
        }

        private string GetFeatureConfigValue(RuntimeProfileFeature feature)
        {
            TextBox box;
            if (_profileValueBoxes.TryGetValue(feature, out box) && box != null && !box.IsDisposed)
                return box.Text ?? string.Empty;
            return string.Empty;
        }

        private List<string> CaptureFeatureConfigLines()
        {
            var lines = new List<string>();
            lines.Add("# Forza Horizon 6 Luna feature configuration");
            lines.Add("# FeatureId|Enabled|Base64Value");
            foreach (var feature in GetConfigurableFeatures())
            {
                StatusDotToggle toggle;
                if (!_runtimeFeatureToggles.TryGetValue(feature, out toggle) || toggle == null || toggle.IsDisposed)
                    continue;
                var value = GetFeatureConfigValue(feature);
                var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
                lines.Add(((int)feature).ToString(CultureInfo.InvariantCulture) + "|" + (toggle.Checked ? "1" : "0") + "|" + encoded);
            }
            return lines;
        }

        private string BuildFeatureConfigSignature()
        {
            var sb = new StringBuilder();
            foreach (var feature in GetConfigurableFeatures())
            {
                StatusDotToggle toggle;
                if (!_runtimeFeatureToggles.TryGetValue(feature, out toggle) || toggle == null || toggle.IsDisposed)
                    continue;
                sb.Append((int)feature).Append(':').Append(toggle.Checked ? '1' : '0').Append(':').Append(GetFeatureConfigValue(feature)).Append('\n');
            }
            return sb.ToString();
        }

        private void SaveFeatureConfig()
        {
            try
            {
                var lines = CaptureFeatureConfigLines();
                using (var dialog = new SaveFileDialog())
                {
                    dialog.Title = "Save feature configuration";
                    dialog.InitialDirectory = FeatureConfigDir;
                    dialog.Filter = "Luna feature config (*.txt)|*.txt|All files (*.*)|*.*";
                    dialog.FileName = "feature_config_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
                    PrepareDialogForLanguage(dialog);
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                        return;

                    File.WriteAllLines(dialog.FileName, lines, Encoding.UTF8);
                    Log("Feature configuration saved: " + dialog.FileName + ".");
                    SetFeaturesStatus("Configuration saved", AccentGreen);
                }
            }
            catch (Exception ex)
            {
                Log("Feature configuration save failed: " + ex.Message);
                ShowInfo(ex.Message);
            }
        }

        private void LoadFeatureConfig()
        {
            try
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Title = "Load feature configuration";
                    dialog.InitialDirectory = FeatureConfigDir;
                    dialog.Filter = "Luna feature config (*.txt)|*.txt|All files (*.*)|*.*";
                    PrepareDialogForLanguage(dialog);
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                        return;

                    ApplyFeatureConfigFile(dialog.FileName);
                }
            }
            catch (Exception ex)
            {
                Log("Feature configuration load failed: " + ex.Message);
                ShowInfo(ex.Message);
            }
        }

        private Dictionary<RuntimeProfileFeature, KeyValuePair<bool, string>> ParseFeatureConfigFile(string fileName)
        {
            var result = new Dictionary<RuntimeProfileFeature, KeyValuePair<bool, string>>();
            foreach (var raw in File.ReadAllLines(fileName, Encoding.UTF8))
            {
                var line = (raw ?? string.Empty).Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                    continue;

                var parts = line.Split(new[] { '|' }, 3);
                if (parts.Length < 2)
                    continue;

                int id;
                if (!int.TryParse(parts[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out id))
                    continue;
                if (!Enum.IsDefined(typeof(RuntimeProfileFeature), id))
                    continue;

                var feature = (RuntimeProfileFeature)id;
                var enabledText = parts[1].Trim();
                var enabled = enabledText == "1" || enabledText.Equals("true", StringComparison.OrdinalIgnoreCase) || enabledText.Equals("on", StringComparison.OrdinalIgnoreCase);
                var value = string.Empty;
                if (parts.Length >= 3)
                {
                    try { value = Encoding.UTF8.GetString(Convert.FromBase64String(parts[2].Trim())); }
                    catch { value = string.Empty; }
                }
                result[feature] = new KeyValuePair<bool, string>(enabled, value);
            }
            return result;
        }

        private void ApplyFeatureConfigFile(string fileName)
        {
            var parsed = ParseFeatureConfigFile(fileName);
            if (parsed.Count == 0)
            {
                Log("Feature configuration file had no recognizable entries.");
                ShowInfo("That file did not contain any Luna feature configuration entries.");
                return;
            }

            _loadingFeatureConfig = true;
            try
            {
                foreach (var kv in parsed)
                {
                    TextBox box;
                    if (_profileValueBoxes.TryGetValue(kv.Key, out box) && box != null && !box.IsDisposed)
                        box.Text = kv.Value.Value ?? string.Empty;
                }

                foreach (var kv in parsed)
                {
                    StatusDotToggle toggle;
                    if (_runtimeFeatureToggles.TryGetValue(kv.Key, out toggle) && toggle != null && !toggle.IsDisposed)
                        toggle.Checked = kv.Value.Key;
                }
            }
            finally
            {
                _loadingFeatureConfig = false;
            }

            var enabledFeatures = parsed.Where(kv => kv.Value.Key).Select(kv => kv.Key).ToList();
            if (_database != null && _database.IsAlive && enabledFeatures.Count > 0)
            {
                RunWorker("Load Configuration", delegate
                {
                    foreach (var feature in enabledFeatures)
                    {
                        try { ApplyFeatureFromConfig(feature, true); }
                        catch (Exception ex) { Log("Load Configuration: " + feature + " could not apply: " + ex.Message); }
                    }
                });
                Log("Feature configuration loaded and applied (" + enabledFeatures.Count + " feature(s) on).");
            }
            else
            {
                Log("Feature configuration loaded (" + parsed.Count + " feature(s)). Attach Luna to apply the enabled ones.");
            }
            SetFeaturesStatus("Configuration loaded", AccentGreen);
        }

        private void ApplyFeatureFromConfig(RuntimeProfileFeature feature, bool enabled)
        {
            switch (feature)
            {
                case RuntimeProfileFeature.Jump: ApplyJumpRuntimeHook(enabled); return;
                case RuntimeProfileFeature.Boost: ApplyBoostRuntimeHook(enabled); return;
                case RuntimeProfileFeature.NoClip: ApplyNoClipRuntimeHook(enabled); return;
                case RuntimeProfileFeature.PhaseDash: ApplyPhaseDashRuntimeHook(enabled); return;
                case RuntimeProfileFeature.Rewind: ApplyRewindRuntimeHook(enabled); return;
                case RuntimeProfileFeature.GrapplingHook: ApplyGrapplingHookRuntimeHook(enabled); return;
                case RuntimeProfileFeature.SuperStrength: ApplySuperStrengthRuntimeHook(enabled); return;
                case RuntimeProfileFeature.DriftMode: ApplyDriftModeRuntimeHook(enabled); return;
                case RuntimeProfileFeature.FovSlider: ApplyFovRuntimeHook(enabled); return;
                case RuntimeProfileFeature.Acceleration:
                    ApplyProfileRuntimeHook(feature, "Acceleration", FormatFloat(GetAccelerationRequestedMultiplier()), enabled);
                    return;
                case RuntimeProfileFeature.AdaptiveBrake:
                    ApplyProfileRuntimeHook(feature, "Adaptive Brake", FormatFloat(_adaptiveBrakeMultiplier), enabled);
                    return;
                default:
                    ApplyProfileRuntimeHook(feature, feature.ToString(), GetFeatureConfigValue(feature), enabled);
                    return;
            }
        }

        private void EnsureFeaturesAutosaveTimer()
        {
            if (_featuresAutosaveTimer != null)
                return;
            _featuresAutosaveTimer = new System.Windows.Forms.Timer();
            _featuresAutosaveTimer.Interval = 3000;
            _featuresAutosaveTimer.Tick += delegate { FeaturesAutosaveTick(); };
            _featuresAutosaveTimer.Start();
        }

        private void FeaturesAutosaveTick()
        {
            if (!_featuresAutosaveEnabled)
                return;
            try
            {
                var signature = BuildFeatureConfigSignature();
                if (string.Equals(signature, _lastFeatureAutosaveSignature, StringComparison.Ordinal))
                    return;
                File.WriteAllLines(FeatureAutosavePath, CaptureFeatureConfigLines(), Encoding.UTF8);
                _lastFeatureAutosaveSignature = signature;
            }
            catch
            {
            }
        }

        private void SetFeaturesAutosaveEnabled(bool enabled)
        {
            _featuresAutosaveEnabled = enabled;
            if (_featuresAutosaveToggle != null && !_featuresAutosaveToggle.IsDisposed && _featuresAutosaveToggle.Checked != enabled)
                _featuresAutosaveToggle.Checked = enabled;
            SaveAppSettings();
            if (enabled)
            {
                _lastFeatureAutosaveSignature = string.Empty;
                EnsureFeaturesAutosaveTimer();
                Log("Features Autosave ON. Luna keeps the autosave config up to date as you change features. Loading is still manual.");
            }
            else
            {
                Log("Features Autosave OFF.");
            }
        }

        private void AutoLoadCurrentRuntimeValuesOnAttach()
        {
            if (_featuresAutoLoadDone)
                return;
            if (_database == null || !_database.IsAlive)
                return;
            _featuresAutoLoadDone = true;

            if (!_loadCurrentOnAttachEnabled)
            {
                _pendingRuntimeCaptures.Clear();
                if (_database != null && _database.IsAlive)
                    SetConnectionState(true);
                Log("Load Current after attach is OFF. Startup skipped live value capture.");
                return;
            }

            StartLoadCurrentTimer();
            var loadOk = false;
            try
            {
                LoadCurrentRuntimeValues();
                loadOk = true;
            }
            catch (Exception ex)
            {
                Log("Load Current error: " + ex.Message);
            }
            finally
            {
                StopLoadCurrentTimer(loadOk);
            }

            if (_database != null && _database.IsAlive)
                SetConnectionState(true);
        }
    }
}
