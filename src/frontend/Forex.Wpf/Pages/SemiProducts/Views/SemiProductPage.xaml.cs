namespace Forex.Wpf.Pages.SemiProducts.Views;

using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Pages.SemiProducts.ViewModels;
using Forex.Wpf.ViewModels;
using Forex.Wpf.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

public partial class SemiProductPage : Page
{
    private readonly SemiProductPageViewModel vm;

    public SemiProductPage()
    {
        InitializeComponent();

        vm = App.AppHost!.Services.GetRequiredService<SemiProductPageViewModel>();
        DataContext = vm;

        FocusNavigator.AttachEnterNavigation(
            [
                btnAddProduct,
                btnShowAll,
                tbAddSemiProduct,
                dpDate,
                tbCostPrice,
                tbCostDelivery,
                tbTransferFee,
                tbTotalAmount,
                cbCurrency,
                cbManufactory,
                cbSupplierName,
                tbSupplierPhone,
                tbSupplierEmail,
                tbSupplierAddress,
                tgbViaMiddleman,
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
            (Application.Current.MainWindow as MainWindow)?.NavigateTo(new HomePage());
    }

    private void ComboBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not ComboBox combo || combo.DataContext is not ProductTypeViewModel type)
            return;

        string? text = combo.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text) || !int.TryParse(text, out int newCode))
            return;

        var existing = vm.Products.FirstOrDefault(p => p.Code == newCode);

        if (existing is not null)
        {
            if (type.Product == existing)
                return;

            if (type.Product.ProductTypes.Count < 2 && type.Product.ProductTypes.Contains(type))
                vm.Products.Remove(type.Product);

            type.Product = existing;
        }
        else
        {
            if (type.Product.ProductTypes.Count < 2 && type.Product.ProductTypes.Contains(type))
                vm.Products.Remove(type.Product);

            type.Product ??= new ProductViewModel();
            type.Product.Code = newCode;
            vm.Products.Add(type.Product);
            vm.ExistProducts.Add(type.Product);
        }

        combo.SelectedItem = type.Product;
    }
}
