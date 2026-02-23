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
}
