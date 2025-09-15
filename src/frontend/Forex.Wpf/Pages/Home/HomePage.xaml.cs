namespace Forex.Wpf.Pages.Home;

using Forex.ClientService;
using Forex.ClientService.Services;
using Forex.Wpf.Pages.Auth;
using Forex.Wpf.Pages.Products;
using Forex.Wpf.Pages.SaleHistories;
using Forex.Wpf.Pages.Sales;
using Forex.Wpf.Pages.SemiProducts;
using Forex.Wpf.Pages.Settings;
using Forex.Wpf.Pages.ShopCashes;
using Forex.Wpf.Pages.Users;
using Forex.Wpf.Services;
using Forex.Wpf.Windows;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for HomePage.xaml
/// </summary>
public partial class HomePage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;
    private readonly ForexClient client;

    public HomePage(ForexClient client)
    {
        InitializeComponent();
        this.client = client;

        DataContext = AuthStore.Instance;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (Application.Current.MainWindow is Window mainWindow)
            WindowResizer.AnimateToSize(mainWindow, 810, 580);
    }

    private void BtnUser_Click(object sender, RoutedEventArgs e)
        => Main.NavigateTo(new UserPage(client));

    private void BtnProduct_Click(object sender, RoutedEventArgs e)
        => Main.NavigateTo(new ProductPage(client));

    private void BtnCash_Click(object sender, RoutedEventArgs e)
        => Main.NavigateTo(new ShopCashPage(client));

    private void BtnSale_Click(object sender, RoutedEventArgs e)
        => Main.NavigateTo(new SalePage(client));

    private void BtnSaleHistory_Click(object sender, RoutedEventArgs e)
        => Main.NavigateTo(new SaleHistoryPage(client));

    private void BtnSettings_Click(object sender, RoutedEventArgs e)
        => Main.NavigateTo(new SettingsPage(client));

    private void BtnSemiProduct_Click(object sender, RoutedEventArgs e)
        => Main.NavigateTo(new SemiProductPage(client));

    private void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        AuthStore.Instance.Logout();
        Main.NavigateTo(new LoginPage(client));
    }
}
