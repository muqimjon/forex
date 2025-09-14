namespace Forex.Wpf.Pages.Home;

using Forex.ClientService;
using Forex.Wpf.Pages.Auth;
using Forex.Wpf.Pages.Products;
using Forex.Wpf.Pages.SaleHistories;
using Forex.Wpf.Pages.Sales;
using Forex.Wpf.Pages.Settings;
using Forex.Wpf.Pages.ShopCashes;
using Forex.Wpf.Pages.Users;
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

    private void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        ((MainWindow)Application.Current.MainWindow).NavigateTo(new LoginPage(client));
    }
}
