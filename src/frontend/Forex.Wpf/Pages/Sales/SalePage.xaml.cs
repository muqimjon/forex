namespace Forex.Wpf.Pages.Sales;

using Forex.Wpf.Pages.Home;
using Forex.Wpf.Pages.Sales.ViewModels;
using Forex.Wpf.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

/// <summary>
/// Interaction logic for SalePage.xaml
/// </summary>
public partial class SalePage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;
    private SaleViewModel vm;

    public SalePage()
    {
        InitializeComponent();
        vm = App.AppHost!.Services.GetRequiredService<SaleViewModel>();
        DataContext = vm;
        _ = vm.LoadUsersAsync();
        btnBack.Click += BtnBack_Click;
        vm.RequestNewCustomer += Vm_RequestNewCustomer;
        supplyDate.SelectedDate = DateTime.Now;
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo(new HomePage());
    }

    private async void Vm_RequestNewCustomer(object? sender, string name)
    {
        var result = MessageBox.Show(
        $"“{name}” nomli mijoz topilmadi.\nYangi mijoz qo‘shmoqchimisiz?",
        "Yangi mijoz",
        MessageBoxButton.YesNo,
        MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            var userWindow = new UserWindow();
            userWindow.Owner = Application.Current.MainWindow;
            bool? dialogResult = userWindow.ShowDialog();

            if (dialogResult == true && sender is SaleViewModel vm)
            {
                await vm.LoadUsersAsync();
                vm.SelectedCustomer = vm.Customers
        .FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
        }
    }

    private void ComboBox_KeyDown(object sender, KeyEventArgs e)
    {
        if ((e.Key == Key.Enter || e.Key == Key.Tab) && DataContext is SaleViewModel vm)
        {
            string text = ((ComboBox)sender).Text?.Trim() ?? "";
            vm.CheckCustomerNameCommand.Execute(text);
        }
    }
    private void ComboBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is ComboBox comboBox)
        {
            comboBox.IsDropDownOpen = true;
        }
    }
}
