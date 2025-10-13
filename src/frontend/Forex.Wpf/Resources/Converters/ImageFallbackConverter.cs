namespace Forex.Wpf.Resources.Converters;

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public class ImageFallbackConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string defaultImagePath = "pack://application:,,,/Resources/Assets/default.png";

        try
        {
            if (value is BitmapImage bmp)
                return bmp;

            if (value is ImageSource imgSrc)
                return imgSrc;

            if (value is string imagePath && !string.IsNullOrWhiteSpace(imagePath))
                return new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));

            // fallback
            return new BitmapImage(new Uri(defaultImagePath));
        }
        catch
        {
            return new BitmapImage(new Uri(defaultImagePath));
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
