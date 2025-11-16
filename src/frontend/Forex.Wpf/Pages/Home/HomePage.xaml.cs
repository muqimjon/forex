namespace Forex.Wpf.Pages.Home;
using Forex.ClientService.Services;
using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Auth;
using Forex.Wpf.Pages.Processes;
using Forex.Wpf.Pages.Products;
using Forex.Wpf.Pages.Reports;
using Forex.Wpf.Pages.Sales;
using Forex.Wpf.Pages.SemiProducts.Views;
using Forex.Wpf.Pages.Settings;
using Forex.Wpf.Pages.ShopCashes;
using Forex.Wpf.Pages.Users;
using Forex.Wpf.Windows;
using Forex.Wpf.Windows.OverdueAccountsWindow;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for HomePage.xaml
/// </summary>
public partial class HomePage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;
    public HomePage()
    {
        InitializeComponent();
        DataContext = AuthStore.Instance;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (Application.Current.MainWindow is Window mainWindow)
            WindowResizer.AnimateToSize(mainWindow, 810, 580);

        //var userAccountService = App.AppHost!.Services.GetRequiredService<IApiUserAccount>();
        //var allAccountsResult = await userAccountService.GetAllAsync();

        //if (allAccountsResult.Data == null)
        //{
        //    if (Application.Current.MainWindow is Window defaultWindow)
        //        defaultWindow.Background = Brushes.Green;
        //    return;
        //}

        //var filteredData = allAccountsResult.Data
        //   //.Where(a =>
        //   //    a.DueDate.HasValue &&
        //   //    a.DueDate.Value.Date <= DateTime.Today)
        //   .ToList();

        //if (filteredData.Count > 0)
        //{
        //    tbWarning.Foreground = Brushes.Red;
        //}
        //else
        //{
        //    tbWarning.Foreground = Brushes.Green;
        //}
    }
    private void BtnUser_Click(object sender, RoutedEventArgs e)
        => Main.NavigateTo(new UserPage());

    private void BtnProduct_Click(object sender, RoutedEventArgs e)
        => Main.NavigateTo(new ProductPage());

    private void BtnCash_Click(object sender, RoutedEventArgs e)
        => Main.NavigateTo(new ShopCashPage());

    private void BtnSale_Click(object sender, RoutedEventArgs e)
        => Main.NavigateTo(new SalePage());

    private void BtnSettings_Click(object sender, RoutedEventArgs e)
        => Main.NavigateTo(new SettingsPage());

    private void BtnSemiProduct_Click(object sender, RoutedEventArgs e)
        => Main.NavigateTo(new SemiProductPage());
    private void btnReports_Click(object sender, RoutedEventArgs e)
        => Main.NavigateTo(new ReportsPage());

    private void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        AuthStore.Instance.Logout();
        Main.NavigateTo(new LoginPage());
    }

    private void btnProcess_Click(object sender, RoutedEventArgs e)
          => Main.NavigateTo(new ProcessPage());

    private void BtnOverdue_Click(object sender, RoutedEventArgs e)
    {
        var window = new OverdueAccountsWindow();
        window.ShowDialog();
    }
}
