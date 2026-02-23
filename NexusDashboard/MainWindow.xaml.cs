using Microsoft.UI.Xaml;
using NEXUS.ViewModels;
using NEXUS.Views.Widgets;

namespace NEXUS;

public sealed partial class MainWindow : Window
{
    private readonly DispatcherTimer _clockTimer;

    public MainWindow()
    {
        this.InitializeComponent();
        Title = "NEXUS Dashboard";

        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (s, e) => ClockText.Text = DateTime.Now.ToString("HH:mm:ss");
        _clockTimer.Start();
        ClockText.Text = DateTime.Now.ToString("HH:mm:ss");

        // ── Row 1: Hardware widgets ──
        var cpuVm = new CpuViewModel(App.HardwareService);
        var gpuVm = new GpuViewModel(App.HardwareService);
        var ramVm = new RamViewModel(App.HardwareService);

        CpuWidgetHost.Child = new CpuWidget(cpuVm);
        GpuWidgetHost.Child = new GpuWidget(gpuVm);
        RamWidgetHost.Child = new RamWidget(ramVm);

        // ── Row 2: Weather + Bluetooth + RGB ──
        var weatherVm = new WeatherViewModel(App.WeatherService);
        WeatherWidgetHost.Child = new WeatherWidget(weatherVm);

        BluetoothWidgetHost.Child = new BluetoothWidget(App.BluetoothService);

        var rgbVm = new RgbViewModel(App.RgbService);
        RgbWidgetHost.Child = new RgbWidget(rgbVm);

        // ── Row 3: Network + Storage ──
        var netVm = new NetworkViewModel(App.NetworkService);
        NetworkWidgetHost.Child = new NetworkWidget(netVm);

        var storageVm = new StorageViewModel(App.StorageService);
        StorageWidgetHost.Child = new StorageWidget(storageVm);
    }
}
