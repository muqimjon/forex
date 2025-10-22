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
using System.ComponentModel;
using System.Threading.Tasks;

public partial class ProductPageViewModel(ForexClient Client, IMapper Mapper) : ViewModelBase
{

    [ObservableProperty] private int count;
    [ObservableProperty] private decimal totalCount;
    [ObservableProperty] private decimal costPreparation;
    [ObservableProperty] private decimal totalAmount;

    [ObservableProperty] private ObservableCollection<UserViewModel> availableEmployees = [];
    [ObservableProperty] private ObservableCollection<UserViewModel> employees = [];
    private UserViewModel? selectedEmployee;

    [ObservableProperty] private ObservableCollection<ProductViewModel> availableProducts = [];
    [ObservableProperty] private ObservableCollection<ProductTypeViewModel> availableProductTypes = [];
    [ObservableProperty] private ObservableCollection<ProductTypeViewModel> filteredProductTypes = [];
    [ObservableProperty] private ProductTypeViewModel? selectedProductType;


    #region Commands

    [RelayCommand]
    private void AddEmployee() => Employees.Add(new());

    [RelayCommand]
    private void DeleteEmployee(UserViewModel user)
    {
        if (user is null)
            return;

        if (user.PreparedProductTypes is not null && user.PreparedProductTypes.Any())
            foreach (var type in user.PreparedProductTypes)
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

        ProductTypeViewModel type = new();
        type.PropertyChanged += ProductTypePropertyChanged;
        FilteredProductTypes.Add(type);
    }

    [RelayCommand]
    private void DeleteProduct(ProductTypeViewModel type)
    {
        if (type is null || SelectedEmployee is null)
            return;

        RemoveProductType(type);
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

    private async Task LoadTypesAsync(ProductTypeViewModel type)
    {
        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["productid"] = [type.Product.Id.ToString()]
            }
        };

        var response = await Client.ProductType.Filter(request).Handle(isLoading => IsLoading = isLoading);
        if (response.IsSuccess)
            AvailableProductTypes = Mapper.Map<ObservableCollection<ProductTypeViewModel>>(response.Data);
        else ErrorMessage = response.Message ?? "Mahsulot o'lchamlarini yuklashda xatolik!";
    }

    #endregion Load Data

    #region Property Changes

    partial void OnCountChanged(int value)
    {
        TotalCount = SelectedEmployee.PreparedProductTypes.Sum(t => t.Count ?? 0);
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

    private void ProductTypePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ProductTypeViewModel type)
            return;

        if (e.PropertyName == nameof(ProductTypeViewModel.Product))
        {
            if (SelectedEmployee is not null)
                _ = LoadTypesAsync(type);
        }
        else if (e.PropertyName == nameof(ProductTypeViewModel.Type))
            SelectedEmployee.PreparedProductTypes.Add(type);
    }

    #endregion Property Changes

    #region Provate Helpers

    private void UpdateProducts()
    {
        FilteredProductTypes.Clear();

        if (SelectedEmployee is null)
            FilteredProductTypes.AddRange(Employees.SelectMany(e => e.PreparedProductTypes));
        else FilteredProductTypes.AddRange(SelectedEmployee.PreparedProductTypes);
    }

    private void RemoveProductType(ProductTypeViewModel type)
    {
        SelectedEmployee?.PreparedProductTypes.Remove(type);
        type.Product.ProductTypes.Remove(type);
        FilteredProductTypes.Remove(type);
    }

    #endregion Provate Helpers
}
