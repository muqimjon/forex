namespace Forex.Wpf.Pages.Sales;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Windows;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for SalePage.xaml
/// </summary>
public partial class SalePage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;
    public SalePage()
    {
        InitializeComponent();
        btnBack.Click += BtnBack_Click;
        supplyDate.SelectedDate = DateTime.Now;
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo(new HomePage());
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {

    }
}
