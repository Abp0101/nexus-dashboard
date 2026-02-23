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
}
