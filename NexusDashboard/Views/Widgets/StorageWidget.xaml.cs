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
                var drivePanel = new StackPanel { Spacing = 8, Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 0) };

                // Top label row (Name on left, used/total on right)
                var labelGrid = new Grid();
                labelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new Microsoft.UI.Xaml.GridLength(1, Microsoft.UI.Xaml.GridUnitType.Star) });
                labelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = Microsoft.UI.Xaml.GridLength.Auto });

                var leftStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
                leftStack.Children.Add(new TextBlock 
                { 
                    Text = drive.DriveName, 
                    FontSize = 12, 
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White) 
                });
                leftStack.Children.Add(new TextBlock 
                { 
                    Text = drive.DriveLabel, 
                    FontSize = 11, 
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(64, 255, 255, 255)),
                    VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
                });
                Grid.SetColumn(leftStack, 0);

                var rightStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
                rightStack.Children.Add(new TextBlock 
                { 
                    Text = drive.UsedGB.Replace(" GB", ""), // Strip GB to mimic spec
                    FontSize = 11, 
                    FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas"),
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White)
                });
                rightStack.Children.Add(new TextBlock 
                { 
                    Text = $"of {drive.TotalGB}", 
                    FontSize = 11, 
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
                }; 

                // Determine bar color based on percentage
                Microsoft.UI.Xaml.Media.Brush fillBrush;
                if (drive.UsedPercent >= 90)
                    fillBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 220, 38, 38)); // Red-600 #DC2626
                else if (drive.UsedPercent >= 70)
                    fillBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 255, 107, 53)); // AccentOrange
                else
                {
                    var gradient = new Microsoft.UI.Xaml.Media.LinearGradientBrush
                    {
                        StartPoint = new Windows.Foundation.Point(0, 0),
                        EndPoint = new Windows.Foundation.Point(1, 0)
                    };
                    gradient.GradientStops.Add(new Microsoft.UI.Xaml.Media.GradientStop { Color = Microsoft.UI.ColorHelper.FromArgb(255, 0, 212, 255), Offset = 0 });
                    gradient.GradientStops.Add(new Microsoft.UI.Xaml.Media.GradientStop { Color = Microsoft.UI.ColorHelper.FromArgb(255, 139, 92, 246), Offset = 1 });
                    fillBrush = gradient;
                }

                var fill = new Border
                {
                    CornerRadius = new Microsoft.UI.Xaml.CornerRadius(99),
                    HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left,
                    Background = fillBrush,
                    Width = 0 // start at 0 for animation
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
        GlassHelper.AttachHoverEvents((Border)sender);
    }

    private void Border_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
    }
}
