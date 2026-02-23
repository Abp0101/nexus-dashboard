using Microsoft.UI.Xaml.Controls;
using NEXUS.ViewModels;

namespace NEXUS.Views.Widgets;

public sealed partial class RgbWidget : UserControl
{
    public RgbViewModel ViewModel { get; }

    public RgbWidget(RgbViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();

        MasterToggle.Toggled += (_, _) =>
        {
            if (ViewModel.IsEnabled != MasterToggle.IsOn)
                ViewModel.ToggleMasterCommand.Execute(null);
        };

        ViewModel.Devices.CollectionChanged += (_, _) => BuildDeviceList();
        BuildDeviceList();
        
        RgbColorPicker.Color = ViewModel.SelectedColor;
    }

    private void RgbColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
    {
        ViewModel.SelectedColor = args.NewColor;
    }

    private void BuildDeviceList()
    {
        DeviceListPanel.Items.Clear();

        foreach (var d in ViewModel.Devices)
        {
            if (d.IsControllable)
            {
                // Create a stylized ToggleButton-like card
                var tb = new Microsoft.UI.Xaml.Controls.Primitives.ToggleButton
                {
                    Content = new StackPanel
                    {
                        Spacing = 2,
                        Children = 
                        {
                            new TextBlock { Text = d.Name, FontSize = 10, Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White) },
                            new TextBlock { Text = d.ZoneLabel, FontSize = 9, Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(115, 255, 255, 255)) }
                        }
                    },
                    IsChecked = d.IsSelected,
                    Tag = d,
                    Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 8, 8),
                    Padding = new Microsoft.UI.Xaml.Thickness(12, 8, 12, 8),
                    CornerRadius = new Microsoft.UI.Xaml.CornerRadius(8),
                    Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(10, 255, 255, 255)),
                    BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(25, 255, 255, 255)),
                    BorderThickness = new Microsoft.UI.Xaml.Thickness(1)
                };
                
                tb.Checked += (s, _) => { if (s is Microsoft.UI.Xaml.Controls.Primitives.ToggleButton c && c.Tag is RgbDeviceDisplay dev) dev.IsSelected = true; };
                tb.Unchecked += (s, _) => { if (s is Microsoft.UI.Xaml.Controls.Primitives.ToggleButton c && c.Tag is RgbDeviceDisplay dev) dev.IsSelected = false; };
                
                DeviceListPanel.Items.Add(tb);
            }
            else
            {
                var border = new Border
                {
                    Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 8, 8),
                    Padding = new Microsoft.UI.Xaml.Thickness(12, 8, 12, 8),
                    CornerRadius = new Microsoft.UI.Xaml.CornerRadius(8),
                    Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(5, 255, 255, 255)),
                    BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(10, 255, 255, 255)),
                    BorderThickness = new Microsoft.UI.Xaml.Thickness(1),
                    Child = new StackPanel
                    {
                        Spacing = 2,
                        Children = 
                        {
                            new TextBlock { Text = d.Name, FontSize = 10, Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(64, 255, 255, 255)) },
                            new TextBlock { Text = "Sync Only", FontSize = 9, Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(40, 255, 255, 255)) }
                        }
                    }
                };
                DeviceListPanel.Items.Add(border);
            }
        }
    }

    private void Border_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        GlassHelper.AttachHoverEvents((Border)sender);
    }

    private void Border_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
    }
}
