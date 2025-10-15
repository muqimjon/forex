namespace Forex.Wpf.Resources.Converters;

using System.Globalization;
using System.Windows.Data;

public class RowNumberConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is int i ? (i + 1).ToString() : "–";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}