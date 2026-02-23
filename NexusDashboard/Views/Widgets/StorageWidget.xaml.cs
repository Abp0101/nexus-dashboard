using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using NEXUS.ViewModels;

namespace NEXUS.Views.Widgets;

public sealed partial class StorageWidget : UserControl
{
    public StorageViewModel ViewModel { get; }

    public StorageWidget(StorageViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();

        // Rebuild progress bars when drives collection changes
        ViewModel.Drives.CollectionChanged += (_, _) => BuildProgressBars();
        this.SizeChanged += (_, _) => BuildProgressBars();
    }

    /// <summary>
    /// Walks the ItemsRepeater to inject colored progress bar Borders
    /// into each drive's Grid (the 6px progress bar track).
    /// </summary>
    private void BuildProgressBars()
    {
        // Small delay to allow the ItemsRepeater to realize items
        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
        {
            for (int i = 0; i < ViewModel.Drives.Count; i++)
            {
                var element = DriveList.TryGetElement(i);
                if (element is not StackPanel panel) continue;

                // The Grid is the last child (the progress bar track)
                if (panel.Children.Count < 3) continue;
                if (panel.Children[2] is not Grid barGrid) continue;

                var drive = ViewModel.Drives[i];
                var pct = Math.Clamp(drive.UsedPercent / 100.0, 0, 1);

                // Parse the hex color
                var brush = drive.BarColor switch
                {
                    "#FF4444" => new SolidColorBrush(ColorHelper.FromArgb(255, 0xFF, 0x44, 0x44)),
                    "#FFAA33" => new SolidColorBrush(ColorHelper.FromArgb(255, 0xFF, 0xAA, 0x33)),
                    _ => new SolidColorBrush(ColorHelper.FromArgb(255, 0x44, 0xDD, 0x88)),
                };

                // Remove existing colored bars (keep only the background border)
                while (barGrid.Children.Count > 1)
                    barGrid.Children.RemoveAt(barGrid.Children.Count - 1);

                var bar = new Border
                {
                    Background = brush,
                    CornerRadius = new CornerRadius(3),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Width = barGrid.ActualWidth > 0 ? barGrid.ActualWidth * pct : 0
                };

                barGrid.Children.Add(bar);
            }
        });
    }
}
