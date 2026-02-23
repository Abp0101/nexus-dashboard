using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using NEXUS.Services;

namespace NEXUS.Views.Widgets;

public sealed partial class BluetoothWidget : UserControl
{
    private readonly BluetoothService _bt;

    public BluetoothWidget(BluetoothService bluetoothService)
    {
        _bt = bluetoothService;
        this.InitializeComponent();

        _bt.DevicesUpdated += OnDevicesUpdated;
        Refresh();
    }

    private void OnDevicesUpdated(object? sender, EventArgs e)
    {
        DispatcherQueue.TryEnqueue(Refresh);
    }

    private void Refresh()
    {
        var items = new List<BluetoothDeviceDisplay>();

        foreach (var d in _bt.Devices)
        {
            items.Add(new BluetoothDeviceDisplay
            {
                Name = d.Name,
                StatusColor = new SolidColorBrush(GetBatteryColor(d)),
                StatusText = FormatStatus(d)
            });
        }

        DeviceCountText.Text = items.Count == 0
            ? "No paired devices found"
            : $"{items.Count} paired device{(items.Count == 1 ? "" : "s")}";

        DeviceList.ItemsSource = items;
    }

    /// <summary>
    /// Green > 50%, Amber 20-50%, Red < 20%, Grey if unavailable.
    /// </summary>
    private static Windows.UI.Color GetBatteryColor(BluetoothDeviceInfo d)
    {
        if (d.BatteryLevel is null)
            return ColorHelper.FromArgb(255, 0x55, 0x55, 0x66); // Grey

        return d.BatteryLevel.Value switch
        {
            > 50 => ColorHelper.FromArgb(255, 0x44, 0xDD, 0x88), // Green
            >= 20 => ColorHelper.FromArgb(255, 0xFF, 0xAA, 0x33), // Amber
            _ => ColorHelper.FromArgb(255, 0xFF, 0x44, 0x44),     // Red
        };
    }

    private static string FormatStatus(BluetoothDeviceInfo d)
    {
        if (d.BatteryLevel.HasValue)
            return $"🔋 {d.BatteryLevel}%";

        return d.IsConnected ? "Connected" : "Paired";
    }
}

/// <summary>Display model for the DataTemplate.</summary>
public class BluetoothDeviceDisplay
{
    public string Name { get; init; } = "";
    public SolidColorBrush StatusColor { get; init; } = new(Colors.Gray);
    public string StatusText { get; init; } = "";
}
