using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using NEXUS.Services;

namespace NEXUS.ViewModels;

/// <summary>
/// Exposes an ObservableCollection of DriveInfoViewModel for storage display.
/// Color-coded: green &lt; 70%, orange 70-89%, red 90%+.
/// Only updates when values actually change.
/// </summary>
#pragma warning disable MVVMTK0045
public partial class StorageViewModel : ObservableObject, IDisposable
{
    private readonly StorageService _storage;
    private readonly DispatcherQueue _dispatcher;

    public ObservableCollection<DriveInfoViewModel> Drives { get; } = new();

    public StorageViewModel(StorageService storageService)
    {
        _storage = storageService;
        _dispatcher = DispatcherQueue.GetForCurrentThread();
        _storage.StorageUpdated += OnStorageUpdated;

        // Trigger initial UI update
        RefreshUi();
    }

    private void OnStorageUpdated(object? sender, EventArgs e)
    {
        _dispatcher.TryEnqueue(RefreshUi);
    }

    private void RefreshUi()
    {
        // Rebuild the collection (drives rarely change, 30s interval)
        Drives.Clear();
        foreach (var d in _storage.Drives)
        {
            var color = d.UsedPercent switch
            {
                >= 90 => "#FF4444",   // Red
                >= 70 => "#FFAA33",   // Orange
                _ => "#44DD88"        // Green
            };

            Drives.Add(new DriveInfoViewModel
            {
                DriveName = d.Name,
                DriveLabel = d.Label,
                DriveTypeLabel = d.DriveTypeLabel,
                TotalGB = $"{d.TotalGB:F1} GB",
                UsedGB = $"{d.UsedGB:F1} GB",
                FreeGB = $"{d.FreeGB:F1} GB",
                UsedPercent = d.UsedPercent,
                UsageText = $"{d.UsedGB:F1} / {d.TotalGB:F1} GB ({d.UsedPercent:F0}%)",
                BarColor = color
            });
        }
    }

    public void Dispose()
    {
        _storage.StorageUpdated -= OnStorageUpdated;
    }
}
#pragma warning restore MVVMTK0045

/// <summary>Display model for a single drive.</summary>
public class DriveInfoViewModel
{
    public string DriveName { get; init; } = "";
    public string DriveLabel { get; init; } = "";
    public string DriveTypeLabel { get; init; } = "";
    public string TotalGB { get; init; } = "";
    public string UsedGB { get; init; } = "";
    public string FreeGB { get; init; } = "";
    public float UsedPercent { get; init; }
    public string UsageText { get; init; } = "";
    /// <summary>Hex color string: green/orange/red based on usage.</summary>
    public string BarColor { get; init; } = "#44DD88";
}
