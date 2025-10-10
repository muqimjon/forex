namespace Forex.Wpf.Pages.Sales;

using Forex.Wpf.Pages.Home;
using Forex.Wpf.Pages.Sales.ViewModels;
using Forex.Wpf.Windows;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for SalePage.xaml
/// </summary>
public partial class SalePage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;

    private SaleViewModel vm;
    public SalePage()
    {
        InitializeComponent();

        vm = new SaleViewModel(App.Client); // ✅ shart
        DataContext = vm;
        vm.LoadUsersAsync();
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
