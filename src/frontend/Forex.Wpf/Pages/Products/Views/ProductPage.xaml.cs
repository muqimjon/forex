namespace Forex.Wpf.Pages.Products;

using CommunityToolkit.Mvvm.Messaging;
using Forex.Wpf.Common.Interfaces;
using Forex.Wpf.Common.Messages;
using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Pages.Products.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public partial class ProductPage : Page
{
    private ProductPageViewModel vm;
    private INavigationService navigation;

    public ProductPage()
    {
        InitializeComponent();
        vm = App.AppHost!.Services.GetRequiredService<ProductPageViewModel>();
        navigation = App.AppHost!.Services.GetRequiredService<INavigationService>();
        DataContext = vm;

        WeakReferenceMessenger.Default.Register<FocusControlMessage>(this, (r, m) =>
        {
            OnFocusRequestReceived(m.ControlName);
        });

        WeakReferenceMessenger.Default.Register<ScrollToDateMessage>(this, (r, m) =>
        {
            dataGrid.ScrollIntoView(m.Item);
        });

        Loaded += ProductPage_Loaded;
    }

    private void ProductPage_Loaded(object sender, RoutedEventArgs e)
    {
        this.ResizeWindow(1300, 700);
        RegisterFocusNavigation();
        RegisterGlobalShortcuts();
        SetupFilterProductComboBox();
        SetupScanBox();
    }

    private void SetupScanBox()
    {
        var box = tbxScan.input;
        if (box is null) return;

        box.PreviewKeyDown += (_, e) =>
        {
            if (e.Key != Key.Enter) return;
            e.Handled = true;

            var code = box.Text?.Trim();
            box.Clear();

            if (!string.IsNullOrWhiteSpace(code))
                vm.ScanToCartCommand.Execute(code);

            // Batch: fokus skanerda qoladi — ketma-ket tez skaner qilish uchun.
        };
    }

    private void SetupFilterProductComboBox()
    {
        var combo = filterProductCombo.InternalComboBox;
        combo.StaysOpenOnEdit = true;

        combo.GotFocus += (_, _) =>
        {
            vm.ApplyFilterProductSearch(null);
            combo.IsDropDownOpen = true;
        };

        void setupEditBox()
        {
            if (combo.Template?.FindName("PART_EditableTextBox", combo) is not TextBox editBox) return;

            bool userTyping = false;

            editBox.PreviewKeyDown += (_, e) =>
                userTyping = e.Key is not (Key.Down or Key.Up or Key.Enter or Key.Escape or Key.Tab or Key.Left or Key.Right);

            editBox.TextChanged += (_, _) =>
            {
                if (!userTyping) return;
                userTyping = false;
                vm.ApplyFilterProductSearch(editBox.Text?.Trim());
                combo.IsDropDownOpen = true;
            };
        }

        if (combo.IsLoaded) setupEditBox();
        else combo.Loaded += (_, _) => setupEditBox();
    }

    private void RegisterFocusNavigation()
    {
        FocusNavigator.RegisterElements([
            date.input,
            productCombo.combo,
            tbxCode.input,
            tbxName.input,
            cbxProductionOrigin.combo,
            cbxProductType.combo,
            tbxBundle.input,
            tbxBundleItemCount.input,
            tbxQuantity.input,
            tbxCostPrice.input,
            btnAdd,
            btnCancel
        ]);

        FocusNavigator.SetFocusRedirect(btnAdd, productCombo.combo);
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
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            navigation.NavigateTo(new HomePage());
    }

    private void OnFocusRequestReceived(string controlName)
    {
        if (controlName == "ProductCode")
            FocusNavigator.FocusElement(productCombo.combo);
        else if (controlName == "ProductType")
            FocusNavigator.FocusElement(cbxProductType.combo);
    }
}
