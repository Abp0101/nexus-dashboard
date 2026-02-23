using Microsoft.UI.Xaml.Controls;
using NEXUS.ViewModels;

namespace NEXUS.Views.Widgets;

public sealed partial class StorageWidget : UserControl
{
    public StorageViewModel ViewModel { get; }

    public StorageWidget(StorageViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();

        ViewModel.Drives.CollectionChanged += (_, _) => BuildDriveCards();
        BuildDriveCards();
    }

    private void BuildDriveCards()
    {
        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
        {
            DriveListPanel.Children.Clear();

            foreach (var drive in ViewModel.Drives)
            {
                var drivePanel = new StackPanel { Spacing = 6, Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 10) };

                // Top label row (Name on left, used/total on right)
                var labelGrid = new Grid();
                labelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new Microsoft.UI.Xaml.GridLength(1, Microsoft.UI.Xaml.GridUnitType.Star) });
                labelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = Microsoft.UI.Xaml.GridLength.Auto });

                var leftStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
                leftStack.Children.Add(new TextBlock 
                { 
                    Text = drive.DriveName, 
                    FontSize = 13, 
                    FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(235, 255, 255, 255)) 
                });
                leftStack.Children.Add(new TextBlock 
                { 
                    Text = drive.DriveLabel, 
                    FontSize = 12, 
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(64, 255, 255, 255)),
                    VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
                });
                Grid.SetColumn(leftStack, 0);

                var rightStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
                rightStack.Children.Add(new TextBlock 
                { 
                    Text = drive.UsedGB, 
                    FontSize = 12, 
                    FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas"),
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(235, 255, 255, 255)) 
                });
                rightStack.Children.Add(new TextBlock 
                { 
                    Text = $"of {drive.TotalGB} GB", 
                    FontSize = 12, 
                    FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas"),
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(115, 255, 255, 255)) 
                });
                Grid.SetColumn(rightStack, 1);

                labelGrid.Children.Add(leftStack);
                labelGrid.Children.Add(rightStack);
                drivePanel.Children.Add(labelGrid);

                // Progress Bar Container
                var track = new Border
                {
                    Height = 4,
                    CornerRadius = new Microsoft.UI.Xaml.CornerRadius(99),
                    Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(15, 255, 255, 255))
                }; // 6% white track

                // Determine bar color based on percentage
                Microsoft.UI.Xaml.Media.Brush fillBrush;
                if (drive.UsedPercent >= 90)
                    fillBrush = (App.Current.Resources["AccentDanger"] as Microsoft.UI.Xaml.Media.SolidColorBrush)!;
                else if (drive.UsedPercent >= 70)
                    fillBrush = (App.Current.Resources["AccentOrange"] as Microsoft.UI.Xaml.Media.SolidColorBrush)!;
                else
                    fillBrush = (App.Current.Resources["CyanVioletGradient"] as Microsoft.UI.Xaml.Media.LinearGradientBrush)!;

                var fill = new Border
                {
                    CornerRadius = new Microsoft.UI.Xaml.CornerRadius(99),
                    HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left,
                    Background = fillBrush,
                    Width = 0 // start at 0
                };

                track.Child = fill;
                
                double pct = drive.UsedPercent / 100.0;
                track.SizeChanged += (s, e) =>
                {
                    if (e.NewSize.Width > 0 && fill.Width == 0) // animate only initially
                    {
                        var targetWidth = e.NewSize.Width * pct;
                        var storyboard = new Microsoft.UI.Xaml.Media.Animation.Storyboard();
                        var animation = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
                        {
                            To = targetWidth,
                            Duration = new Microsoft.UI.Xaml.Duration(System.TimeSpan.FromMilliseconds(300)),
                            EnableDependentAnimation = true,
                            EasingFunction = new Microsoft.UI.Xaml.Media.Animation.ExponentialEase { EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseOut }
                        };
                        Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(animation, fill);
                        Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(animation, "Width");
                        storyboard.Children.Add(animation);
                        storyboard.Begin();
                    }
                };

                drivePanel.Children.Add(track);
                DriveListPanel.Children.Add(drivePanel);
            }
        });
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
