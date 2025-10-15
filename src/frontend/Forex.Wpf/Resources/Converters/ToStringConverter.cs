namespace Forex.Wpf.Resources.Converters;

using System.Globalization;
using System.Windows.Data;

public class ToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int number16)
            return number16.ToString("#,0", new NumberFormatInfo { NumberGroupSeparator = " " });
        else if (value is decimal number32)
            return number32.ToString("N2");
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            s = s.Replace(" ", "");

            if (targetType == typeof(int) && int.TryParse(s, out int i))
                return i;

            if (targetType == typeof(decimal) && decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal d))
                return d;
        }

        return Binding.DoNothing;
    }
}