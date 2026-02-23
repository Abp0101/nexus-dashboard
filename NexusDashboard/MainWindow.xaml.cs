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
        // ── Background Orb Animations ──
        AnimateOrb(Orb1, 40, -30, 22);
        AnimateOrb(Orb2, -50, 25, 18);
        AnimateOrb(Orb3, 30, 45, 25);
        AnimateOrb(Orb4, -35, -40, 20);
        AnimateOrb(Orb5, 20, -20, 15);

        // ── Header Pulse Animation ──
        var pulseStoryboard = new Microsoft.UI.Xaml.Media.Animation.Storyboard();
        var pulseAnim = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
        {
            From = 1.0,
            To = 0.3,
            Duration = new Microsoft.UI.Xaml.Duration(System.TimeSpan.FromSeconds(1)),
            AutoReverse = true,
            RepeatBehavior = Microsoft.UI.Xaml.Media.Animation.RepeatBehavior.Forever,
            EasingFunction = new Microsoft.UI.Xaml.Media.Animation.SineEase { EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseInOut }
        };
        Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(pulseAnim, StatusDot);
        Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(pulseAnim, "Opacity");
        pulseStoryboard.Children.Add(pulseAnim);
        pulseStoryboard.Begin();

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

    private void AnimateOrb(Microsoft.UI.Xaml.Shapes.Ellipse orb, double moveX, double moveY, double seconds)
    {
        var transform = new Microsoft.UI.Xaml.Media.TranslateTransform();
        orb.RenderTransform = transform;

        var storyboard = new Microsoft.UI.Xaml.Media.Animation.Storyboard();
        
        var animX = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
        {
            To = moveX,
            Duration = new Microsoft.UI.Xaml.Duration(System.TimeSpan.FromSeconds(seconds)),
            AutoReverse = true,
            RepeatBehavior = Microsoft.UI.Xaml.Media.Animation.RepeatBehavior.Forever,
            EasingFunction = new Microsoft.UI.Xaml.Media.Animation.SineEase { EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseInOut }
        };
        Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(animX, transform);
        Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(animX, "X");

        var animY = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
        {
            To = moveY,
            Duration = new Microsoft.UI.Xaml.Duration(System.TimeSpan.FromSeconds(seconds * 1.1)), // Slightly different Y timing
            AutoReverse = true,
            RepeatBehavior = Microsoft.UI.Xaml.Media.Animation.RepeatBehavior.Forever,
            EasingFunction = new Microsoft.UI.Xaml.Media.Animation.SineEase { EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseInOut }
        };
        Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(animY, transform);
        Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(animY, "Y");

        storyboard.Children.Add(animX);
        storyboard.Children.Add(animY);
        storyboard.Begin();
    }
}
