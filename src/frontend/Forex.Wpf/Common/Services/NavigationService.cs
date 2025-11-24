namespace Forex.Wpf.Common.Services;

using Forex.Wpf.Common.Interfaces;
using Forex.Wpf.Windows;
using System.Windows.Controls;

public class NavigationService : INavigationService
{
    private readonly MainWindow _mainWindow;

    public NavigationService(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public void NavigateTo(Page page)
    {
        _mainWindow.NavigateTo(page);
    }

    public void GoBack()
    {
        _mainWindow.GoBack();
    }

    public bool CanGoBack => _mainWindow.MainFrame.CanGoBack;
}
