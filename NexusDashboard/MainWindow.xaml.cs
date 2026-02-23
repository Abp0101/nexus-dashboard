using Microsoft.UI.Xaml;
using NEXUS.ViewModels;
using NEXUS.Views.Widgets;

namespace NEXUS;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        Title = "NEXUS Dashboard";

        // ── Row 1: Hardware widgets ──
        var cpuVm = new CpuViewModel(App.HardwareService);
        var gpuVm = new GpuViewModel(App.HardwareService);
        var ramVm = new RamViewModel(App.HardwareService);

        CpuWidgetHost.Child = new CpuWidget(cpuVm);
        GpuWidgetHost.Child = new GpuWidget(gpuVm);
        RamWidgetHost.Child = new RamWidget(ramVm);

        // ── Row 2: Weather + Bluetooth ──
        var weatherVm = new WeatherViewModel(App.WeatherService);
        WeatherWidgetHost.Child = new WeatherWidget(weatherVm);

        BluetoothWidgetHost.Child = new BluetoothWidget(App.BluetoothService);
    }
}
