using System.Windows;
using System.Windows.Media;

namespace TapWeaver.UI.Themes;

public static class ThemeService
{
    public static void ApplyTheme(bool useDarkMode)
    {
        var app = Application.Current;
        if (app == null)
            return;

        ApplyColor(app.Resources, "PrimaryColor", useDarkMode ? "#4DA3FF" : "#2196F3");
        ApplyColor(app.Resources, "AccentColor", useDarkMode ? "#2E7DD1" : "#1565C0");
        ApplyColor(app.Resources, "SuccessColor", useDarkMode ? "#56C271" : "#4CAF50");
        ApplyColor(app.Resources, "DangerColor", useDarkMode ? "#FF6B6B" : "#F44336");
        ApplyColor(app.Resources, "WarningColor", useDarkMode ? "#FFB347" : "#FF9800");
        ApplyColor(app.Resources, "BackgroundColor", useDarkMode ? "#101316" : "#F5F5F5");
        ApplyColor(app.Resources, "SurfaceColor", useDarkMode ? "#1B2229" : "#FFFFFF");
        ApplyColor(app.Resources, "TextPrimaryColor", useDarkMode ? "#F3F5F7" : "#212121");
        ApplyColor(app.Resources, "TextSecondaryColor", useDarkMode ? "#A8B1BA" : "#757575");
        ApplyColor(app.Resources, "BorderColor", useDarkMode ? "#2C353F" : "#E0E0E0");

        ApplyBrushColor(app.Resources, "PrimaryBrush", useDarkMode ? "#4DA3FF" : "#2196F3");
        ApplyBrushColor(app.Resources, "AccentBrush", useDarkMode ? "#2E7DD1" : "#1565C0");
        ApplyBrushColor(app.Resources, "SuccessBrush", useDarkMode ? "#56C271" : "#4CAF50");
        ApplyBrushColor(app.Resources, "DangerBrush", useDarkMode ? "#FF6B6B" : "#F44336");
        ApplyBrushColor(app.Resources, "WarningBrush", useDarkMode ? "#FFB347" : "#FF9800");
        ApplyBrushColor(app.Resources, "BackgroundBrush", useDarkMode ? "#101316" : "#F5F5F5");
        ApplyBrushColor(app.Resources, "SurfaceBrush", useDarkMode ? "#1B2229" : "#FFFFFF");
        ApplyBrushColor(app.Resources, "TextPrimaryBrush", useDarkMode ? "#F3F5F7" : "#212121");
        ApplyBrushColor(app.Resources, "TextSecondaryBrush", useDarkMode ? "#A8B1BA" : "#757575");
        ApplyBrushColor(app.Resources, "CardBorderBrush", useDarkMode ? "#2C353F" : "#E0E0E0");
    }

    private static void ApplyColor(ResourceDictionary resources, string key, string hex)
    {
        resources[key] = (Color)ColorConverter.ConvertFromString(hex);
    }

    private static void ApplyBrushColor(ResourceDictionary resources, string key, string hex)
    {
        var color = (Color)ColorConverter.ConvertFromString(hex);
        if (resources[key] is SolidColorBrush brush)
        {
            brush.Color = color;
            return;
        }

        resources[key] = new SolidColorBrush(color);
    }
}
