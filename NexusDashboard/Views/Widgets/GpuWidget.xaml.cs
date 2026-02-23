using Microsoft.UI.Xaml.Controls;
using NEXUS.ViewModels;

namespace NEXUS.Views.Widgets;

public sealed partial class GpuWidget : UserControl
{
    public GpuViewModel ViewModel { get; }

    public GpuWidget(GpuViewModel viewModel)
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
