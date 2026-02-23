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

        // Create ViewModels backed by the shared HardwareService singleton
        var cpuVm = new CpuViewModel(App.HardwareService);
        var gpuVm = new GpuViewModel(App.HardwareService);

        // Create widgets and inject their ViewModels
        var cpuWidget = new CpuWidget(cpuVm);
        var gpuWidget = new GpuWidget(gpuVm);

        // Attach widgets to the host containers in XAML
        CpuWidgetHost.Child = cpuWidget;
        GpuWidgetHost.Child = gpuWidget;
    }
}
