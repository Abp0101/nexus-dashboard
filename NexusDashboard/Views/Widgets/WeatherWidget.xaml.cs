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
        GlassHelper.AttachHoverEvents((Border)sender);
    }

    private void Border_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
    }
}
