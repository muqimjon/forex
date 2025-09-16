namespace Forex.Wpf.Windows;

using Forex.ClientService;
using Forex.Wpf.Pages.SemiProducts;
using System.Windows;
using System.Windows.Controls;

public partial class MainWindow : Window
{
    public ForexClient Client { get; private set; } = default!;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => NavigateTo(new SemiProductPage());
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
