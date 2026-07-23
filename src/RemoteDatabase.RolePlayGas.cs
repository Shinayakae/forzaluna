using System;
using System.Globalization;

namespace ForzaHorizon6AutoshowUnlocker
{
    public sealed partial class RemoteDatabase
    {
        private readonly object _rolePlayGasLock = new object();
        private bool _rolePlayGasEnabled;
        private bool _rolePlayGasDrainByDistance = true;
        private float _rolePlayGasBudget = 100000F;
        private float _rolePlayGasRemaining = 100000F;
        private bool _rolePlayGasHasLastPosition;
        private float _rolePlayGasLastX;
        private float _rolePlayGasLastY;
        private float _rolePlayGasLastZ;
        private long _rolePlayGasLastTickUtcTicks;
        private long _rolePlayGasLastStatusTicks;
        private long _rolePlayGasLastLogTicks;
        private Action<float, string, string> _rolePlayGasStatusCallback;

        public void SetRolePlayGasStatusCallback(Action<float, string, string> callback)
        {
            lock (_rolePlayGasLock)
                _rolePlayGasStatusCallback = callback;
        }

        public void SetRolePlayGasRuntimeHook(bool enabled, bool drainByDistance, float amount)
        {
            if (!IsAlive)
                throw new InvalidOperationException("Not attached.");

            if (!enabled)
            {
                StopRolePlayGasRuntime();
                _log("Role Play Gas runtime OFF.");
                return;
            }

            if (!IsFiniteFloat(amount) || amount <= 0F)
                throw new InvalidOperationException("Gas full-tank amount is not valid.");

            EnsureAssistVehicleDetour();
            lock (_rolePlayGasLock)
            {
                _rolePlayGasDrainByDistance = drainByDistance;
                _rolePlayGasBudget = drainByDistance ? amount * 1000F : amount * 60F;
                _rolePlayGasBudget = Math.Max(1F, _rolePlayGasBudget);
                _rolePlayGasRemaining = _rolePlayGasBudget;
                _rolePlayGasHasLastPosition = false;
                _rolePlayGasLastTickUtcTicks = 0;
                _rolePlayGasLastStatusTicks = 0;
                _rolePlayGasEnabled = true;
            }
            EnsureTeleportGuardTimer();
            PublishRolePlayGasStatus("Full tank");
            _log("Role Play Gas runtime ON: " + (drainByDistance ? "distance" : "minutes") + " mode, amount " + amount.ToString("0.###", CultureInfo.InvariantCulture) + ".");
        }

        public void ResetRolePlayGasTank()
        {
            lock (_rolePlayGasLock)
            {
                _rolePlayGasRemaining = Math.Max(1F, _rolePlayGasBudget);
                _rolePlayGasHasLastPosition = false;
                _rolePlayGasLastTickUtcTicks = 0;
                _rolePlayGasLastStatusTicks = 0;
            }
            PublishRolePlayGasStatus("Full tank");
        }

        private void StopRolePlayGasRuntime()
        {
            lock (_rolePlayGasLock)
            {
                _rolePlayGasEnabled = false;
                _rolePlayGasHasLastPosition = false;
                _rolePlayGasLastTickUtcTicks = 0;
                _rolePlayGasLastStatusTicks = 0;
            }
            StopTeleportGuardTimerIfIdle();
        }

        private void ApplyRolePlayGasTick(long nowTicks)
        {
            bool enabled;
            bool distanceMode;
            lock (_rolePlayGasLock)
            {
                enabled = _rolePlayGasEnabled;
                distanceMode = _rolePlayGasDrainByDistance;
            }
            if (!enabled)
                return;

            var vp = ResolveAutoRaceDriveVehiclePointer();
            if (!IsAssistVehicleWriteSafe(vp, nowTicks))
            {
                lock (_rolePlayGasLock)
                    _rolePlayGasHasLastPosition = false;
                return;
            }

            if (!IsReadableMemoryRange(vp + (ulong)VehiclePositionOffset, 12))
                return;

            float x, y, z;
            if (!TryReadWorldVec3(vp + (ulong)VehiclePositionOffset, out x, out y, out z))
                return;

            float percent;
            string detail = null;
            lock (_rolePlayGasLock)
            {
                if (!_rolePlayGasEnabled)
                    return;

                if (distanceMode)
                {
                    if (_rolePlayGasHasLastPosition)
                    {
                        var dx = x - _rolePlayGasLastX;
                        var dz = z - _rolePlayGasLastZ;
                        var distance = (float)Math.Sqrt((dx * dx) + (dz * dz));
                        if (IsFiniteFloat(distance) && distance >= 0F && distance <= 120F)
                            _rolePlayGasRemaining = Math.Max(0F, _rolePlayGasRemaining - distance);
                    }
                    _rolePlayGasLastX = x;
                    _rolePlayGasLastY = y;
                    _rolePlayGasLastZ = z;
                    _rolePlayGasHasLastPosition = true;
                    detail = (_rolePlayGasRemaining / 1000F).ToString("0.0", CultureInfo.InvariantCulture) + " km left";
                }
                else
                {
                    if (_rolePlayGasLastTickUtcTicks != 0)
                    {
                        var seconds = (float)((nowTicks - _rolePlayGasLastTickUtcTicks) / (double)TimeSpan.TicksPerSecond);
                        if (IsFiniteFloat(seconds) && seconds > 0F && seconds < 1F)
                            _rolePlayGasRemaining = Math.Max(0F, _rolePlayGasRemaining - seconds);
                    }
                    _rolePlayGasLastTickUtcTicks = nowTicks;
                    detail = (_rolePlayGasRemaining / 60F).ToString("0.0", CultureInfo.InvariantCulture) + " min left";
                }

                percent = _rolePlayGasBudget <= 0F ? 0F : Math.Max(0F, Math.Min(100F, (_rolePlayGasRemaining / _rolePlayGasBudget) * 100F));
            }

            if (percent <= 0F)
                HoldRolePlayGasVehicleStopped(vp, nowTicks);

            if (nowTicks - _rolePlayGasLastStatusTicks > TimeSpan.TicksPerMillisecond * 250)
            {
                _rolePlayGasLastStatusTicks = nowTicks;
                PublishRolePlayGasStatus(detail);
            }
        }

        private void HoldRolePlayGasVehicleStopped(ulong vehiclePointer, long nowTicks)
        {
            if (vehiclePointer == 0)
                return;
            try
            {
                if (!IsWritableMemoryRange(vehiclePointer + (ulong)VehicleVelocityOffset, 16))
                    return;

                var zero = new byte[16];
                WriteBytes(vehiclePointer + (ulong)VehicleVelocityOffset, zero);
                if (IsWritableMemoryRange(vehiclePointer + (ulong)VehicleAngularVelocityOffset, 16))
                    WriteBytes(vehiclePointer + (ulong)VehicleAngularVelocityOffset, zero);

                if (nowTicks - _rolePlayGasLastLogTicks > TimeSpan.TicksPerSecond * 5)
                {
                    _rolePlayGasLastLogTicks = nowTicks;
                    _log("Role Play Gas empty - vehicle held stopped.");
                }
            }
            catch (Exception ex)
            {
                LogRuntimeGuardWaiting("Role Play Gas", ex);
            }
        }

        private void PublishRolePlayGasStatus(string detail)
        {
            Action<float, string, string> callback;
            float percent;
            bool distanceMode;
            lock (_rolePlayGasLock)
            {
                callback = _rolePlayGasStatusCallback;
                percent = _rolePlayGasBudget <= 0F ? 0F : Math.Max(0F, Math.Min(100F, (_rolePlayGasRemaining / _rolePlayGasBudget) * 100F));
                distanceMode = _rolePlayGasDrainByDistance;
            }

            if (callback == null)
                return;

            try
            {
                callback(percent, distanceMode ? "Distance" : "Minutes", detail ?? string.Empty);
            }
            catch
            {
            }
        }
    }
}
