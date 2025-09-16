namespace Forex.Wpf.Pages.SaleHistories;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Windows;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for SaleHistoryPage.xaml
/// </summary>
public partial class SaleHistoryPage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;

    public SaleHistoryPage()
    {
        InitializeComponent();
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo(new HomePage());
    }
}
