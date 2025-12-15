namespace Forex.Wpf.Resources.Converters;

using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

public class IndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // XAML dagi o'zgartirish tufayli, 'value' endi DataGridRow obyekti bo'ladi.
        if (value is DataGridRow row)
        {
            try
            {
                // 1. DataGrid ni DataGridRow dan topish
                DataGrid dataGrid = ItemsControl.ItemsControlFromItemContainer(row) as DataGrid;

                if (dataGrid is not null)
                {
                    // 2. DataGrid ning ItemsSource ichida elementning indeksini topish
                    // row.Item bu bizning UserResponse obyekti
                    int index = dataGrid.Items.IndexOf(row.Item);

                    // Tartib raqami 1 dan boshlanishi uchun +1
                    if (index >= 0)
                    {
                        return (index + 1).ToString();
                    }
                }
            }
            catch (Exception)
            {
                // Xatolar yuz berganda
            }
        }

        // Agar Index topilmasa
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}