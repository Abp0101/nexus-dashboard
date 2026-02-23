using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using NEXUS.Services;

namespace NEXUS.ViewModels;

/// <summary>
/// Consumes HardwareService and exposes formatted RAM properties for UI binding.
/// </summary>
#pragma warning disable MVVMTK0045
public partial class RamViewModel : ObservableObject, IDisposable
{
    private readonly HardwareService _hw;
    private readonly DispatcherQueue _dispatcher;

    [ObservableProperty]
    private string _ramUsed = "0.0 GB";

    [ObservableProperty]
    private string _ramTotal = "0.0 GB";

    [ObservableProperty]
    private string _ramAvailable = "0.0 GB";

    [ObservableProperty]
    private double _ramUsagePercent = 0;

    [ObservableProperty]
    private string _ramUsagePercentText = "0 %";

    public RamViewModel(HardwareService hardwareService)
    {
        _hw = hardwareService;
        _dispatcher = DispatcherQueue.GetForCurrentThread();
        _hw.SensorsUpdated += OnSensorsUpdated;
    }

    private void OnSensorsUpdated(object? sender, EventArgs e)
    {
        _dispatcher.TryEnqueue(() =>
        {
            RamUsed = $"{_hw.RamUsed:F1} GB";
            RamTotal = $"{_hw.RamTotal:F1} GB";
            RamAvailable = $"{_hw.RamAvailable:F1} GB";

            var pct = _hw.RamTotal > 0
                ? (_hw.RamUsed / _hw.RamTotal) * 100.0
                : 0;

            RamUsagePercent = pct;
            RamUsagePercentText = $"{pct:F0} %";
        });
    }

    public void Dispose()
    {
        _hw.SensorsUpdated -= OnSensorsUpdated;
    }
}
#pragma warning restore MVVMTK0045
