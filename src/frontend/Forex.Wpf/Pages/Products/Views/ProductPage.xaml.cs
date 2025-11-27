namespace Forex.Wpf.Pages.Products;

using Forex.Wpf.Pages.Home;
using Forex.Wpf.Pages.Products.ViewModels;
using Forex.Wpf.Resources.UserControls;
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

        // LostFocus event'larini ulash - FloatingComboBox darajasida
        cbxProductCode.LostFocus += CbxProductCode_LostFocus;
        cbxProductName.LostFocus += CbxProductName_LostFocus;
        cbxRazmerType.LostFocus += CbxRazmerType_LostFocus;
    }

    private async void CbxProductCode_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is FloatingComboBox comboBox)
        {
            string enteredText = comboBox.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(enteredText))
            {
                //await vm.ValidateProductCodeAsync(enteredText);
            }
        }
    }

    private void CbxProductName_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is FloatingComboBox comboBox)
        {

            string enteredText = comboBox.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(enteredText))
            {
                //vm.CurrentProductEntry.Product!.Name = enteredText;
            }
        }
    }

    private void CbxRazmerType_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is FloatingComboBox comboBox)
        {
            string enteredText = comboBox.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(enteredText))
            {
                //vm.ValidateProductType(enteredText);
            }
        }
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
        //vm.Edit();
    }
}