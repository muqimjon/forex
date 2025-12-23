namespace Forex.Wpf.Resources.Converters;

using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

public class IndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DataGridRow row)
        {
            // Qatorning haqiqiy indeksini olish - bu eng ishonchli usul
            int index = row.GetIndex();
            return (index + 1).ToString();
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}