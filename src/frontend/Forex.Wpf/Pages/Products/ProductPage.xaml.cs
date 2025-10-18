namespace Forex.Wpf.Pages.Products;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for ProductPage.xaml
/// </summary>
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

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await vm.InitializeAsync();
    }


}
