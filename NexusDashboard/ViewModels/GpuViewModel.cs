using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using NEXUS.Services;

namespace NEXUS.ViewModels;

/// <summary>
/// Consumes HardwareService and exposes formatted GPU properties for UI binding.
/// </summary>
#pragma warning disable MVVMTK0045
public partial class GpuViewModel : ObservableObject, IDisposable
{
    private readonly HardwareService _hw;
    private readonly DispatcherQueue _dispatcher;

    [ObservableProperty]
    private string _gpuLoad = "0 %";

    [ObservableProperty]
    private string _gpuTemp = "0 °C";

    [ObservableProperty]
    private string _gpuVramUsed = "0.0 GB";

    [ObservableProperty]
    private string _gpuVramTotal = "0.0 GB";

    public GpuViewModel(HardwareService hardwareService)
    {
        _hw = hardwareService;
        _dispatcher = DispatcherQueue.GetForCurrentThread();
        _hw.SensorsUpdated += OnSensorsUpdated;
    }

    private void OnSensorsUpdated(object? sender, EventArgs e)
    {
        _dispatcher.TryEnqueue(() =>
        {
            GpuLoad = $"{_hw.GpuLoad:F1} %";
            GpuTemp = $"{_hw.GpuTemperature:F0} °C";
            GpuVramUsed = $"{_hw.GpuVramUsed:F1} GB";
            GpuVramTotal = $"{_hw.GpuVramTotal:F1} GB";
        });
    }

    public void Dispose()
    {
        _hw.SensorsUpdated -= OnSensorsUpdated;
    }
}
#pragma warning restore MVVMTK0045
