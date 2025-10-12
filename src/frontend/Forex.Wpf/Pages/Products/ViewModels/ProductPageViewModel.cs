using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Enums;
using Forex.ClientService.Models.Products;
using Forex.ClientService.Models.Users;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.Pages.Products.ViewModels;
using System.Collections.ObjectModel;

public partial class ProductPageViewModel(ForexClient _client) : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<UserViewModel> users = new();
    [ObservableProperty] private ObservableCollection<UserResponse> employees = new();

    [ObservableProperty] private ObservableCollection<ProductViewModel> products = new();
    [ObservableProperty] private ObservableCollection<ProductResponse> mahsulotlar = new();

    public async Task LoadEmployeesAsync()
    {
        try
        {
            var response = await _client.Users.GetAll();
            if (response.IsSuccess && response.Data != null)
            {
                var hodimlar = response.Data
                    .Where(u => u.Role == Role.Hodim)
                    .ToList();

                Employees = new ObservableCollection<UserResponse>(hodimlar);
            }
            else
            {
                WarningMessage = "Hodimlarni yuklashda xatolik.";
            }
        }
        catch (Exception ex)
        {
            WarningMessage = $"Server bilan aloqa yo'q: {ex.Message}";
        }
    }


    public async Task InitializeAsync()
    {
        await LoadEmployeesAsync();

        Users.Add(new UserViewModel
        {
            Employees = Employees
        });

        Products.Add(new ProductViewModel
        {

        });
    }

    [RelayCommand]
    private void AddEmployee()
    {
        Users.Add(new UserViewModel
        {
            Employees = Employees
        });
    }

    [RelayCommand]
    private void AddProduct() 
    {
        Products.Add(new ProductViewModel { } );
    }
}
