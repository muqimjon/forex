namespace Forex.Wpf.Common.Services;

using Forex.Wpf.Common.Interfaces;
using System.Windows.Controls;
using System.Windows.Navigation;

public class NavigationService : INavigationService
{
    private readonly Frame frame;

    public NavigationService(Frame frame)
    {
        this.frame = frame;
        this.frame.Navigated += Frame_Navigated;
    }

    public void NavigateTo(Page page)
    {
        frame.Navigate(page);
    }

    public void GoBack()
    {
        if (frame.CanGoBack)
            frame.GoBack();
    }

    public bool CanGoBack => frame.CanGoBack;

    private void Frame_Navigated(object? sender, NavigationEventArgs e)
    {
        if (e.Content is Page page && page.DataContext is INavigationAware aware)
        {
            aware.OnNavigatedTo();
        }
    }
}
