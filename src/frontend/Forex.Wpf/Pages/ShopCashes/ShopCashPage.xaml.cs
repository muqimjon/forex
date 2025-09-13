namespace Forex.Wpf.Pages.ShopCashes;

using Forex.ClientService;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Windows;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for ShopCashPage.xaml
/// </summary>
public partial class ShopCashPage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;
    private readonly ForexClient client;
    public ShopCashPage(ForexClient client)
    {
        InitializeComponent();
        this.client = client;
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo(new HomePage(client));
    }
}
