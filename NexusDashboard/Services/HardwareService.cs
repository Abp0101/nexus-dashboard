using System.Diagnostics;
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
    private bool _sensorsLogged;

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
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true  // Needed for AMD CPU temp fallback
        };

        // Enable WinRing0 kernel driver — required for AMD CPU sensor access
        try
        {
            // Property may not exist in all versions of the library
            var prop = typeof(Computer).GetProperty("UseWinRing0AndTarget");
            prop?.SetValue(_computer, true);
            Debug.WriteLine("[HardwareService] UseWinRing0AndTarget = true");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HardwareService] Could not set UseWinRing0AndTarget: {ex.Message}");
        }

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

            // Also update sub-hardware (e.g. motherboard chips)
            foreach (var sub in hardware.SubHardware)
            {
                sub.Update();
            }

            // Log ALL sensors on every hardware + sub-hardware on first poll
            if (!_sensorsLogged)
            {
                LogHardware(hardware, indent: 0);
            }

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

                case HardwareType.Motherboard:
                    // Fallback: AMD CPU temp sometimes lives under motherboard
                    ReadMotherboardCpuTemp(hardware);
                    break;
            }
        }

        _sensorsLogged = true;
        SensorsUpdated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Recursively logs every sensor on a hardware item and all its sub-hardware.
    /// </summary>
    private static void LogHardware(IHardware hw, int indent)
    {
        var prefix = new string(' ', indent * 2);
        Debug.WriteLine($"{prefix}══ {hw.HardwareType}: \"{hw.Name}\" ══");

        foreach (var sensor in hw.Sensors)
        {
            Debug.WriteLine($"{prefix}  [{sensor.SensorType,-14}] \"{sensor.Name}\" = {sensor.Value}");
        }

        foreach (var sub in hw.SubHardware)
        {
            LogHardware(sub, indent + 1);
        }
    }

    // ─── CPU ─────────────────────────────────────────────────────────

    private void ReadCpu(IHardware hw)
    {
        // Load: prefer "CPU Total", fallback to first Load sensor
        // ✅ Works on AMD Ryzen 5800X
        CpuLoad = FindSensorValue(hw, SensorType.Load,
            "CPU Total") ?? CpuLoad;

        // Temperature: AMD Ryzen 5800X cannot report CPU temp via
        // LibreHardwareMonitor without the WinRing0 kernel driver,
        // which has signing/compatibility issues on modern Windows.
        // CpuTemperature will remain 0 when unavailable — the UI
        // shows "Temp unavailable on AMD" instead.
        CpuTemperature = FindSensorValue(hw, SensorType.Temperature,
            "Core (Tdie)",
            "CPU Die (average)",
            "CPU Package",
            "Core Average") ?? CpuTemperature;

        // Clock: prefer "Core #0", fallback to first Clock sensor
        // ✅ Works on AMD Ryzen 5800X
        CpuClock = FindSensorValue(hw, SensorType.Clock,
            "Core #0",
            "Core #1") ?? CpuClock;
    }

    // ─── Motherboard CPU temp fallback ───────────────────────────────

    /// <summary>
    /// AMD CPU temperatures are sometimes reported under motherboard
    /// sub-hardware (e.g. the Super I/O chip). Only used as a fallback
    /// when CPU hardware itself reports no temperature.
    /// </summary>
    private void ReadMotherboardCpuTemp(IHardware hw)
    {
        // Only use as fallback if CPU didn't already give us a temp
        if (CpuTemperature > 0) return;

        // Search the motherboard itself and all sub-hardware
        var temp = FindSensorValueRecursive(hw, SensorType.Temperature,
            "CPU",
            "CPU Socket",
            "CPU (Tctl)",
            "CPU (Tctl/Tdie)");

        if (temp.HasValue)
        {
            CpuTemperature = temp.Value;
            if (!_sensorsLogged)
                Debug.WriteLine($"  [Fallback] CPU temp from motherboard: {temp.Value} °C");
        }
    }

    // ─── GPU ─────────────────────────────────────────────────────────

    private void ReadGpu(IHardware hw)
    {
        GpuLoad = FindSensorValue(hw, SensorType.Load,
            "GPU Core",
            "D3D 3D") ?? GpuLoad;

        GpuTemperature = FindSensorValue(hw, SensorType.Temperature,
            "GPU Core",
            "GPU Hot Spot") ?? GpuTemperature;

        // VRAM: try SmallData first (MB), then Data (GB)
        var vramUsedMB = FindSensorValue(hw, SensorType.SmallData, "GPU Memory Used");
        if (vramUsedMB.HasValue)
        {
            GpuVramUsed = vramUsedMB.Value / 1024f;
        }
        else
        {
            var vramUsedGB = FindSensorValue(hw, SensorType.Data, "GPU Memory Used");
            if (vramUsedGB.HasValue) GpuVramUsed = vramUsedGB.Value;
        }

        var vramTotalMB = FindSensorValue(hw, SensorType.SmallData, "GPU Memory Total");
        if (vramTotalMB.HasValue)
        {
            GpuVramTotal = vramTotalMB.Value / 1024f;
        }
        else
        {
            var vramTotalGB = FindSensorValue(hw, SensorType.Data, "GPU Memory Total");
            if (vramTotalGB.HasValue) GpuVramTotal = vramTotalGB.Value;
        }
    }

    // ─── RAM ─────────────────────────────────────────────────────────

    private void ReadRam(IHardware hw)
    {
        RamUsed = FindSensorValue(hw, SensorType.Data, "Memory Used") ?? RamUsed;
        RamAvailable = FindSensorValue(hw, SensorType.Data, "Memory Available") ?? RamAvailable;
        RamTotal = RamUsed + RamAvailable;
    }

    // ─── Helpers ─────────────────────────────────────────────────────

    /// <summary>
    /// Searches sensors on a single hardware item for a match by type + preferred name.
    /// Falls back to the first sensor of that type if no preferred name matches.
    /// </summary>
    private static float? FindSensorValue(IHardware hw, SensorType type, params string[] preferredNames)
    {
        ISensor? fallback = null;

        foreach (var sensor in hw.Sensors)
        {
            if (sensor.SensorType != type || sensor.Value is null)
                continue;

            foreach (var name in preferredNames)
            {
                if (sensor.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return sensor.Value.Value;
            }

            fallback ??= sensor;
        }

        return fallback?.Value;
    }

    /// <summary>
    /// Same as FindSensorValue but also searches all sub-hardware recursively.
    /// </summary>
    private static float? FindSensorValueRecursive(IHardware hw, SensorType type, params string[] preferredNames)
    {
        // Check this hardware first
        var result = FindSensorValue(hw, type, preferredNames);
        if (result.HasValue) return result;

        // Then recurse into sub-hardware
        foreach (var sub in hw.SubHardware)
        {
            result = FindSensorValueRecursive(sub, type, preferredNames);
            if (result.HasValue) return result;
        }

        return null;
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
