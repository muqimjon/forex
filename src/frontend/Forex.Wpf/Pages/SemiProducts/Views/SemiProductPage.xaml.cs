namespace Forex.Wpf.Pages.SemiProducts.Views;

using Forex.Wpf.Pages.Home;
using Forex.Wpf.Pages.SemiProducts.ViewModels;
using Forex.Wpf.ViewModels;
using Forex.Wpf.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public partial class SemiProductPage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;

    public SemiProductPage()
    {
        InitializeComponent();
        DataContext = App.AppHost!.Services.GetRequiredService<SemiProductPageViewModel>();
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo(new HomePage());
    }

    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is ScrollViewer scroll)
        {
            scroll.ScrollToVerticalOffset(scroll.VerticalOffset - e.Delta / 3.0);
            e.Handled = true;
        }
    }

    private void DeleteProduct_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ProductViewModel product)
        {
            var vm = DataContext as SemiProductPageViewModel;
            if (vm?.Products.Contains(product) == true)
            {
                vm.Products.Remove(product);
            }
        }
    }

    private void DeleteProductType_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ProductTypeViewModel productType)
        {
            // Find parent Product
            var vm = DataContext as SemiProductPageViewModel;
            if (vm?.Products != null)
            {
                foreach (var product in vm.Products)
                {
                    if (product.ProductTypes.Contains(productType))
                    {
                        product.ProductTypes.Remove(productType);
                        break;
                    }
                }
            }
        }
    }

    private void DeleteProductTypeItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ProductTypeItemViewModel item)
        {
            // Find parent ProductType
            var vm = DataContext as SemiProductPageViewModel;
            if (vm?.Products != null)
            {
                foreach (var product in vm.Products)
                {
                    foreach (var productType in product.ProductTypes)
                    {
                        if (productType.ProductTypeItems.Contains(item))
                        {
                            productType.ProductTypeItems.Remove(item);
                            return;
                        }
                    }
                }
            }
        }
    }
}