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

    [ObservableProperty]
    private string _gpuVram = "0.0 GB";

    [ObservableProperty]
    private string _gpuClock = "0 MHz";

    [ObservableProperty]
    private string _gpuTempBg = "#1410F59E";

    [ObservableProperty]
    private string _gpuTempFg = "#10F59E";

    [ObservableProperty]
    private string _gpuTempBorder = "#3310F59E";

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
            GpuVram = $"{_hw.GpuVramUsed:F1} / {_hw.GpuVramTotal:F1} GB";
            GpuClock = $"{_hw.GpuClock:F0} MHz";

            // Temperature Chip Pill colors
            if (_hw.GpuTemperature < 60) {
                GpuTempBg = "#1410F59E"; // 8% of #10F59E 
                GpuTempBorder = "#3310F59E"; // 20%
                GpuTempFg = "#10F59E";
            } else if (_hw.GpuTemperature <= 80) {
                GpuTempBg = "#14FBBF24"; // 8% of #FBBF24
                GpuTempBorder = "#33FBBF24"; // 20%
                GpuTempFg = "#FBBF24";
            } else {
                GpuTempBg = "#14FF4D6D"; // 8% of #FF4D6D
                GpuTempBorder = "#33FF4D6D"; // 20%
                GpuTempFg = "#FF4D6D";
            }
        });
    }

    public void Dispose()
    {
        _hw.SensorsUpdated -= OnSensorsUpdated;
    }
}
#pragma warning restore MVVMTK0045
