namespace Forex.Wpf.Resources.Converters;

using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

public class BalanceToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal balance)
        {
            return balance > 0 ? Brushes.Green : balance < 0 ? Brushes.Red : Brushes.Black;
        }
        return Brushes.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}