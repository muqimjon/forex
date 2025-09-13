namespace Forex.Wpf.Pages.Home;
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
    public HomePage()
    {
        InitializeComponent();
    }

    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;

    private void BtnUser_Click(object sender, RoutedEventArgs e) => Main.NavigateTo<UserPage>();
    private void BtnProduct_Click(object sender, RoutedEventArgs e) => Main.NavigateTo<ProductPage>();
    private void BtnCash_Click(object sender, RoutedEventArgs e) => Main.NavigateTo<ShopCashPage>();
    private void BtnSale_Click(object sender, RoutedEventArgs e) => Main.NavigateTo<SalePage>();
    private void BtnSaleHistory_Click(object sender, RoutedEventArgs e) => Main.NavigateTo<SaleHistoryPage>();
    private void BtnSettings_Click(object sender, RoutedEventArgs e) => Main.NavigateTo<SettingsPage>();
}