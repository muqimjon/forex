namespace Forex.Wpf.Pages.Sales;

using Forex.Wpf.Pages.Home;
using Forex.Wpf.Pages.Sales.ViewModels;
using Forex.Wpf.ViewModels;
using Forex.Wpf.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for SalePage.xaml
/// </summary>
public partial class SalePage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;
    private readonly SalePageViewModel vm;

    public SalePage()
    {
        InitializeComponent();
        vm = App.AppHost!.Services.GetRequiredService<SalePageViewModel>();
        DataContext = vm;
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo(new HomePage());
    }

    private async void CustomerComboBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not ComboBox comboBox || string.IsNullOrWhiteSpace(comboBox.Text))
            return;

        var input = comboBox.Text.Trim();

        var existing = vm.AvailableCustomers.FirstOrDefault(c =>
            c.Name.Equals(input, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            vm.Customer = existing;
            return;
        }

        var confirm = MessageBox.Show(
            $"Mijoz '{input}' topilmadi. Yangi mijoz yaratilishini istaysizmi?",
            "Yangi mijoz",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm == MessageBoxResult.Yes)
        {
            var newCustomer = CreateCustomerAsync(input);
            if (newCustomer is not null)
            {
                vm.Customer = newCustomer;

                vm.AvailableCustomers.Add(vm.Customer);

            }
        }
    }

    private void ComboBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is ComboBox comboBox)
        {
            comboBox.IsDropDownOpen = true;
        }
    }

    private UserViewModel CreateCustomerAsync(string name)
    {
        var dialog = new UserWindow();
        dialog.txtName.Text = name;

        var result = dialog.ShowDialog();
        if (result != true)
            return null!;


        return dialog.user!;
    }


}