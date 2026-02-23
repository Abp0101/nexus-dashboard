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
        DeviceListPanel.Children.Clear();

        foreach (var d in ViewModel.Devices)
        {
            if (d.IsControllable)
            {
                var cb = new CheckBox
                {
                    Content = $"{d.Name}  ({d.ZoneLabel})",
                    IsChecked = d.IsSelected,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White),
                    Tag = d
                };
                cb.Checked += (s, _) => { if (s is CheckBox c && c.Tag is RgbDeviceDisplay dev) dev.IsSelected = true; };
                cb.Unchecked += (s, _) => { if (s is CheckBox c && c.Tag is RgbDeviceDisplay dev) dev.IsSelected = false; };
                DeviceListPanel.Children.Add(cb);
            }
            else
            {
                var tb = new TextBlock
                {
                    Text = $"  {d.Name}  ({d.ZoneLabel})",
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 0x88, 0x92, 0xA4)),
                    FontSize = 13,
                    Margin = new Microsoft.UI.Xaml.Thickness(0, 4, 0, 4)
                };
                DeviceListPanel.Children.Add(tb);
            }
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
