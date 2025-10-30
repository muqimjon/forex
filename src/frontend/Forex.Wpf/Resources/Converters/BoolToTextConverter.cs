namespace Forex.Wpf.Resources.Converters;

using System.Globalization;
using System.Windows.Data;

public class BoolToTextConverter : IValueConverter
{
    // ConverterParameter formatlari:
    //  - "TrueText|FalseText"
    //  - "!TrueText|FalseText"  -> "!" bo‘lsa invert qiladi (true<->false)
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not string p || string.IsNullOrWhiteSpace(p))
            return string.Empty;

        var parts = p.Split('|');
        var trueText = parts.Length > 0 ? parts[0] : string.Empty;
        var falseText = parts.Length > 1 ? parts[1] : string.Empty;

        var invert = false;
        if (trueText.StartsWith('!'))
        {
            invert = true;
            trueText = trueText[1..];
        }

        var b = value is bool bv && bv;
        if (invert) b = !b;

        return b ? trueText : falseText;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}