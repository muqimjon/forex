namespace Forex.Wpf.Pages.ShopCashes;

using Forex.Wpf.Common.Services;
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
    public ShopCashPage()
    {
        InitializeComponent();
        FocusNavigator.AttachEnterNavigation(
        [
            cbxIncome,
            cbxValyutaType,
            tbSum,
            tbDescription,
            btnRegister
        ]);

        FocusNavigator.AttachEnterNavigation(
        [
            startDate.dateTextBox,
            endDate.dateTextBox,
            btnShow
        ]);
        supplyDate.SelectedDate = DateTime.Now;
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo(new HomePage());
    }

}
