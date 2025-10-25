namespace Forex.Wpf.Pages.Products.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Forex.Wpf.Common.Extensions;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using System.Collections.ObjectModel;
using System.ComponentModel;

public partial class ProductPageViewModel(ForexClient Client, IMapper Mapper) : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<UserViewModel> availableEmployees = [];
    [ObservableProperty] private ObservableCollection<UserViewModel> employees = [];
    private UserViewModel? selectedEmployee;

    [ObservableProperty] private ObservableCollection<ProductViewModel> availableProducts = [];

    [ObservableProperty] private ObservableCollection<ProductEntryViewModel> filteredProducts = [];
    [ObservableProperty] private ProductEntryViewModel? selectedProduct;


    #region Commands

    [RelayCommand]
    private void AddEmployee()
    {
        UserViewModel employee = new();
        Employees.Add(employee);
    }

    [RelayCommand]
    private void DeleteEmployee(UserViewModel user)
    {
        if (user is null)
            return;

        if (user.PreparedProducts is not null && user.PreparedProducts.Any())
            foreach (var type in user.PreparedProducts)
                RemoveProductType(type);

        Employees.Remove(user);

        if (user == SelectedEmployee)
            SelectedEmployee = null!;

        UpdateProducts();
    }

    [RelayCommand]
    private void ShowAllProducts() => SelectedEmployee = null!;

    [RelayCommand]
    private void AddProduct()
    {
        if (SelectedEmployee is null || string.IsNullOrEmpty(SelectedEmployee.Name))
        {
            WarningMessage = "Hodim tanla";
            return;
        }

        ProductEntryViewModel entry = new();
        entry.PropertyChanged += ProductEntryPropertyChanged;
        FilteredProducts.Add(entry);
    }

    [RelayCommand]
    private void DeleteProduct(ProductEntryViewModel type)
    {
        if (type is null || SelectedEmployee is null)
            return;

        RemoveProductType(type);
    }

    [RelayCommand]
    private async Task Submit()
    {
        List<ProductEntryRequest> requests = [];

        foreach (var entry in Employees.SelectMany(e => e.PreparedProducts))
        {
            if (entry.Product is null || entry.ProductType is null)
            {
                WarningMessage = "Barcha maydonlarni to'ldiring!";
                return;
            }

            requests.Add(new ProductEntryRequest
            {
                BundleCount = (int)entry.BundleCount!,
                PreparationCostPerUnit = (decimal)entry.PreparationCostPerUnit!,
                TotalAmount = (decimal)entry.TotalAmount!,
                ProductTypeId = entry.ProductType.Id,
                EmployeeId = SelectedEmployee!.Id
            });
        }

        var response = await Client.ProductEntries.Create(new() { Command = requests });

        if (response.IsSuccess)
            SuccessMessage = "Mahsulotlar muvaffaqiyatli saqlandi.";
        else
            ErrorMessage = response.Message ?? "Mahsulotlarni saqlashda xatolik yuz berdi.";
    }

    #endregion Commands

    #region Load Data

    public async Task InitializeAsync()
    {
        await LoadEmployeesAsync();
        await LoadProductsAsync();

        Employees.Add(new UserViewModel { });
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
                ["unitMeasure"] = ["include"]
            }
        };

        var response = await Client.Products.Filter(request)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
            AvailableProducts = Mapper.Map<ObservableCollection<ProductViewModel>>(response.Data);
        else WarningMessage = "Mahsulotlarni yuklashda xatolik.";
    }

    private async Task LoadTypesAsync(ProductEntryViewModel model)
    {
        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["productid"] = [model.Product.Id.ToString()],
                ["productResidue"] = ["include"]
            }
        };

        var response = await Client.ProductTypes.Filter(request).Handle(isLoading => IsLoading = isLoading);
        if (response.IsSuccess)
            model.AvailableProductTypes = Mapper.Map<ObservableCollection<ProductTypeViewModel>>(response.Data);
        else ErrorMessage = response.Message ?? "Mahsulot o'lchamlarini yuklashda xatolik!";
    }

    private async Task LoadTypeItemssAsync(ProductEntryViewModel model)
    {
        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["productTypeId"] = [model.ProductType.Id.ToString()],
                ["semiproduct"] = ["include:unitMeasure"]
            }
        };

        var response = await Client.ProductTypeItems.Filter(request).Handle(isLoading => IsLoading = isLoading);
        if (response.IsSuccess)
            model.ProductType.ProductTypeItems = Mapper.Map<ObservableCollection<ProductTypeItemViewModel>>(response.Data);
        else ErrorMessage = response.Message ?? "Yarim tayyor mahsulotlarni yuklashda xatolik!";
    }

    #endregion Load Data

    #region Property Changes

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

    private void ProductEntryPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ProductEntryViewModel entry)
            return;

        if (e.PropertyName == nameof(ProductEntryViewModel.Product))
        {
            if (SelectedEmployee is not null)
                _ = LoadTypesAsync(entry);
        }
        else if (e.PropertyName == nameof(ProductEntryViewModel.ProductType))
        {
            SelectedEmployee.PreparedProducts.Add(entry);
            _ = LoadTypeItemssAsync(entry);
        }
    }

    //private void EmployeePropertyChanged(object? sender, PropertyChangedEventArgs e) => AddProduct();

    #endregion Property Changes

    #region Provate Helpers

    private void UpdateProducts()
    {
        FilteredProducts.Clear();

        if (SelectedEmployee is null)
            FilteredProducts.AddRange(Employees.SelectMany(e => e.PreparedProducts));
        else FilteredProducts.AddRange(SelectedEmployee.PreparedProducts);
    }

    private void RemoveProductType(ProductEntryViewModel model)
    {
        SelectedEmployee?.PreparedProducts.Remove(model);
        model.ProductType.Product.ProductTypes.Remove(model.ProductType);
        FilteredProducts.Remove(model);
    }

    #endregion Provate Helpers
}
