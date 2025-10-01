namespace Forex.Wpf.Pages.ShopCashes;

using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

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
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo(new HomePage());
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (decimal.TryParse(tbSum.Text, out decimal value))
        {
            tbSum.Text = value.ToString("N2"); // 2 xona kasr format
        }
    }

}
