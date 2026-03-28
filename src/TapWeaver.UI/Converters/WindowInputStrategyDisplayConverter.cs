using System.Globalization;
using System.Windows.Data;
using TapWeaver.Core.Input;

namespace TapWeaver.UI.Converters;

public sealed class WindowInputStrategyDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not WindowInputStrategy strategy)
            return string.Empty;

        return strategy switch
        {
            WindowInputStrategy.FocusSwitch => "Focus Switch (empfohlen)",
            WindowInputStrategy.PostMessage => "PostMessage (Hintergrund)",
            _ => strategy.ToString()
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
