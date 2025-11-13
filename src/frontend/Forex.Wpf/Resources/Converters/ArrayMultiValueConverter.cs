namespace Forex.Wpf.Resources.Converters;

using System.Globalization;
using System.Windows.Data;

public class ArrayMultiValueConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return values?.Clone() ?? Array.Empty<object>();
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}