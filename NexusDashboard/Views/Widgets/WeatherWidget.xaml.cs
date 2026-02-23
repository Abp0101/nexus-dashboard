using Microsoft.UI.Xaml.Controls;
using NEXUS.ViewModels;

namespace NEXUS.Views.Widgets;

public sealed partial class WeatherWidget : UserControl
{
    public WeatherViewModel ViewModel { get; }

    public WeatherWidget(WeatherViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();
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
