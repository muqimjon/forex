namespace Forex.Wpf.Pages.Sales;

using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Pages.Sales.ViewModels;
using Forex.Wpf.Pages.Sales.Views;
using Forex.Wpf.ViewModels;
using Forex.Wpf.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public partial class SalePage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;
    private SalePageViewModel vm;

    public SalePage()
    {
        InitializeComponent();
        vm = App.AppHost!.Services.GetRequiredService<SalePageViewModel>();
        DataContext = vm;

        Loaded += Page_Loaded;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        RegisterFocusNavigation();
        RegisterGlobalShortcuts();
    }

    private void RegisterFocusNavigation()
    {
        // pass
    }

    private void RegisterGlobalShortcuts()
    {
        btnBack.RegisterShortcut(Key.Escape);
        btnAdd.RegisterShortcut(Key.Add);
        btnFilter.RegisterShortcut(Key.F, ModifierKeys.Control);
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo(new HomePage());
    }

    private void BtnAddSale_Click(object sender, RoutedEventArgs e)
    {
        // Yangi savdo qo'shish - bo'sh sahifa
        Main.NavigateTo(new AddSalePage());
    }
    private async void BtnPrintSale_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not SaleViewModel sale)
            return;

        // 1. ServiceProvider'dan VM'ni olamiz
        var printVm = App.AppHost!.Services.GetRequiredService<AddSalePageViewModel>();

        // 2. Savdo ma'lumotlarini (SaleItems va barcha detallari bilan) yuklaymiz
        await printVm.LoadSaleForEditAsync(sale.Id);

        // 3. Metodni to'g'ridan-to'g'ri chaqiramiz (chunki u endi public)
        printVm.ShowPrintPreview();
    }
    private async void BtnEditSale_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not SaleViewModel sale)
            return;

        var addSalePage = new AddSalePage();
        var addSaleVm = App.AppHost!.Services.GetRequiredService<AddSalePageViewModel>();
        addSalePage.DataContext = addSaleVm;

        await addSaleVm.LoadSaleForEditAsync(sale.Id);

        Main.NavigateTo(addSalePage);
    }
}