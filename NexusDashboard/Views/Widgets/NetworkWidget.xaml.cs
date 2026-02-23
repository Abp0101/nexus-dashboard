using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using NEXUS.ViewModels;

namespace NEXUS.Views.Widgets;

public sealed partial class NetworkWidget : UserControl
{
    public NetworkViewModel ViewModel { get; }

    public NetworkWidget(NetworkViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();

        // Update ping dot color based on latency
        ViewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(NetworkViewModel.LatencyText))
                UpdatePingDot();
        };
    }

    private void UpdatePingDot()
    {
        var text = ViewModel.LatencyText?.Replace("ms", "").Trim();
        if (long.TryParse(text, out var ms))
        {
            PingDot.Fill = ms switch
            {
                < 50 => new SolidColorBrush(ColorHelper.FromArgb(255, 0x44, 0xDD, 0x88)),   // Green
                < 100 => new SolidColorBrush(ColorHelper.FromArgb(255, 0xFF, 0x8C, 0x00)),   // Orange
                _ => new SolidColorBrush(ColorHelper.FromArgb(255, 0xFF, 0x3B, 0x5C)),       // Red
            };
        }
        else
        {
            PingDot.Fill = new SolidColorBrush(ColorHelper.FromArgb(255, 0xFF, 0x3B, 0x5C)); // Red for timeout
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
