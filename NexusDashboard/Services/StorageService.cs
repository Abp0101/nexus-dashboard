using System.Diagnostics;
using System.Timers;

namespace NEXUS.Services;

/// <summary>
/// Enumerates all ready drives using System.IO.DriveInfo every 30 seconds.
/// Exposes drive name, label, type, total/free/used space, and usage percentage.
/// Runs entirely on a background thread.
/// </summary>
public sealed class StorageService : IDisposable
{
    private readonly System.Timers.Timer _pollTimer;
    private bool _disposed;

    /// <summary>Current snapshot of drive info.</summary>
    public List<StorageDriveInfo> Drives { get; private set; } = new();

    /// <summary>Fires after each storage poll.</summary>
    public event EventHandler? StorageUpdated;

    public StorageService()
    {
        Poll();

        _pollTimer = new System.Timers.Timer(30_000);
        _pollTimer.Elapsed += (_, _) => Poll();
        _pollTimer.AutoReset = true;
        _pollTimer.Start();
    }

    private void Poll()
    {
        try
        {
            var drives = new List<StorageDriveInfo>();

            foreach (var d in DriveInfo.GetDrives())
            {
                if (!d.IsReady) continue;

                var totalGB = d.TotalSize / (1024.0 * 1024.0 * 1024.0);
                var freeGB = d.TotalFreeSpace / (1024.0 * 1024.0 * 1024.0);
                var usedGB = totalGB - freeGB;
                var usedPct = totalGB > 0 ? (float)((usedGB / totalGB) * 100.0) : 0f;

                var typeLabel = d.DriveType switch
                {
                    DriveType.Fixed => "SSD/HDD",
                    DriveType.Network => "Network",
                    DriveType.Removable => "Removable",
                    DriveType.CDRom => "CD-ROM",
                    _ => d.DriveType.ToString()
                };

                drives.Add(new StorageDriveInfo
                {
                    Name = d.Name.TrimEnd('\\'),
                    Label = string.IsNullOrWhiteSpace(d.VolumeLabel) ? "Local Disk" : d.VolumeLabel,
                    DriveTypeLabel = typeLabel,
                    TotalGB = totalGB,
                    FreeGB = freeGB,
                    UsedGB = usedGB,
                    UsedPercent = usedPct
                });

                Debug.WriteLine($"[Storage] {d.Name} \"{d.VolumeLabel}\" {usedGB:F1}/{totalGB:F1} GB ({usedPct:F0}%)");
            }

            Drives = drives;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Storage] Poll error: {ex.Message}");
        }

        StorageUpdated?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _pollTimer.Stop();
        _pollTimer.Dispose();
    }
}

/// <summary>Represents a single drive.</summary>
public class StorageDriveInfo
{
    public string Name { get; init; } = "";
    public string Label { get; init; } = "";
    public string DriveTypeLabel { get; init; } = "";
    public double TotalGB { get; init; }
    public double FreeGB { get; init; }
    public double UsedGB { get; init; }
    /// <summary>0–100</summary>
    public float UsedPercent { get; init; }
}
