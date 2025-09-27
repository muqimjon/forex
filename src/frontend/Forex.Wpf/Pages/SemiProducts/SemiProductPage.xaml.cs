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

    public SemiProductPage()
    {
        InitializeComponent();

        var vm = new SemiProductPageViewModel();
        vm.Seeding();
        DataContext = vm;

        FocusNavigator.AttachEnterNavigation([
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

            var vm = (SemiProductPageViewModel)DataContext;
            if (combo.DataContext is not ProductTypeViewModel row)
                return;

            var existing = vm.Products.FirstOrDefault(p => p.Code.ToString() == text);

            if (existing is not null)
                row.Product = existing;
            else
            {
                if (int.TryParse(text, out int newCode))
                {
                    var newProduct = new ProductViewModel { Code = newCode };
                    vm.Products.Add(newProduct);
                    row.Product = newProduct;
                }
            }

            combo.SelectedItem = row.Product;
        }
    }
}
