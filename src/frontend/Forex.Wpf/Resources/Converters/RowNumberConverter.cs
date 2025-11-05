namespace Forex.Wpf.Resources.Converters;

using System.Globalization;
using System.Windows.Data;

public class RowNumberConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int index)
            return index + 1; // 0 dan emas, 1 dan boshlaydi
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
