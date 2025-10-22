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
using System.Windows;

public partial class ProductPageViewModel(ForexClient Client, IMapper Mapper) : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<UserViewModel> employees = [];
    [ObservableProperty] private ObservableCollection<UserViewModel> availableEmployees = [];
    private UserViewModel? selectedEmployee;

    [ObservableProperty] private ObservableCollection<ProductViewModel> availableProducts = [];
    [ObservableProperty] private ObservableCollection<ProductViewModel> products = [];
    [ObservableProperty] private ObservableCollection<ProductViewModel> filteredProducts = [];
    [ObservableProperty] private string productName = string.Empty;

    [ObservableProperty] private ObservableCollection<ProductTypeViewModel> productTypes = [];


    private ProductViewModel? selectedProduct;
    [ObservableProperty] private Visibility detailTextVisibility = Visibility.Collapsed;

    public ProductViewModel? SelectedProduct
    {
        get => selectedProduct;
        set
        {
            if (SetProperty(ref selectedProduct, value))
            {
                ProductName = selectedProduct?.Name ?? string.Empty;
                DetailTextVisibility =
                    string.IsNullOrEmpty(selectedProduct?.Name)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }
    }

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
                ["unitMeasure"] = ["include"],
                ["productTypes"] = ["include:productTypeItems.semiProduct.unitMeasure"]
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
        if (SelectedEmployee == null || string.IsNullOrEmpty(SelectedEmployee.Name)) 
        {
            WarningMessage = "Hodim tanla";
            return;
        }
        var product = new ProductViewModel();
        Products.Add(product);
        SelectedEmployee.PreparedProducts.Add(product);
        FilteredProducts.Add(product);
    }

    [RelayCommand]
    private void DeleteUser(UserViewModel user)
    {
        if (user == null)
            return;

        // 🔹 Avval hodimga tegishli mahsulotlarni o‘chir
        if (user.PreparedProducts != null && user.PreparedProducts.Any())
        {
            foreach (var product in user.PreparedProducts.ToList())
            {
                Products.Remove(product); // umumiy ro‘yxatdan o‘chir
            }
        }
        // 🔹 Keyin hodimni o‘chir
        Employees.Remove(user);

        // 🔹 Agar o‘chirilgan hodim tanlangan bo‘lsa, uni tozalaymiz
        if (user == SelectedEmployee)
            SelectedEmployee = null!;

        // 🔹 Filter yangilansin
        UpdateProducts();
    }

    [RelayCommand]
    private void DeleteProduct(ProductViewModel product)
    {
        if (product == null || SelectedEmployee == null)
            return;
        SelectedEmployee.PreparedProducts.Remove(product);
        Products.Remove(product);
        FilteredProducts.Remove(product);
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
