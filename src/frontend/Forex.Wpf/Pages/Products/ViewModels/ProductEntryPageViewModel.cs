namespace Forex.Wpf.Pages.Products.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Enums;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
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
            if (CurrentProductEntry.Product is not null && CurrentProductEntry.Product.Id > 0)
            {
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
                Name = string.Empty,
                ProductTypes = []
            };

            CurrentProductEntry.Product = newProduct;
            CurrentProductEntry.IsEditing = true;
        }
    }

    public void ValidateProductType(string productType)
    {
        if (string.IsNullOrWhiteSpace(productType) || CurrentProductEntry.Product is null)
            return;

        productType = productType.Trim();

        var existingType = CurrentProductEntry.Product.ProductTypes?.FirstOrDefault(t =>
            t.Type?.Trim().Equals(productType, StringComparison.OrdinalIgnoreCase) == true);

        if (existingType is not null)
        {
            CurrentProductEntry.Product.SelectedType = existingType;
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
                CurrentProductEntry.Product.SelectedType = null;
                return;
            }

            var newType = new ProductTypeViewModel
            {
                Type = productType,
                ProductTypeItems = []
            };

            CurrentProductEntry.Product.ProductTypes ??= [];
            CurrentProductEntry.Product.ProductTypes.Add(newType);
            CurrentProductEntry.Product.SelectedType = newType;
        }
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void Add()
    {
        if (CurrentProductEntry.Product is null || string.IsNullOrWhiteSpace(CurrentProductEntry.Product.Code))
        {
            WarningMessage = "Mahsulot kodini kiriting!";
            return;
        }
        if (string.IsNullOrWhiteSpace(CurrentProductEntry.Product.Name))
        {
            WarningMessage = "Mahsulot nomini kiriting!";
            return;
        }
        if (CurrentProductEntry.Product.SelectedType is null || string.IsNullOrWhiteSpace(CurrentProductEntry.Product.SelectedType.Type))
        {
            WarningMessage = "Razmerni kiriting!";
            return;
        }
        if (CurrentProductEntry.BundleCount <= 0)
        {
            WarningMessage = "Qop sonini kiriting!";
            return;
        }
        if (CurrentProductEntry.Product.SelectedType.BundleItemCount <= 0)
        {
            WarningMessage = "Qopdagi sonni kiriting!";
            return;
        }
        if (CurrentProductEntry.Product.SelectedType.UnitPrice <= 0)
        {
            WarningMessage = "Tannarxni kiriting!";
            return;
        }
        if (string.IsNullOrWhiteSpace(CurrentProductEntry.ProductionOriginName))
        {
            WarningMessage = "Tayyorlanish usulini tanlang!";
            return;
        }

        // Mavjud yozuvni tekshirish
        var existingItem = ProductEntries.FirstOrDefault(pe =>
            pe.Product?.Code == CurrentProductEntry.Product.Code &&
            pe.Product?.SelectedType?.Type == CurrentProductEntry.Product.SelectedType.Type);

        if (existingItem is not null)
        {
            var result = MessageBox.Show(
                $"Ushbu mahsulot ro'yxatda mavjud. Ma'lumotlarni yangilashni tasdiqlaysizmi?",
                "Tasdiqlash",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
                return;

            // Mavjud yozuvni yangilash
            existingItem.Product = CurrentProductEntry.Product;
            existingItem.BundleCount = CurrentProductEntry.BundleCount;
            existingItem.Count = CurrentProductEntry.Count;
            existingItem.ProductionOrigin = CurrentProductEntry.ProductionOrigin;
            existingItem.ProductionOriginName = CurrentProductEntry.ProductionOriginName;

            SuccessMessage = "Ma'lumotlar muvaffaqiyatli yangilandi!";
        }
        else
        {
            // Yangi entry yaratish - Product obyektini klon qilish
            var clonedProduct = new ProductViewModel
            {
                Id = CurrentProductEntry.Product.Id,
                Code = CurrentProductEntry.Product.Code,
                Name = CurrentProductEntry.Product.Name,
                UnitMeasureId = CurrentProductEntry.Product.UnitMeasureId,
                ProductionOrigin = CurrentProductEntry.ProductionOrigin,
                SelectedType = new ProductTypeViewModel
                {
                    Id = CurrentProductEntry.Product.SelectedType.Id,
                    Type = CurrentProductEntry.Product.SelectedType.Type,
                    BundleItemCount = CurrentProductEntry.Product.SelectedType.BundleItemCount,
                    UnitPrice = CurrentProductEntry.Product.SelectedType.UnitPrice,
                    ProductTypeItems = new ObservableCollection<ProductTypeItemViewModel>(
                        CurrentProductEntry.Product.SelectedType.ProductTypeItems ?? []
                    )
                }
            };

            var item = new ProductEntryViewModel
            {
                Product = clonedProduct,
                BundleCount = CurrentProductEntry.BundleCount,
                Count = CurrentProductEntry.Count,
                ProductionOrigin = CurrentProductEntry.ProductionOrigin,
                ProductionOriginName = CurrentProductEntry.ProductionOriginName
            };

            ProductEntries.Insert(0, item);

            if (!AvailableProducts.Any(p => p.Code == CurrentProductEntry.Product.Code))
                AvailableProducts.Add(CurrentProductEntry.Product);
        }

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

        if (result == MessageBoxResult.No)
            return;

        // Product Code bo'yicha guruhlash
        var groupedByProduct = ProductEntries
            .Where(p => p.Product is not null &&
                       p.Product.SelectedType is not null &&
                       p.Count > 0)
            .GroupBy(p => p.Product!.Code)
            .Select(group =>
            {
                var firstEntry = group.First();

                // Har bir ProductType uchun alohida ProductEntryRequest
                var productTypeRequests = group.Select(entry => new ProductEntryRequest
                {
                    Date = Date,
                    Count = entry.Count,
                    BundleItemCount = (uint)entry.Product!.SelectedType!.BundleItemCount!.Value,
                    PreparationCostPerUnit = 0,
                    UnitPrice = entry.Product.SelectedType.UnitPrice!.Value,
                    ProductionOrigin = entry.ProductionOrigin,
                    Product = new ProductRequest
                    {
                        Id = entry.Product.Id,
                        Code = entry.Product.Code,
                        Name = entry.Product.Name,
                        ProductionOrigin = entry.ProductionOrigin,
                        ProductTypes =
                        [
                            new ProductTypeRequest
                            {
                                Id = entry.Product.SelectedType.Id,
                                Type = entry.Product.SelectedType.Type,
                                BundleItemCount = (int)entry.Product.SelectedType.BundleItemCount.Value,
                                UnitPrice = entry.Product.SelectedType.UnitPrice.Value,
                                ProductTypeItems = []
                            }
                        ]
                    }
                }).ToList();

                return productTypeRequests;
            })
            .SelectMany(x => x)
            .ToList();

        if (groupedByProduct.Count == 0)
        {
            WarningMessage = "Hech qanday to'g'ri mahsulot kiritilmagan!";
            return;
        }

        var response = await Client.ProductEntries.Create(new CreateProductEntryCommandRequest { Command = groupedByProduct })
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            SuccessMessage = "Mahsulotlar muvaffaqiyatli saqlandi.";
            ProductEntries.Clear();
            ClearCurrentEntry();
            await LoadProductsAsync();
        }
        else
        {
            ErrorMessage = response.Message ?? "Mahsulotlarni saqlashda xatolik yuz berdi.";
        }
    }

    #endregion

    #region Private Helpers

    public void Edit()
    {
        if (SelectedProductEntry is null)
            return;

        bool hasCurrentData = CurrentProductEntry.Product is not null ||
                             CurrentProductEntry.BundleCount.HasValue ||
                             CurrentProductEntry.Count.HasValue;

        if (hasCurrentData)
        {
            var result = MessageBox.Show(
                "Hozirgi kiritilgan ma'lumotlar o'chib ketadi. Davom etmoqchimisiz?",
                "Ogohlantirish",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.No)
                return;
        }

        CurrentProductEntry.PropertyChanged -= OnCurrentProductEntryPropertyChanged;
        CurrentProductEntry = SelectedProductEntry;
        CurrentProductEntry.PropertyChanged += OnCurrentProductEntryPropertyChanged;
        ProductEntries.Remove(SelectedProductEntry);
        SelectedProductEntry = null;
        SuccessMessage = "Ma'lumotlar tahrirlash uchun yuklandi!";
    }

    private void ClearCurrentEntry()
    {
        CurrentProductEntry.Product = null;
        CurrentProductEntry.BundleCount = null;
        CurrentProductEntry.Count = null;
        CurrentProductEntry.ProductionOriginName = null!;
    }

    #endregion
}