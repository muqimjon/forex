namespace Forex.Wpf.Pages.Products.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.Wpf.Common.Extensions;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using System.Collections.ObjectModel;

public partial class ProductPageViewModel(ForexClient Client, IMapper Mapper) : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<UserViewModel> employees = [];
    [ObservableProperty] private ObservableCollection<UserViewModel> availableEmployees = [];
    private UserViewModel? selectedEmployee;

    [ObservableProperty] private ObservableCollection<ProductViewModel> availableProducts = [];
    [ObservableProperty] private ObservableCollection<ProductViewModel> products = [];
    [ObservableProperty] private ObservableCollection<ProductViewModel> filteredProducts = [];

    public async Task InitializeAsync()
    {
        await LoadEmployeesAsync();
        await LoadProductsAsync();

        Employees.Add(new UserViewModel { });
        UpdateProducts();
    }


    public async Task LoadEmployeesAsync()
    {
        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["role"] = ["hodim"],
                ["accounts"] = ["include:currency"]
            }
        };

        var response = await Client.Users.Filter(request)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
            AvailableEmployees = Mapper.Map<ObservableCollection<UserViewModel>>(response.Data);
        else
            WarningMessage = response.Message ?? "Hodimlarni yuklashda xatolik.";
    }


    public async Task LoadProductsAsync()
    {

        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["productTypes"] = ["include"]
            }
        };

        var response = await Client.Products.Filter(request)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess && response.Data != null)
        {
            AvailableProducts = Mapper.Map<ObservableCollection<ProductViewModel>>(response.Data);
        }
        else
        {
            WarningMessage = "Mahsulotlarni yuklashda xatolik.";
        }

    }

    [RelayCommand]
    private void AddEmployee()
    {
        Employees.Add(new UserViewModel { });
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

        FilteredProducts.Add(new ProductViewModel());
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

    private void UpdateProducts()
    {
        FilteredProducts.Clear();

        if (SelectedEmployee is null)
        {
            FilteredProducts.AddRange(Products);
        }
        else
        {
            FilteredProducts.AddRange(SelectedEmployee.PreparedProducts);
        }
    }
}
