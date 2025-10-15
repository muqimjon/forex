namespace Forex.Wpf.Resources.Converters;

using System.Globalization;
using System.Windows.Data;

public class IntToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (int.TryParse(value?.ToString(), out var result))
            return result;

        return 0; // yoki default qiymat
    }
}