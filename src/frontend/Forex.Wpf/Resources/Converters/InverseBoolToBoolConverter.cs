namespace Forex.Wpf.Resources.Converters;

using System.Globalization;
using System.Windows;
using System.Windows.Data;

public class InverseBoolToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b ? !b : DependencyProperty.UnsetValue;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b ? !b : DependencyProperty.UnsetValue;
}
