namespace Forex.Wpf.Pages.Products.Views;

using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Pages.Products.ViewModels;
using Forex.Wpf.Resources.UserControls;
using Forex.Wpf.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        cbxProductType.LostFocus += CbxRazmerType_LostFocus;

        Loaded += ProductEntryPage_Loaded;
    }

    private void ProductEntryPage_Loaded(object sender, RoutedEventArgs e)
    {
        RegisterFocusNavigation();
        RegisterGlobalShortcuts();
    }

    private void RegisterFocusNavigation()
    {
        List<UIElement> focusElements =
        [
            date.text,
            cbxProductCode.combobox,
            cbxProductName.combobox,
            cbxProductionOrigin.combobox,
            cbxProductType.combobox,
            tbxBundle.inputBox,
            tbxBundleItemCount.inputBox,
            tbxQuantity.inputBox,
            tbxCostPrice.inputBox,
            btnAdd
        ];

        FocusNavigator.RegisterElements(focusElements);
        FocusNavigator.SetFocusRedirect(btnAdd, cbxProductCode.combobox);
    }

    private void RegisterGlobalShortcuts()
    {
        ShortcutAttacher.RegisterShortcut(
            key: Key.Enter,
            modifiers: ModifierKeys.Control,
            targetButton: btnSubmit
        );

        ShortcutAttacher.RegisterShortcut(
            key: Key.Escape,
            targetButton: btnBack
        );
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