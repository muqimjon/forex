namespace Forex.Wpf.Pages.Products.Views;

using Forex.Wpf.Pages.Home;
using Forex.Wpf.Pages.Products.ViewModels;
using Forex.Wpf.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for ProductEntryPage.xaml
/// </summary>
public partial class ProductEntryPage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;
    private ProductEntryPageViewModel vm;

    public ProductEntryPage()
    {
        InitializeComponent();
        vm = App.AppHost!.Services.GetRequiredService<ProductEntryPageViewModel>();
        DataContext = vm;
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo(new HomePage());
    }
}
