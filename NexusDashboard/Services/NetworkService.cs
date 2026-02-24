using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Timers;

namespace NEXUS.Services;

/// <summary>
/// Calculates network upload/download speeds by sampling total bytes every 2 seconds.
/// Pings 8.8.8.8 for latency. Runs entirely on background thread.
/// </summary>
public sealed class NetworkService : IDisposable
{
    private readonly System.Timers.Timer _pollTimer;
    private readonly object _lock = new();
    private bool _disposed;

    private long _prevBytesSent;
    private long _prevBytesReceived;
    private DateTime _prevSampleTime;
    private string? _activeInterfaceId;

    /// <summary>Download speed in MB/s.</summary>
    public double DownloadSpeedMBps { get; private set; }

    /// <summary>Upload speed in MB/s.</summary>
    public double UploadSpeedMBps { get; private set; }

    /// <summary>Latency in milliseconds, or -1 if unreachable.</summary>
    public long LatencyMs { get; private set; } = -1;

    /// <summary>True if an active network interface was found.</summary>
    public bool HasConnection { get; private set; }

    /// <summary>Fires every 2 seconds after a network poll.</summary>
    public event EventHandler? NetworkUpdated;

    public NetworkService()
    {
        // Take initial sample
        InitSample();

        // Poll every 2 seconds on a background thread
        _pollTimer = new System.Timers.Timer(2000);
        _pollTimer.Elapsed += (_, _) => Poll();
        _pollTimer.AutoReset = true;
        _pollTimer.Start();
    }

    private void InitSample()
    {
        var iface = FindActiveInterface();
        if (iface is null)
        {
            HasConnection = false;
            _prevBytesSent = 0;
            _prevBytesReceived = 0;
            _prevSampleTime = DateTime.UtcNow;
            return;
        }

        HasConnection = true;
        _activeInterfaceId = iface.Id;
        var stats = iface.GetIPv4Statistics();
        _prevBytesSent = stats.BytesSent;
        _prevBytesReceived = stats.BytesReceived;
        _prevSampleTime = DateTime.UtcNow;
    }

    private void Poll()
    {
        lock (_lock)
        {
            try
            {
                var iface = FindActiveInterface();
                if (iface is null)
                {
                    HasConnection = false;
                    DownloadSpeedMBps = 0;
                    UploadSpeedMBps = 0;
                    LatencyMs = -1;
                    NetworkUpdated?.Invoke(this, EventArgs.Empty);
                    return;
                }

                HasConnection = true;
                _activeInterfaceId = iface.Id;

                var stats = iface.GetIPv4Statistics();
                var now = DateTime.UtcNow;
                var elapsed = (now - _prevSampleTime).TotalSeconds;

                if (elapsed > 0)
                {
                    var deltaDown = stats.BytesReceived - _prevBytesReceived;
                    var deltaUp = stats.BytesSent - _prevBytesSent;

                    // Convert bytes/sec to MB/s
                    DownloadSpeedMBps = (deltaDown / elapsed) / (1024.0 * 1024.0);
                    UploadSpeedMBps = (deltaUp / elapsed) / (1024.0 * 1024.0);

                    // Clamp negative values (counter resets)
                    if (DownloadSpeedMBps < 0) DownloadSpeedMBps = 0;
                    if (UploadSpeedMBps < 0) UploadSpeedMBps = 0;
                }

                _prevBytesReceived = stats.BytesReceived;
                _prevBytesSent = stats.BytesSent;
                _prevSampleTime = now;

                // Ping 8.8.8.8 with 1 second timeout
                try
                {
                    using var ping = new Ping();
                    var reply = ping.Send("8.8.8.8", 1000);
                    LatencyMs = reply.Status == IPStatus.Success ? reply.RoundtripTime : -1;
                }
                catch
                {
                    LatencyMs = -1;
                }

                Debug.WriteLine($"[Network] ↓{DownloadSpeedMBps:F2} MB/s ↑{UploadSpeedMBps:F2} MB/s Ping={LatencyMs}ms");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Network] Poll error: {ex.Message}");
            }
        }

        NetworkUpdated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Selects the first active, non-loopback network interface.
    /// </summary>
    private static NetworkInterface? FindActiveInterface()
    {
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up) continue;
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel) continue;

            var stats = ni.GetIPv4Statistics();
            if (stats.BytesReceived > 0)
                return ni;
        }
        return null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _pollTimer.Stop();
        _pollTimer.Dispose();
    }
}
