using System.Diagnostics;
using System.Timers;
using OpenRGB.NET;

namespace NEXUS.Services;

/// <summary>
/// Connects to OpenRGB SDK server on localhost:6742.
/// ALL color changes use client.UpdateLeds(deviceIndex, Color[]) exclusively.
/// Refreshes device list every 30 seconds. Handles connection failure gracefully.
///
/// NOTE: RAM RGB detection requires OpenRGB SMBus/I2C access to be
/// manually enabled by the user (Settings → SMBus Access → Enable,
/// then rescan devices).
/// </summary>
public sealed class RgbService : IDisposable
{
    private const string Host = "127.0.0.1";
    private const int Port = 6742;

    private static readonly string[] ExcludedKeywords = ["B550", "motherboard"];

    private OpenRgbClient? _client;
    private readonly System.Timers.Timer _refreshTimer;
    private readonly object _lock = new();
    private bool _disposed;

    public List<RgbDeviceInfo> Devices { get; private set; } = new();
    public bool IsConnected { get; private set; }
    public string? ConnectionError { get; private set; }
    public bool IsEnabled { get; set; } = true;

    public event EventHandler? DevicesUpdated;

    public RgbService()
    {
        TryConnect();
        RefreshDevices();

        _refreshTimer = new System.Timers.Timer(30_000);
        _refreshTimer.Elapsed += (_, _) => RefreshDevices();
        _refreshTimer.AutoReset = true;
        _refreshTimer.Start();
    }

    private void TryConnect()
    {
        try
        {
            _client = new OpenRgbClient(name: "NEXUS Dashboard", ip: Host, port: Port);
            _client.Connect();
            IsConnected = true;
            ConnectionError = null;
            Debug.WriteLine("[RGB] Connected to OpenRGB server");
        }
        catch (Exception ex)
        {
            IsConnected = false;
            ConnectionError = ex.Message;
            _client = null;
            Debug.WriteLine($"[RGB] Connection failed: {ex.Message}");
        }
    }

    private void RefreshDevices()
    {
        if (!IsConnected || _client is null)
        {
            TryConnect();
            if (!IsConnected) return;
        }

        lock (_lock)
        {
            try
            {
                var controllers = _client!.GetAllControllerData();
                var devices = new List<RgbDeviceInfo>();

                for (int i = 0; i < controllers.Length; i++)
                {
                    var c = controllers[i];
                    var ledCount = c.Leds.Length;
                    if (ledCount == 0) continue;

                    var excluded = false;
                    foreach (var kw in ExcludedKeywords)
                    {
                        if (c.Name.Contains(kw, StringComparison.OrdinalIgnoreCase))
                        { excluded = true; break; }
                    }
                    if (excluded) continue;

                    var isEssential = c.Name.Contains("Essential", StringComparison.OrdinalIgnoreCase);

                    devices.Add(new RgbDeviceInfo
                    {
                        Name = c.Name,
                        DeviceIndex = i,
                        LedCount = ledCount,
                        IsHardwiredGreen = isEssential
                    });
                    Debug.WriteLine($"[RGB] Device {i}: \"{c.Name}\" — {ledCount} LEDs{(isEssential ? " (hardwired green)" : "")}");
                }

                Devices = devices;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RGB] Refresh error: {ex.Message}");
                IsConnected = false;
                ConnectionError = ex.Message;
            }
        }

        DevicesUpdated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Sends a uniform color to ALL LEDs on a device using UpdateLeds.
    /// Gets device.Leds.Length, creates Color[ledCount] filled with target, calls UpdateLeds.
    /// </summary>
    public void SendColorToDevice(int deviceIndex, byte r, byte g, byte b)
    {
        if (!IsConnected || _client is null) return;

        lock (_lock)
        {
            try
            {
                var controllers = _client.GetAllControllerData();
                if (deviceIndex < 0 || deviceIndex >= controllers.Length) return;

                var ledCount = controllers[deviceIndex].Leds.Length;
                var color = new OpenRGB.NET.Color(r, g, b);
                var colors = new OpenRGB.NET.Color[ledCount];
                for (int i = 0; i < ledCount; i++)
                    colors[i] = color;

                _client.UpdateLeds(deviceIndex, colors.AsSpan());
                Debug.WriteLine($"[RGB] UpdateLeds device {deviceIndex} ({ledCount} LEDs) → ({r},{g},{b})");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RGB] UpdateLeds error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Turns OFF all LEDs on ALL devices using UpdateLeds with black (0,0,0).
    /// </summary>
    public void TurnOffAll()
    {
        if (!IsConnected || _client is null) return;

        foreach (var d in Devices)
        {
            SendColorToDevice(d.DeviceIndex, 0, 0, 0);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _refreshTimer.Stop();
        _refreshTimer.Dispose();

        lock (_lock) { _client?.Dispose(); }
    }
}

/// <summary>Represents an RGB device detected by OpenRGB.</summary>
public class RgbDeviceInfo
{
    public string Name { get; init; } = "";
    public int DeviceIndex { get; init; }
    public int LedCount { get; init; }
    /// <summary>True for Razer Essential — hardwired green LED, only supports on/off.</summary>
    public bool IsHardwiredGreen { get; init; }
}
