using Microsoft.UI.Xaml.Controls;
using NEXUS.ViewModels;

namespace NEXUS.Views.Widgets;

public sealed partial class NetworkWidget : UserControl
{
    public NetworkViewModel ViewModel { get; }

    public NetworkWidget(NetworkViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();
    }
}
