namespace Forex.Wpf.Pages.SemiProducts;

using Forex.ClientService;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Services;
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

        FocusNavigator.AttachEnterNavigation([
            cbSender,
            cbOrg,
            txContainerCount,
            txTransferFeePerContainer,
            txDeliveryPrice,
            txNote,
            fcbName.comboBox,
            fibCode.inputBox,
            fcbMeasure.comboBox,
            fibQuantity.inputBox,
            fibCost.inputBox,
            fibDelivery.inputBox,
            fibTransfer.inputBox,
            ffiPhoto.btnBrowse,
            btnAdd,
            ]);
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (Application.Current.MainWindow is Window mainWindow)
            WindowResizer.AnimateToSize(mainWindow, 810, 580);
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo(new HomePage(client));
    }
}
