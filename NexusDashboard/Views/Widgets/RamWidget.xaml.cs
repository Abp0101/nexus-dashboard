using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NEXUS.ViewModels;

namespace NEXUS.Views.Widgets;

public sealed partial class RamWidget : UserControl
{
    public RamViewModel ViewModel { get; }

    public RamWidget(RamViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();

        // Update the usage bar width whenever the percentage changes
        ViewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(RamViewModel.RamUsagePercent))
            {
                UpdateBar();
            }
        };

        this.SizeChanged += (_, _) => UpdateBar();
    }

    private void UpdateBar()
    {
        if (UsageBar.Parent is Grid container && container.ActualWidth > 0)
        {
            var pct = ViewModel.RamUsagePercent / 100.0;
            UsageBar.Width = container.ActualWidth * pct;
        }
    }
}
