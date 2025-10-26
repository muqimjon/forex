namespace Forex.Wpf.Pages.ShopCashes;

using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Pages.ShopCashes.ViewModels;
using Forex.Wpf.Windows;
using Microsoft.Extensions.DependencyInjection;
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

        DataContext = App.AppHost!.Services.GetRequiredService<PaymentPageViewModel>();

        FocusNavigator.AttachEnterNavigation(
        [
            startDate.dateTextBox,
            endDate.dateTextBox,
            btnShow
        ]);

        operationDate.SelectedDate = DateTime.Now;
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo(new HomePage());
    }
}
