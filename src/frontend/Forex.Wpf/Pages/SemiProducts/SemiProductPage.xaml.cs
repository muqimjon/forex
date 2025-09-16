namespace Forex.Wpf.Pages.SemiProducts;

using Forex.ClientService;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Services;
using Forex.Wpf.Windows;
using System.Collections.ObjectModel;
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

        DataContext = this;

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

    #region Seeding..

    public class SemiProduct
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Measure { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal CostDelivery { get; set; }
        public decimal TransferFee { get; set; }
        public decimal TotalCost => CostPrice + CostDelivery + TransferFee;
    }
    public ObservableCollection<SemiProduct> SemiProducts { get; set; } = new()
    {
        new SemiProduct
        {
            Name = "Plastik qoplama",
            Code = "PL-001",
            Measure = "m²",
            Quantity = 120,
            CostPrice = 1500,
            CostDelivery = 200,
            TransferFee = 50
        },
        new SemiProduct
        {
            Name = "Metall karkas",
            Code = "MT-002",
            Measure = "kg",
            Quantity = 75,
            CostPrice = 3200,
            CostDelivery = 300,
            TransferFee = 100
        },
        new SemiProduct
        {
            Name = "Yelimli qatlam",
            Code = "YL-003",
            Measure = "litr",
            Quantity = 40,
            CostPrice = 800,
            CostDelivery = 150,
            TransferFee = 25
        },
        new SemiProduct
        {
            Name = "Shisha panel",
            Code = "SH-004",
            Measure = "dona",
            Quantity = 20,
            CostPrice = 5000,
            CostDelivery = 500,
            TransferFee = 200
        }
    };

    #endregion

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