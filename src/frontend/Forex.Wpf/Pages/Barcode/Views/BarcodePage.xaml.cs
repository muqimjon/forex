namespace Forex.Wpf.Pages.Barcode.Views;

using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Barcode.ViewModels;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Windows;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

public partial class BarcodePage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;
    private readonly BarcodePageViewModel vm;

    public BarcodePage()
    {
        InitializeComponent();
        vm = App.AppHost!.Services.GetRequiredService<BarcodePageViewModel>();
        DataContext = vm;
        vm.PropertyChanged += OnViewModelPropertyChanged;
        Loaded += Page_Loaded;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        this.ResizeWindow(1280, 800);
        SetupScanBox();
        SetupKeyboardFlow();
        FocusScan();
    }

    private void SetupKeyboardFlow()
    {
        if (tbxSearch.input is { } scan)
            scan.PreviewKeyDown += (_, e) =>
            {
                if (e.Key == Key.Tab && (Keyboard.Modifiers & ModifierKeys.Shift) == 0 && productList.Items.Count > 0)
                {
                    e.Handled = true;
                    FocusListItem(productList);
                }
            };

        productList.PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter && vm.DetailProduct is not null)
            {
                e.Handled = true;
                FocusListItem(razmerList);
            }
        };

        razmerList.PreviewKeyDown += (_, e) =>
        {
            if (e.Key != Key.Enter) return;
            e.Handled = true;
            FocusListItem(unitList);
        };

        unitList.PreviewKeyDown += (_, e) =>
        {
            if (e.Key != Key.Enter) return;
            e.Handled = true;
            copiesBoxSide.Focus();
            copiesBoxSide.SelectAll();
        };

        copiesBoxSide.PreviewKeyDown += (_, e) =>
        {
            if (e.Key != Key.Enter) return;
            e.Handled = true;
            NormalizeCopies(copiesBoxSide);
            vm.ShowCommand.Execute(null);
        };

        PreviewKeyDown += Page_PreviewKeyDown;
    }

    private void Page_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Control) != 0
            && !vm.IsPopupOpen && vm.DetailProduct is not null)
        {
            e.Handled = true;
            vm.ShowCommand.Execute(null);
        }
    }

    private static void FocusListItem(ListBox list)
    {
        if (list.Items.Count == 0) return;
        if (list.SelectedIndex < 0) list.SelectedIndex = 0;
        var index = list.SelectedIndex;

        list.Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
        {
            list.UpdateLayout();
            if (list.ItemContainerGenerator.ContainerFromIndex(index) is ListBoxItem item)
                item.Focus();
            else
                list.Focus();
        });
    }

    private void SetupScanBox()
    {
        if (tbxSearch.input is { } scan)
            scan.PreviewKeyDown += (_, e) =>
            {
                if (e.Key != Key.Enter) return;

                var code = scan.Text?.Trim();
                if (string.IsNullOrWhiteSpace(code) || BarcodeResolver.Resolve(vm.AvailableProducts, code) is null)
                    return;

                e.Handled = true;
                scan.Clear();
                vm.ScanCommand.Execute(code);
            };

        copiesBox.PreviewKeyDown += (_, e) =>
        {
            if (e.Key != Key.Enter) return;
            e.Handled = true;

            var text = copiesBox.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(text) && BarcodeResolver.Resolve(vm.AvailableProducts, text) is not null)
            {
                vm.ScanCommand.Execute(text);
                copiesBox.Text = vm.Copies.ToString();
                copiesBox.SelectAll();
                return;
            }

            NormalizeCopies(copiesBox);
            vm.PrintCommand.Execute(null);
        };

        copiesBox.LostFocus += (_, _) => NormalizeCopies(copiesBox);
        copiesBoxSide.LostFocus += (_, _) => NormalizeCopies(copiesBoxSide);
    }

    private void NormalizeCopies(TextBox box)
    {
        var n = int.TryParse(box.Text, out var c) ? Math.Clamp(c, 1, 500) : 1;
        vm.Copies = n;
        box.Text = n.ToString();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(BarcodePageViewModel.IsPopupOpen)) return;

        if (vm.IsPopupOpen)
            Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
            {
                copiesBox.Focus();
                copiesBox.SelectAll();
            });
        else
            FocusScan();
    }

    private void FocusScan() =>
        Dispatcher.BeginInvoke(DispatcherPriority.Input, () => tbxSearch.input?.Focus());

    private void ClosePopup_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource == sender && vm.IsPopupOpen)
        {
            vm.ClosePopupCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo(new HomePage());
    }
}
