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
        if (UsageBar.Parent is Border container && container.ActualWidth > 0)
        {
            var pct = ViewModel.RamUsagePercent / 100.0;
            double targetWidth = container.ActualWidth * pct;
            
            var storyboard = new Microsoft.UI.Xaml.Media.Animation.Storyboard();
            var animation = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
            {
                To = targetWidth,
                Duration = new Microsoft.UI.Xaml.Duration(System.TimeSpan.FromMilliseconds(300)),
                EnableDependentAnimation = true,
                EasingFunction = new Microsoft.UI.Xaml.Media.Animation.ExponentialEase { EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseOut }
            };
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(animation, UsageBar);
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(animation, "Width");
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }
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
