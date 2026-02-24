using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using NEXUS.Services;

namespace NEXUS.ViewModels;

/// <summary>
/// Consumes HardwareService and exposes formatted CPU properties for UI binding.
/// </summary>
#pragma warning disable MVVMTK0045
public partial class CpuViewModel : ObservableObject, IDisposable
{
    private readonly HardwareService _hw;
    private readonly DispatcherQueue _dispatcher;

    [ObservableProperty]
    private string _cpuLoad = "0 %";

    [ObservableProperty]
    private string _cpuTemp = "0 °C";

    [ObservableProperty]
    private string _cpuClock = "0 MHz";

    public CpuViewModel(HardwareService hardwareService)
    {
        _hw = hardwareService;
        _dispatcher = DispatcherQueue.GetForCurrentThread();
        _hw.SensorsUpdated += OnSensorsUpdated;
    }

    private void OnSensorsUpdated(object? sender, EventArgs e)
    {
        _dispatcher.TryEnqueue(() =>
        {
            CpuLoad = $"{_hw.CpuLoad:F1} %";
            CpuTemp = $"{_hw.CpuTemperature:F0} °C";
            CpuClock = $"{_hw.CpuClock:F0} MHz";
        });
    }

    public void Dispose()
    {
        _hw.SensorsUpdated -= OnSensorsUpdated;
    }
}
#pragma warning restore MVVMTK0045
