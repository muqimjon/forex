namespace Forex.Wpf.Pages.Products;

using Forex.Wpf.Pages.Home;
using Forex.Wpf.Windows;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for ProductPage.xaml
/// </summary>
public partial class ProductPage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;
    public ProductPage()
    {
        InitializeComponent();
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo<HomePage>();
    }
}
