using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace NEXUS.Views
{
    public static class GlassHelper
    {
        public static void AttachHoverEvents(Border border)
        {
            if (border == null) return;
            
            border.PointerEntered += (s, e) =>
            {
                if (Application.Current.Resources.TryGetValue("GlassHoverColor", out var hoverColor) && hoverColor is Windows.UI.Color color)
                {
                    border.BorderBrush = new SolidColorBrush(color);
                }
                if (Application.Current.Resources.TryGetValue("GlassBackgroundHover", out var hoverBg) && hoverBg is Brush bgBrush)
                {
                    border.Background = bgBrush;
                }
            };
            
            border.PointerExited += (s, e) =>
            {
                if (Application.Current.Resources.TryGetValue("GlassBorder", out var borderBrush) && borderBrush is Brush standardBorder)
                {
                    border.BorderBrush = standardBorder;
                }
                if (Application.Current.Resources.TryGetValue("GlassBackground", out var bg) && bg is Brush bgBrush)
                {
                    border.Background = bgBrush;
                }
            };
        }
    }
}
