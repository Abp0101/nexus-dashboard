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
        DeviceListPanel.Items.Clear();

        var devices = _bt.Devices;
        DeviceCountText.Text = devices.Count == 0
            ? "No paired devices found"
            : $"{devices.Count} paired device{(devices.Count == 1 ? "" : "s")}";

        foreach (var d in devices)
        {
            var isConnected = d.IsConnected;
            
            // Container Border (Flex Card)
            var card = new Border
            {
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(10, 255, 255, 255)),
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(25, 255, 255, 255)),
                BorderThickness = new Microsoft.UI.Xaml.Thickness(1),
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(12),
                Padding = new Microsoft.UI.Xaml.Thickness(12, 10, 12, 10),
                Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 8, 8),
                Width = 140, // Fixed width to ensure nice wrapping grid
                Height = 64
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = Microsoft.UI.Xaml.GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new Microsoft.UI.Xaml.GridLength(1, Microsoft.UI.Xaml.GridUnitType.Star) });

            // Emoji Icon Logic
            string emoji = "💻";
            var n = d.Name.ToLower();
            if (n.Contains("xbox") || n.Contains("controller")) emoji = "🎮";
            else if (n.Contains("head") || n.Contains("ear") || n.Contains("audio") || n.Contains("bose") || n.Contains("sony") || n.Contains("pod")) emoji = "🎧";
            else if (n.Contains("mouse") || n.Contains("logi") || n.Contains("mx") || n.Contains("basilisk")) emoji = "🖱️";
            else if (n.Contains("key")) emoji = "⌨️";
            else if (n.Contains("phone") || n.Contains("iphone") || n.Contains("galaxy")) emoji = "📱";

            var iconBorder = new Border
            {
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(8),
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(15, 255, 255, 255)),
                Width = 32, Height = 32,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 10, 0),
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
                Child = new TextBlock
                {
                    Text = emoji,
                    FontSize = 16,
                    HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
                    VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
                }
            };
            Grid.SetColumn(iconBorder, 0);

            var textStack = new StackPanel { VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center };
            textStack.Children.Add(new TextBlock
            {
                Text = d.Name,
                FontSize = 11,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White),
                TextTrimming = Microsoft.UI.Xaml.TextTrimming.CharacterEllipsis,
                MaxLines = 1
            });
            
            var statusStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4, Margin = new Microsoft.UI.Xaml.Thickness(0, 2, 0, 0) };
            statusStack.Children.Add(new Microsoft.UI.Xaml.Shapes.Ellipse
            {
                 Width = 4, Height = 4, VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
                 Fill = new Microsoft.UI.Xaml.Media.SolidColorBrush(isConnected ? Microsoft.UI.ColorHelper.FromArgb(255, 0, 212, 255) : Microsoft.UI.Colors.Gray)
            });
            statusStack.Children.Add(new TextBlock
            {
                Text = isConnected ? "Connected" : "Paired",
                FontSize = 9,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(isConnected ? Microsoft.UI.Colors.White : Microsoft.UI.ColorHelper.FromArgb(115, 255, 255, 255)),
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
            });

            textStack.Children.Add(statusStack);
            Grid.SetColumn(textStack, 1);

            grid.Children.Add(iconBorder);
            grid.Children.Add(textStack);
            card.Child = grid;

            DeviceListPanel.Items.Add(card);
        }
    }

    private void Border_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        GlassHelper.AttachHoverEvents((Border)sender);
    }

    private void Border_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
    }
}
