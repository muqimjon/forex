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

    [ObservableProperty] private ObservableCollection<ProductResponse> comboProducts = [];
    [ObservableProperty] private ObservableCollection<ProductViewModel> products = [];
    [ObservableProperty] private ObservableCollection<ProductViewModel> filteredProducts = [];

    [ObservableProperty] private ObservableCollection<ProductTypeResponse> allTypes = [];


    public async Task LoadEmployeesAsync()
    {
        try
        {
            var response = await _client.Users.GetAll();
            if (response.IsSuccess && response.Data != null)
            {
                var hodimlar = response.Data
                    .Where(u => u.Role == UserRole.Hodim)
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
        await LoadProductsAsync();
        await LoadProductTypeAsync();

        Users.Add(new UserViewModel { });
        UpdateProducts();
    }

    public async Task LoadProductsAsync()
    {
        try
        {
            var response = await _client.Products.GetAll();

            if (response.IsSuccess && response.Data != null)
            {
                ComboProducts = new ObservableCollection<ProductResponse>(response.Data);
            }
            else
            {
                WarningMessage = "Mahsulotlarni yuklashda xatolik.";
            }
        }
        catch (Exception ex)
        {
            WarningMessage = $"Server bilan aloqa yo‘q: {ex.Message}";
        }
    }

    public async Task LoadProductTypeAsync()
    {
        try
        {
            var response = await _client.ProductType.GetAll();
            if (response.IsSuccess && response.Data != null)
            {
                AllTypes = new ObservableCollection<ProductTypeResponse>(response.Data);
            }
            else
            {
                WarningMessage = "Mahsulot turlarini yuklashda xatolik.";
            }
        }
        catch (Exception ex)
        {
            WarningMessage = $"Server bilan aloqa yo‘q: {ex.Message}";
        }
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

        // 🔹 Agar typelar hali yuklanmagan bo‘lsa
        if (AllTypes == null || AllTypes.Count == 0)
        {
            WarningMessage = "Mahsulot turlari hali yuklanmagan.";
            return;
        }

        var product = new ProductViewModel
        {
            Parent = this
        };

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
