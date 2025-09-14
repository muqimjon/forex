namespace Forex.Wpf.Pages.SemiProducts;

using Forex.ClientService;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

/// <summary>
/// Interaction logic for SemiProductPage.xaml
/// </summary>
public partial class SemiProductPage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;
    private readonly ForexClient client;
    public SemiProductPage(ForexClient client)
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
