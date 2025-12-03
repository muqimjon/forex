namespace Forex.Wpf.Pages.Products.Views;

using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Pages.Products.ViewModels;
using Forex.Wpf.Resources.UserControls;
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

        // LostFocus event'larini ulash
        cbxProductCode.LostFocus += CbxProductCode_LostFocus;
        cbxProductName.LostFocus += CbxProductName_LostFocus;
        cbxRazmerType.LostFocus += CbxRazmerType_LostFocus;

        // Add button click'dan keyin fokusni qaytarish uchun event ulash
        addButton.Click += AddButton_Click;
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        // Add komandasi bajarilgandan keyin fokusni qaytarish
        Dispatcher.BeginInvoke(new Action(() =>
        {
            cbxProductCode.comboBox.Focus();
        }), System.Windows.Threading.DispatcherPriority.Input);
    }

    private async void CbxProductCode_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is FloatingComboBox comboBox)
        {
            string enteredText = comboBox.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(enteredText))
            {
                await vm.ValidateProductCodeAsync(enteredText);
            }
        }
    }

    private void CbxProductName_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is FloatingComboBox comboBox)
        {
            string enteredText = comboBox.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(enteredText) && vm.CurrentProductEntry.Product is not null)
            {
                vm.CurrentProductEntry.Product.Name = enteredText;
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
                vm.ValidateProductType(enteredText);
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

    private async void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        await vm.Edit();
    }
}