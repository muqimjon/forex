namespace Forex.Wpf.Windows;

using Forex.ClientService;
using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Auth;
using System.Windows;
using System.Windows.Controls;

public partial class MainWindow : Window
{
    public ForexClient Client { get; private set; } = default!;

    public MainWindow()
    {
        InitializeComponent();

        SpinnerService.Init(this);

        Loaded += (_, _) => NavigateTo(new LoginPage());
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
