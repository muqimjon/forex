namespace Forex.Wpf.Pages.Products;

using Forex.Wpf.Pages.Home;
using Forex.Wpf.Pages.Products.ViewModels;
using Forex.Wpf.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

public partial class ProductPage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;
    private ProductPageViewModel vm;

    public ProductPage()
    {
        InitializeComponent();
        vm = App.AppHost!.Services.GetRequiredService<ProductPageViewModel>();
        DataContext = vm;
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo(new HomePage());
    }

    private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _ = vm.EditCommand.ExecuteAsync(null);
    }
}