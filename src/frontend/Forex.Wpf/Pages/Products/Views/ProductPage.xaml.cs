namespace Forex.Wpf.Pages.Products;

using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Pages.Products.ViewModels;
using Forex.Wpf.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public partial class ProductPage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;
    private ProductPageViewModel vm;

    public ProductPage()
    {
        InitializeComponent();
        vm = App.AppHost!.Services.GetRequiredService<ProductPageViewModel>();
        DataContext = vm;

        Loaded += ProductPage_Loaded;
    }

    private void ProductPage_Loaded(object sender, RoutedEventArgs e)
    {
        RegisterFocusNavigation();
        RegisterGlobalShortcuts();
    }

    private void RegisterFocusNavigation()
    {
        FocusNavigator.RegisterElements([
            date.text,
            cbxProductCode.combobox,
            cbxProductName.combobox,
            cbxProductionOrigin.combobox,
            cbxProductType.combobox,
            tbxBundle.inputBox,
            tbxBundleItemCount.inputBox,
            tbxQuantity.inputBox,
            tbxCostPrice.inputBox,
            btnAdd,
            btnCancel
        ]);
        FocusNavigator.SetFocusRedirect(btnAdd, cbxProductCode.combobox);
    }

    private void RegisterGlobalShortcuts()
    {
        ShortcutAttacher.RegisterShortcut(
            targetButton: btnBack,
            key: Key.Escape
        );

        ShortcutAttacher.RegisterShortcut(
            targetButton: btnAdd,
            key: Key.Enter,
            modifiers: ModifierKeys.Control
        );

        ShortcutAttacher.RegisterShortcut(
            targetElement: this,
            key: Key.E,
            modifiers: ModifierKeys.Control,
            targetAction: () => _ = vm.Edit()
        );
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo(new HomePage());
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        _ = vm.Edit();
    }
}