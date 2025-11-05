namespace Forex.Wpf.Pages.Products.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using System.Collections.ObjectModel;

public partial class ProductPageViewModel(ForexClient Client, IMapper Mapper) : ViewModelBase
{

    [ObservableProperty] private ObservableCollection<ProductViewModel> availableProducts = [];

    [ObservableProperty] private ObservableCollection<ProductEntryViewModel> filteredProducts = [];
    [ObservableProperty] private ProductEntryViewModel? selectedProduct;


    #region Commands



    [RelayCommand]
    private void DeleteEmployee(UserViewModel user)
    {
        if (user.PreparedProducts is not null && user.PreparedProducts.Any())
            foreach (var type in user.PreparedProducts)
                RemoveProductType(type);


        UpdateProducts();
    }


    [RelayCommand]
    private void AddProduct()
    {
        ProductEntryViewModel entry = new();
        FilteredProducts.Add(entry);
    }

    [RelayCommand]
    private void DeleteProduct(ProductEntryViewModel type)
    {
        if (type is null)
            return;

        RemoveProductType(type);
    }
    [RelayCommand]
    private async Task Submit()
    {
        List<ProductEntryRequest> requests = [];


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
        await LoadProductsAsync();

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


    #endregion Property Changes

    #region Provate Helpers

    private void UpdateProducts()
    {
        FilteredProducts.Clear();
    }

    private void RemoveProductType(ProductEntryViewModel model)
    {
        model.ProductType.Product.ProductTypes.Remove(model.ProductType);
        FilteredProducts.Remove(model);
    }

    #endregion Provate Helpers
}
