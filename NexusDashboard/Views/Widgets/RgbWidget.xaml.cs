using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using NEXUS.ViewModels;

namespace NEXUS.Views.Widgets;

public sealed partial class RgbWidget : UserControl
{
    public RgbViewModel ViewModel { get; }

    public RgbWidget(RgbViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();

        // Master toggle wiring
        MasterToggle.Toggled += (_, _) =>
        {
            if (ViewModel.IsEnabled != MasterToggle.IsOn)
                ViewModel.ToggleMasterCommand.Execute(null);
        };

        // Build Essential ON/OFF buttons when devices update
        ViewModel.Devices.CollectionChanged += (_, _) => BuildEssentialButtons();
        BuildEssentialButtons();
    }

    /// <summary>
    /// Creates ON/OFF toggle buttons for each Razer Essential device.
    /// </summary>
    private void BuildEssentialButtons()
    {
        EssentialButtons.Children.Clear();

        foreach (var d in ViewModel.Devices)
        {
            if (!d.IsHardwiredGreen) continue;

            var btn = new Button
            {
                Content = $"💡 {d.Name} — {(d.IsOn ? "ON" : "OFF")}",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush(d.IsOn
                    ? ColorHelper.FromArgb(255, 0x22, 0x66, 0x33)   // Dark green
                    : ColorHelper.FromArgb(255, 0x1A, 0x1A, 0x25)), // Dark grey
                Foreground = new SolidColorBrush(d.IsOn
                    ? ColorHelper.FromArgb(255, 0x44, 0xDD, 0x88)
                    : ColorHelper.FromArgb(255, 0x66, 0x77, 0x99)),
                Tag = d
            };

            btn.Click += (sender, _) =>
            {
                if (sender is Button b && b.Tag is RgbDeviceDisplay device)
                {
                    ViewModel.ToggleEssentialCommand.Execute(device);

                    // Update button appearance
                    b.Content = $"💡 {device.Name} — {(device.IsOn ? "ON" : "OFF")}";
                    b.Background = new SolidColorBrush(device.IsOn
                        ? ColorHelper.FromArgb(255, 0x22, 0x66, 0x33)
                        : ColorHelper.FromArgb(255, 0x1A, 0x1A, 0x25));
                    b.Foreground = new SolidColorBrush(device.IsOn
                        ? ColorHelper.FromArgb(255, 0x44, 0xDD, 0x88)
                        : ColorHelper.FromArgb(255, 0x66, 0x77, 0x99));
                }
            };

            EssentialButtons.Children.Add(btn);
        }
    }
}
