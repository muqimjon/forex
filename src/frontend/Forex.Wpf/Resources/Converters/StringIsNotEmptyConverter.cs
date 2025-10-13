namespace Forex.Wpf.Resources.Converters;

using System.Globalization;
using System.Windows.Data;

public class StringIsNotEmptyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is string str && !string.IsNullOrWhiteSpace(str);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}