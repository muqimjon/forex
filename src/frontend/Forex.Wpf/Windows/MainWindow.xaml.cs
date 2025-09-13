namespace Forex.Wpf.Windows;

using Forex.Wpf.Pages.Home;
using System.Windows;
using System.Windows.Controls;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Loaded += (_, _) => MainFrame.Navigate(new HomePage());
    }

    public void NavigateTo<T>() where T : Page, new()
    {
        var page = new T();
        Console.WriteLine($"Navigating to: {typeof(T).Name}");
        MainFrame.Navigate(page);
    }

    public void NavigateTo(Page page)
    {
        Console.WriteLine($"Navigating to: {page.GetType().Name}");
        MainFrame.Navigate(page);
    }

    public void GoBack()
    {
        if (MainFrame.CanGoBack)
            MainFrame.GoBack();
    }
}
