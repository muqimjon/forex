namespace Forex.Wpf.Pages.Products;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Pages.Products.ViewModels;
using Forex.Wpf.Windows;
using System.Collections.Specialized;
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
        vm = new ProductPageViewModel(App.Client);
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

    /// <summary>
    /// Foydalanuvchi DataGrid orqali yangi qator qo‘shganda ishlaydi
    /// </summary>
    private void Products_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (var item in e.NewItems!)
            {
                var newProduct = item as ProductViewModel;

                if (vm.SelectedEmployee != null)
                {
                    vm.SelectedEmployee.EmployeeProducts.Add(newProduct!);
                    vm.Products.Add(newProduct!);
                    vm.UpdateProducts();
                }
                else
                {
                    vm.WarningMessage = "Avval hodimni tanlang!";
                    vm.Products.Remove(newProduct!);
                    break;
                }
            }
        }
    }
}
