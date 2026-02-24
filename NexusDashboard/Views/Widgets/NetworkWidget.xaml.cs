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
                < 50 => new SolidColorBrush(ColorHelper.FromArgb(255, 16, 245, 158)),   // 10F59E (AccentGreen)
                < 100 => new SolidColorBrush(ColorHelper.FromArgb(255, 255, 107, 53)),   // FF6B35 (AccentOrange)
                _ => new SolidColorBrush(ColorHelper.FromArgb(255, 220, 38, 38)),       // DC2626 (AccentDanger)
            };
        }
        else
        {
            PingDot.Fill = new SolidColorBrush(ColorHelper.FromArgb(255, 220, 38, 38)); // Danger for timeout
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
