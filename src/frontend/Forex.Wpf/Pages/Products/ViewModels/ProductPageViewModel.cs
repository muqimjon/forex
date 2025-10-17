using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Enums;
using Forex.ClientService.Models.Responses;
using Forex.Wpf.Common.Extensions;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.Pages.Products.ViewModels;
using System.Collections.ObjectModel;

public partial class ProductPageViewModel(ForexClient _client) : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<UserResponse> employees = [];
    
    [ObservableProperty] private ObservableCollection<UserViewModel> users = [];
    private UserViewModel? selectedEmployee;


    [ObservableProperty] private ObservableCollection<ProductViewModel> products = [];
    [ObservableProperty] private ObservableCollection<ProductViewModel> filteredProducts = [];



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

        Users.Add(new UserViewModel { });

    }

    [RelayCommand]
    private void AddEmployee()
    {
        Users.Add(new UserViewModel { });
    }

    [RelayCommand]
    private void ShowAllProducts()
    {
        SelectedEmployee = null!;
    }

    [RelayCommand]
    private void AddProduct()
    {
        if (selectedEmployee == null)
        {
            WarningMessage = "Hodim tanla";
            return;
        }

        ProductViewModel product = new();
        if (!string.IsNullOrEmpty(SelectedEmployee.Name))
        {
            SelectedEmployee.EmployeeProducts.Add(product);
            Products.Add(product);
            UpdateProducts();
        }
        else
        {
            WarningMessage = "Hodim tanla";
        }
    }

    public UserViewModel SelectedEmployee 
    { 
        get => selectedEmployee!;
        set 
        {
            if (value != selectedEmployee)
            {
                selectedEmployee = value;
                UpdateProducts();
            }
        } 
    } 

    public void UpdateProducts()
    {
        FilteredProducts.Clear();

        if (SelectedEmployee is null)
        {
            FilteredProducts.AddRange(Products);
        }
        else
        {
            FilteredProducts.AddRange(SelectedEmployee.EmployeeProducts);
        }
    }

}
