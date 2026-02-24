using NEXUS.Services;

namespace NEXUS;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private Window? _window;

    // ── Singleton services ──
    public static HardwareService HardwareService { get; } = new();
    public static WeatherService WeatherService { get; } = new();
    public static BluetoothService BluetoothService { get; } = new();
    public static RgbService RgbService { get; } = new();
    public static NetworkService NetworkService { get; } = new();
    public static StorageService StorageService { get; } = new();

    public App()
    {
        this.InitializeComponent();
        this.UnhandledException += OnUnhandledException;
    }

    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
        _window = new MainWindow();
        _window.Activate();
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        e.Handled = true;
    }
}
