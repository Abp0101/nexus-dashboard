using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using NEXUS.Services;
using Windows.UI;

namespace NEXUS.ViewModels;

/// <summary>
/// Exposes RGB device list, color selection, and commands.
/// Razer Essential gets ON/OFF toggle only (green/black).
/// All other devices get color picker + Apply.
/// </summary>
#pragma warning disable MVVMTK0045
public partial class RgbViewModel : ObservableObject, IDisposable
{
    private readonly RgbService _rgb;
    private readonly DispatcherQueue _dispatcher;

    [ObservableProperty]
    private Color _selectedColor = Color.FromArgb(255, 0x44, 0x88, 0xFF);

    [ObservableProperty]
    private bool _isEnabled = true;

    [ObservableProperty]
    private string _connectionStatus = "Connecting...";

    [ObservableProperty]
    private bool _isConnected;

    public ObservableCollection<RgbDeviceDisplay> Devices { get; } = new();

    public RgbViewModel(RgbService rgbService)
    {
        _rgb = rgbService;
        _dispatcher = DispatcherQueue.GetForCurrentThread();
        _rgb.DevicesUpdated += OnDevicesUpdated;
        RefreshUi();
    }

    private void OnDevicesUpdated(object? sender, EventArgs e)
    {
        _dispatcher.TryEnqueue(RefreshUi);
    }

    private void RefreshUi()
    {
        IsConnected = _rgb.IsConnected;
        ConnectionStatus = _rgb.IsConnected
            ? $"{_rgb.Devices.Count} device{(_rgb.Devices.Count == 1 ? "" : "s")} detected"
            : $"Disconnected: {_rgb.ConnectionError ?? "OpenRGB not running"}";

        Devices.Clear();
        foreach (var d in _rgb.Devices)
        {
            string zoneLabel;
            if (d.IsHardwiredGreen)
                zoneLabel = "(Green Only — ON/OFF)";
            else if (d.LedCount == 1)
                zoneLabel = $"(Single Zone)";
            else
                zoneLabel = $"({d.LedCount} LEDs)";

            Devices.Add(new RgbDeviceDisplay
            {
                Name = d.Name,
                DeviceIndex = d.DeviceIndex,
                LedCount = d.LedCount,
                ZoneLabel = zoneLabel,
                IsHardwiredGreen = d.IsHardwiredGreen,
                IsSelected = !d.IsHardwiredGreen
            });
        }
    }

    /// <summary>
    /// Applies selected color to all checked (non-hardwired) devices via UpdateLeds.
    /// </summary>
    [RelayCommand]
    private void ApplyColor()
    {
        if (!_rgb.IsConnected || !_rgb.IsEnabled) return;

        foreach (var d in Devices)
        {
            if (d.IsSelected && !d.IsHardwiredGreen)
            {
                _rgb.SendColorToDevice(d.DeviceIndex, SelectedColor.R, SelectedColor.G, SelectedColor.B);
            }
        }
    }

    /// <summary>
    /// Toggles a Razer Essential device: ON = green (0,255,0), OFF = black (0,0,0).
    /// </summary>
    [RelayCommand]
    private void ToggleEssential(RgbDeviceDisplay device)
    {
        if (!_rgb.IsConnected) return;

        device.IsOn = !device.IsOn;

        if (device.IsOn)
            _rgb.SendColorToDevice(device.DeviceIndex, 0, 255, 0); // Green
        else
            _rgb.SendColorToDevice(device.DeviceIndex, 0, 0, 0);   // Off
    }

    /// <summary>
    /// Master toggle: OFF = TurnOffAll (black via UpdateLeds), ON = re-apply colors.
    /// </summary>
    [RelayCommand]
    private void ToggleMaster()
    {
        IsEnabled = !IsEnabled;
        _rgb.IsEnabled = IsEnabled;

        if (!IsEnabled)
            _rgb.TurnOffAll();
        else
            ApplyColor();
    }

    public void Dispose()
    {
        _rgb.DevicesUpdated -= OnDevicesUpdated;
    }
}
#pragma warning restore MVVMTK0045

/// <summary>Display model for a single RGB device.</summary>
public class RgbDeviceDisplay : ObservableObject
{
    public string Name { get; set; } = "";
    public int DeviceIndex { get; set; }
    public int LedCount { get; set; }
    public string ZoneLabel { get; set; } = "";
    public bool IsHardwiredGreen { get; set; }
    public bool IsControllable => !IsHardwiredGreen;

    private bool _isSelected = true;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    private bool _isOn = true;
    /// <summary>For Razer Essential: whether the green LED is currently on.</summary>
    public bool IsOn
    {
        get => _isOn;
        set => SetProperty(ref _isOn, value);
    }
}
