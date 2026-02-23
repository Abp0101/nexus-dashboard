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
}
