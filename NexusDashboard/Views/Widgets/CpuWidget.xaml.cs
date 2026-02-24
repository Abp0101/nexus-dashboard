using Microsoft.UI.Xaml.Controls;
using NEXUS.ViewModels;

namespace NEXUS.Views.Widgets;

public sealed partial class CpuWidget : UserControl
{
    public CpuViewModel ViewModel { get; }

    public CpuWidget(CpuViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();
    }

    private void Border_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        // Now handled via global styles or GlassHelper if preferred.
        // Left intact to keep XAML wiring valid, but we redirect implementation to GlassHelper:
        GlassHelper.AttachHoverEvents((Border)sender);
    }

    private void Border_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        // Handled via GlassHelper.
    }
}
