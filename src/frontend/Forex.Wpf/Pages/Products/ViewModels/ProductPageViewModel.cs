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
    [ObservableProperty] private ObservableCollection<ProductEntryViewModel> productEntries = [];
    [ObservableProperty] private ProductEntryViewModel? selectedProductEntry;

    #region Commands

    [RelayCommand]
    private void DeleteProduct(ProductEntryViewModel? entry)
    {
        if (entry is null) return;

        ProductEntries.Remove(entry);
        if (SelectedProductEntry == entry)
            SelectedProductEntry = ProductEntries.FirstOrDefault();
    }

    [RelayCommand]
    private async Task Submit()
    {
        if (!ProductEntries.Any())
        {
            WarningMessage = "Mahsulotlar ro'yxati bo'sh!";
            return;
        }

        var requests = ProductEntries
            .Where(p => p.Product is not null && p.ProductType is not null && p.Quantity > 0)
            .Select(p => new ProductEntryRequest
            {
                ProductTypeId = p.ProductType!.Id,
                Quantity = p.Quantity
            })
            .ToList();

        if (requests.Count == 0)
        {
            WarningMessage = "Hech qanday to'g'ri mahsulot kiritilmagan!";
            return;
        }

        var response = await Client.ProductEntries.Create(new() { Command = requests })
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            SuccessMessage = "Mahsulotlar muvaffaqiyatli saqlandi.";
            ProductEntries.Clear();
        }
        else
        {
            ErrorMessage = response.Message ?? "Mahsulotlarni saqlashda xatolik yuz berdi.";
        }
    }

    #endregion Commands

    #region Load Data

    public async Task InitializeAsync()
    {
        await LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["unitMeasure"] = ["include"],
                ["productTypes"] = ["include"]
            }
        };

        var response = await Client.Products.Filter(request)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            AvailableProducts = Mapper.Map<ObservableCollection<ProductViewModel>>(response.Data);
        }
        else
        {
            ErrorMessage = response.Message ?? "Mahsulotlarni yuklashda xatolik.";
        }
    }

    private async Task LoadProductTypesAsync(ProductEntryViewModel entry)
    {
        if (entry.Product is null) return;

        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["productid"] = [entry.Product.Id.ToString()],
                ["productResidue"] = ["include"]
            }
        };

        var response = await Client.ProductTypes.Filter(request)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            entry.AvailableProductTypes = Mapper.Map<ObservableCollection<ProductTypeViewModel>>(response.Data);
        }
        else
        {
            ErrorMessage = response.Message ?? "Mahsulot o'lchamlarini yuklashda xatolik!";
        }
    }

    private async Task LoadTypeItemsAsync(ProductEntryViewModel entry)
    {
        if (entry.ProductType is null) return;

        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["productTypeId"] = [entry.ProductType.Id.ToString()],
                ["semiproduct"] = ["include:unitMeasure"]
            }
        };

        var response = await Client.ProductTypeItems.Filter(request)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            entry.ProductType.ProductTypeItems = Mapper.Map<ObservableCollection<ProductTypeItemViewModel>>(response.Data);
            entry.AvailableSemiProducts = new ObservableCollection<ProductTypeItemViewModel>(
                entry.ProductType.ProductTypeItems);
        }
        else
        {
            ErrorMessage = response.Message ?? "Yarim tayyor mahsulotlarni yuklashda xatolik!";
        }
    }

    #endregion Load Data

    #region Property Changes

    partial void OnSelectedProductEntryChanged(ProductEntryViewModel? value)
    {
        if (value?.ProductType is not null)
        {
            _ = LoadTypeItemsAsync(value);
        }
    }

    #endregion Property Changes

    #region Internal Methods

    internal async Task OnProductChanged(ProductEntryViewModel entry)
    {
        if (entry.Product is null) return;

        entry.AvailableProductTypes.Clear();
        entry.ProductType = null;
        entry.AvailableSemiProducts.Clear();

        await LoadProductTypesAsync(entry);
    }

    internal async Task OnProductTypeChanged(ProductEntryViewModel entry)
    {
        if (entry.ProductType is null) return;

        entry.AvailableSemiProducts.Clear();
        await LoadTypeItemsAsync(entry);

        if (entry == SelectedProductEntry)
            OnPropertyChanged(nameof(SelectedProductEntry));
    }

    #endregion Internal Methods
}