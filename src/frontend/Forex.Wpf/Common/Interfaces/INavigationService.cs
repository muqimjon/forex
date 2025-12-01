namespace Forex.Wpf.Common.Interfaces;

using System.Windows.Controls;

public interface INavigationService
{
    void NavigateTo(Page page);
    void GoBack();
    bool CanGoBack { get; }
}
