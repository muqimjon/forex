namespace Forex.Wpf.Pages.Products.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Enums;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Windows;

public partial class ProductEntryPageViewModel : ViewModelBase
{
    private readonly ForexClient Client = App.AppHost!.Services.GetRequiredService<ForexClient>();
    private readonly IMapper Mapper = App.AppHost!.Services.GetRequiredService<IMapper>();

    public ProductEntryPageViewModel()
    {
        CurrentProductEntry = new ProductEntryViewModel();
        CurrentProductEntry.PropertyChanged += OnCurrentProductEntryPropertyChanged;
        _ = LoadDataAsync();
    }

    [ObservableProperty] private DateTime date = DateTime.Now;
    [ObservableProperty] private ObservableCollection<ProductViewModel> availableProducts = [];
    public string[] ProductionOrigins { get; set; } = Enum.GetNames<ProductionOrigin>();
    [ObservableProperty] private ObservableCollection<ProductEntryViewModel> productEntries = [];
    [ObservableProperty] private ProductEntryViewModel currentProductEntry = new();
    [ObservableProperty] private ProductEntryViewModel? selectedProductEntry = new();

    #region Load Data

    public async Task LoadDataAsync()
    {
        await LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        var response = await Client.Products.GetAllAsync()
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
            AvailableProducts = Mapper.Map<ObservableCollection<ProductViewModel>>(response.Data);
        else
            ErrorMessage = response.Message ?? "Mahsulotlarni yuklashda xatolik!";
    }

    private async Task LoadProductTypesAsync(long productId)
    {
        if (CurrentProductEntry.Product!.ProductTypes.Any())
            return;

        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["productid"] = [productId.ToString()]
            }
        };

        var response = await Client.ProductTypes.Filter(request)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
            CurrentProductEntry.Product!.ProductTypes = Mapper.Map<ObservableCollection<ProductTypeViewModel>>(response.Data);
        else ErrorMessage = response.Message ?? "Maxsulot turlarini yuklashda xatolik";
    }

    #endregion

    #region Property Change Handlers

    private async void OnCurrentProductEntryPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProductEntryViewModel.Product))
        {
            if (CurrentProductEntry.Product is not null &&
                CurrentProductEntry.Product.ProductTypes.Any())
            {
                CurrentProductEntry.IsSelected = false;
                await LoadProductTypesAsync(CurrentProductEntry.Product.Id);
            }
        }
    }

    public async Task ValidateProductCodeAsync(string productCode)
    {
        if (string.IsNullOrWhiteSpace(productCode))
            return;

        productCode = productCode.Trim();

        var existingProduct = AvailableProducts.FirstOrDefault(p =>
            p.Code?.Trim().Equals(productCode, StringComparison.OrdinalIgnoreCase) == true);

        if (existingProduct is not null)
        {
            CurrentProductEntry.Product = existingProduct;
            await LoadProductTypesAsync(existingProduct.Id);
        }
        else
        {
            var result = MessageBox.Show(
                $"'{productCode}' kodli mahsulot topilmadi. Yangi mahsulot qo'shmoqchimisiz?",
                "Tasdiqlash",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                ClearCurrentEntry();
                return;
            }

            var newProduct = new ProductViewModel
            {
                Code = productCode,
                Name = string.Empty
            };

            CurrentProductEntry.Product = newProduct;
            CurrentProductEntry.IsSelected = true;
        }
    }

    public void ValidateProductType(string productType)
    {
        if (string.IsNullOrWhiteSpace(productType))
        {
            return;
        }

        productType = productType.Trim();

        var existingType = CurrentProductEntry.Product!.ProductTypes.FirstOrDefault(t =>
            t.Type?.Trim().Equals(productType, StringComparison.OrdinalIgnoreCase) == true);

        if (existingType is not null)
        {
            CurrentProductEntry.ProductType = existingType;
        }
        else
        {
            var result = MessageBox.Show(
                $"'{productType}' razmeri topilmadi. Yangi razmer qo'shmoqchimisiz?",
                "Tasdiqlash",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                CurrentProductEntry.ProductType = null;
                return;
            }

            var newType = new ProductTypeViewModel
            {
                Type = productType
            };

            CurrentProductEntry.Product!.ProductTypes.Add(newType);
            CurrentProductEntry.ProductType = newType;
        }
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void Add()
    {
        if (CurrentProductEntry.Product is null || string.IsNullOrWhiteSpace(CurrentProductEntry.Product.Code))
        {
            ErrorMessage = "Mahsulot kodini kiriting!";
            return;
        }

        if (CurrentProductEntry.ProductType is null || string.IsNullOrWhiteSpace(CurrentProductEntry.ProductType.Type))
        {
            ErrorMessage = "Razmerni kiriting!";
            return;
        }

        if (CurrentProductEntry.BundleCount <= 0)
        {
            ErrorMessage = "Qop sonini kiriting!";
            return;
        }

        if (CurrentProductEntry.UnitPrice <= 0)
        {
            ErrorMessage = "Tannarxni kiriting!";
            return;
        }

        var item = ProductEntries.FirstOrDefault(pe => pe.IsEditing);
        if (item is not null)
        {
            item.Product = CurrentProductEntry.Product;
            item.ProductType = CurrentProductEntry.ProductType;
            item.BundleCount = CurrentProductEntry.BundleCount;
            item.TotalCount = CurrentProductEntry.TotalCount;
            item.UnitPrice = CurrentProductEntry.UnitPrice;
            item.IsEditing = default;
            return;
        }

        item = new ProductEntryViewModel
        {
            Product = CurrentProductEntry.Product,
            ProductType = CurrentProductEntry.ProductType,
            BundleCount = CurrentProductEntry.BundleCount,
            TotalCount = CurrentProductEntry.TotalCount,
            UnitPrice = CurrentProductEntry.UnitPrice
        };

        ProductEntries.Add(item);

        if (!AvailableProducts.Contains(item.Product))
            AvailableProducts.Add(item.Product);

        ClearCurrentEntry();
    }

    [RelayCommand]
    private async Task Submit()
    {
        if (ProductEntries.Count == 0)
        {
            ErrorMessage = "Hech qanday mahsulot qo'shilmagan!";
            return;
        }

        var result = MessageBox.Show(
            $"{ProductEntries.Count} ta mahsulotni saqlashni tasdiqlaysizmi?",
            "Tasdiqlash",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            SuccessMessage = "Ma'lumotlar muvaffaqiyatli saqlandi!";
            ProductEntries.Clear();
            ClearCurrentEntry();
        }
    }

    private void ClearCurrentEntry()
    {
        CurrentProductEntry.Product = null;
        CurrentProductEntry.ProductType = null;
        CurrentProductEntry.BundleCount = null;
        CurrentProductEntry.TotalCount = null;
        CurrentProductEntry.UnitPrice = null;
    }

    #endregion

    #region Private Helpers

    public void Edit()
    {
        if (SelectedProductEntry is null)
            return;

        SelectedProductEntry.IsEditing = true;
        var copiedEntry = JsonSerializer.Deserialize<ProductEntryViewModel>(JsonSerializer.Serialize(SelectedProductEntry));

        if (copiedEntry is null)
            return;

        CurrentProductEntry.PropertyChanged -= OnCurrentProductEntryPropertyChanged;
        CurrentProductEntry = copiedEntry;
        CurrentProductEntry.PropertyChanged += OnCurrentProductEntryPropertyChanged;
    }

    #endregion
}