using Microsoft.UI.Xaml.Controls;
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
        DeviceListPanel.Children.Clear();

        var devices = _bt.Devices;
        DeviceCountText.Text = devices.Count == 0
            ? "No paired devices found"
            : $"{devices.Count} paired device{(devices.Count == 1 ? "" : "s")}";

        foreach (var d in devices)
        {
            var isConnected = d.IsConnected;
            
            // Container Border
            var card = new Border
            {
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(8, 255, 255, 255)), // ~3%
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(18, 255, 255, 255)), // ~7%
                BorderThickness = new Microsoft.UI.Xaml.Thickness(1),
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(14),
                Padding = new Microsoft.UI.Xaml.Thickness(16)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = Microsoft.UI.Xaml.GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new Microsoft.UI.Xaml.GridLength(1, Microsoft.UI.Xaml.GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = Microsoft.UI.Xaml.GridLength.Auto });

            // Ensure emoji logic based on name heuristics
            string emoji = "💻";
            var n = d.Name.ToLower();
            if (n.Contains("xbox") || n.Contains("controller")) emoji = "🎮";
            else if (n.Contains("head") || n.Contains("ear") || n.Contains("audio") || n.Contains("bose") || n.Contains("sony")) emoji = "🎧";
            else if (n.Contains("mouse") || n.Contains("logi") || n.Contains("mx")) emoji = "🖱️";
            else if (n.Contains("key")) emoji = "⌨️";

            var iconTb = new TextBlock
            {
                Text = emoji,
                FontSize = 20,
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 16, 0)
            };
            Grid.SetColumn(iconTb, 0);

            var nameStack = new StackPanel { VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center };
            nameStack.Children.Add(new TextBlock
            {
                Text = d.Name,
                FontSize = 13,
                FontWeight = Microsoft.UI.Text.FontWeights.Medium,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(235, 255, 255, 255))
            });
            nameStack.Children.Add(new TextBlock
            {
                Text = isConnected ? "BLUETOOTH DEVICE" : "PAIRED DEVICE",
                FontSize = 10,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(64, 255, 255, 255)),
                CharacterSpacing = 100,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 2, 0, 0)
            });
            Grid.SetColumn(nameStack, 1);

            var statusStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center };
            var dot = new Microsoft.UI.Xaml.Shapes.Ellipse
            {
                Width = 6, Height = 6,
                Fill = new Microsoft.UI.Xaml.Media.SolidColorBrush(isConnected ? Microsoft.UI.ColorHelper.FromArgb(255, 16, 245, 158) : Microsoft.UI.Colors.Gray)
            };

            statusStack.Children.Add(dot);
            statusStack.Children.Add(new TextBlock
            {
                Text = isConnected ? "CONNECTED" : "PAIRED",
                FontSize = 10,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(isConnected ? Microsoft.UI.ColorHelper.FromArgb(255, 16, 245, 158) : Microsoft.UI.Colors.Gray)
            });
            Grid.SetColumn(statusStack, 2);

            grid.Children.Add(iconTb);
            grid.Children.Add(nameStack);
            grid.Children.Add(statusStack);
            card.Child = grid;

            DeviceListPanel.Children.Add(card);
        }
    }

    private void Border_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is Border b)
            b.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(41, 255, 255, 255));
    }

    private void Border_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is Border b)
            b.BorderBrush = App.Current.Resources["GlassBorder"] as Microsoft.UI.Xaml.Media.Brush;
    }
}
