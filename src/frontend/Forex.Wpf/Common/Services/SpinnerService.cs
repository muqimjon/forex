namespace Forex.Wpf.Common.Services;

using Forex.Wpf.Resources.UserControls;
using System;
using System.Windows;

public static class SpinnerService
{
    private static LoadingSpinner? spinner;

    public static void Init(Window mainWindow)
    {
        spinner = (mainWindow.FindName("GlobalSpinner") as LoadingSpinner)
                   ?? throw new InvalidOperationException("GlobalSpinner topilmadi");
    }

    public static void Show()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (spinner != null)
                spinner.IsActive = true;
        });
    }

    public static void Hide()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (spinner != null)
                spinner.IsActive = false;
        });
    }
}
