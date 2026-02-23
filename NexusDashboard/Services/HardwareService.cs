using System.Timers;
using LibreHardwareMonitor.Hardware;

namespace NEXUS.Services;

/// <summary>
/// Monitors CPU, GPU, and RAM sensors via LibreHardwareMonitorLib.
/// Polls every 1 second on a background thread. Requires administrator privileges.
/// </summary>
public sealed class HardwareService : IDisposable
{
    private readonly Computer _computer;
    private readonly System.Timers.Timer _pollTimer;
    private bool _disposed;

    // ── CPU ──────────────────────────────────────────
    public float CpuLoad { get; private set; }
    public float CpuTemperature { get; private set; }
    public float CpuClock { get; private set; }

    // ── GPU ──────────────────────────────────────────
    public float GpuLoad { get; private set; }
    public float GpuTemperature { get; private set; }
    public float GpuVramUsed { get; private set; }
    public float GpuVramTotal { get; private set; }

    // ── RAM ──────────────────────────────────────────
    public float RamUsed { get; private set; }
    public float RamTotal { get; private set; }
    public float RamAvailable { get; private set; }

    /// <summary>
    /// Fires after every poll cycle with updated sensor values.
    /// </summary>
    public event EventHandler? SensorsUpdated;

    public HardwareService()
    {
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true
        };

        _computer.Open();

        // Initial read
        Update();

        // Poll every 1 second
        _pollTimer = new System.Timers.Timer(1000);
        _pollTimer.Elapsed += OnTimerElapsed;
        _pollTimer.AutoReset = true;
        _pollTimer.Start();
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        Update();
    }

    private void Update()
    {
        foreach (var hardware in _computer.Hardware)
        {
            hardware.Update();

            switch (hardware.HardwareType)
            {
                case HardwareType.Cpu:
                    ReadCpu(hardware);
                    break;

                case HardwareType.GpuNvidia:
                case HardwareType.GpuAmd:
                case HardwareType.GpuIntel:
                    ReadGpu(hardware);
                    break;

                case HardwareType.Memory:
                    ReadRam(hardware);
                    break;
            }
        }

        SensorsUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void ReadCpu(IHardware hw)
    {
        foreach (var sensor in hw.Sensors)
        {
            if (sensor.Value is null) continue;

            switch (sensor.SensorType)
            {
                case SensorType.Load when sensor.Name.Contains("Total"):
                    CpuLoad = sensor.Value.Value;
                    break;

                case SensorType.Temperature when sensor.Name.Contains("Package") ||
                                                  sensor.Name.Contains("Average") ||
                                                  sensor.Name.Contains("Core #1"):
                    CpuTemperature = sensor.Value.Value;
                    break;

                case SensorType.Clock when sensor.Name.Contains("Core #1"):
                    CpuClock = sensor.Value.Value;
                    break;
            }
        }
    }

    private void ReadGpu(IHardware hw)
    {
        foreach (var sensor in hw.Sensors)
        {
            if (sensor.Value is null) continue;

            switch (sensor.SensorType)
            {
                case SensorType.Load when sensor.Name.Contains("Core"):
                    GpuLoad = sensor.Value.Value;
                    break;

                case SensorType.Temperature when sensor.Name.Contains("Core") ||
                                                  sensor.Name.Contains("GPU"):
                    GpuTemperature = sensor.Value.Value;
                    break;

                case SensorType.SmallData when sensor.Name.Contains("GPU Memory Used"):
                    GpuVramUsed = sensor.Value.Value / 1024f; // MB → GB
                    break;

                case SensorType.SmallData when sensor.Name.Contains("GPU Memory Total"):
                    GpuVramTotal = sensor.Value.Value / 1024f; // MB → GB
                    break;
            }
        }
    }

    private void ReadRam(IHardware hw)
    {
        foreach (var sensor in hw.Sensors)
        {
            if (sensor.Value is null) continue;

            switch (sensor.SensorType)
            {
                case SensorType.Data when sensor.Name.Contains("Memory Used"):
                    RamUsed = sensor.Value.Value;
                    break;

                case SensorType.Data when sensor.Name.Contains("Memory Available"):
                    RamAvailable = sensor.Value.Value;
                    break;
            }
        }

        // Total = Used + Available
        RamTotal = RamUsed + RamAvailable;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _pollTimer.Stop();
        _pollTimer.Dispose();
        _computer.Close();
    }
}
