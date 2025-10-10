namespace Forex.Wpf.Pages.SemiProducts;

using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Pages.SemiProducts.ViewModels;
using Forex.Wpf.Windows;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for SemiProductPage.xaml
/// </summary>
public partial class SemiProductPage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;
    private readonly SemiProductPageViewModel vm;
    private int code;

    public SemiProductPage()
    {
        InitializeComponent();

        vm = new SemiProductPageViewModel();
        vm.Seeding();
        DataContext = vm;
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
            Main.NavigateTo(new HomePage());
    }

    private void DataGrid_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SemiProductPageViewModel vm) return;

        vm.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(vm.ShowQuantityColumn))
            {
                QuantityColumn.Visibility = vm.ShowQuantityColumn
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        };
    }

    private void ComboBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is ComboBox combo)
        {
            string? text = combo.Text?.Trim();
            if (string.IsNullOrWhiteSpace(text))
                return;

            if (!int.TryParse(text, out int newCode) || newCode == code)
                return;

            code = newCode;

            if (combo.DataContext is ProductTypeViewModel product)
            {
                var existing = vm.Products.FirstOrDefault(p => p.Code == newCode);

                if (existing is not null)
                    product.Product = existing;
                else
                {
                        var newProduct = new ProductViewModel { Code = newCode };
                        vm.Products.Add(newProduct);
                        product.Product = newProduct;
                }

                combo.SelectedItem = product.Product;
            }
            else if (combo.DataContext is SemiProductViewModel semiProduct)
            {
                var existing = vm.SemiProducts.FirstOrDefault(x => x.Code == newCode);

                if (existing is not null)
                {
                    if (semiProduct.LinkedItem is not null)
                    {
                       semiProduct.LinkedItem.SemiProduct = existing;
                        existing.IsEditing = true;
                    }

                    // Agar eski semiProduct bo‘sh bo‘lsa, uni olib tashlash
                    if (string.IsNullOrWhiteSpace(semiProduct.Name))
                    {
                        vm.SemiProducts.Remove(semiProduct);
                    }

                    vm.UpdateSemiProductsForSelectedProduct();
                }
                else
                {
                    semiProduct.Code = newCode;
                }
            }
        }
    }
}
