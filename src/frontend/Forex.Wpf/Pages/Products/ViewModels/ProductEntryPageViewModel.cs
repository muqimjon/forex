namespace Forex.Wpf.Pages.Products.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Enums;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Forex.Wpf.Common.Interfaces;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using System.Collections.ObjectModel;
using System.Windows;

public partial class ProductEntryPageViewModel : ViewModelBase
{
    private readonly ForexClient Client;
    private readonly IMapper Mapper;
    private readonly INavigationService Navigation;

    public ProductEntryPageViewModel(IMapper mapper, ForexClient client, INavigationService navigation)
    {
        Mapper = mapper;
        Client = client;
        Navigation = navigation;
        CurrentProductEntry = new ProductEntryViewModel();
        CurrentProductEntry.PropertyChanged += OnCurrentProductEntryPropertyChanged;
        _ = LoadDataAsync();
    }

    [ObservableProperty] private DateTime date = DateTime.Now;
    [ObservableProperty] private ObservableCollection<ProductViewModel> availableProducts = [];
    public string[] ProductionOrigins { get; set; } = Enum.GetNames<ProductionOrigin>();
    [ObservableProperty] private ObservableCollection<ProductEntryViewModel> productEntries = [];
    [ObservableProperty] private ProductEntryViewModel currentProductEntry = new();
    [ObservableProperty] private ProductEntryViewModel? selectedProductEntry = default;

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

        var groupedByProduct = ProductEntries
            .Where(p => p.Product is not null &&
                       p.Product.SelectedType is not null &&
                       p.Count > 0)
            .GroupBy(p => p.Product!.Code)
            .Select(group =>
            {
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

            if (Navigation.CanGoBack) Navigation.GoBack();
            else Navigation.NavigateTo(new ProductPage());
        }
        else
        {
            ErrorMessage = response.Message ?? "Mahsulotlarni saqlashda xatolik yuz berdi.";
        }
    }

    #endregion

    #region Private Helpers

    public async Task Edit()
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

        // PropertyChanged eventini vaqtincha o'chirish
        CurrentProductEntry.PropertyChanged -= OnCurrentProductEntryPropertyChanged;

        // AvailableProducts dan to'g'ri Product obyektini topish
        var originalProduct = AvailableProducts.FirstOrDefault(p =>
            p.Code == SelectedProductEntry.Product?.Code);

        if (originalProduct is not null)
        {
            // ProductTypes yuklanganligini tekshirish
            if (!originalProduct.ProductTypes.Any())
            {
                await LoadProductTypesForProduct(originalProduct);
            }

            // CurrentProductEntry ga ma'lumotlarni ko'chirish
            CurrentProductEntry.Product = originalProduct;

            // SelectedType ni to'g'ri topish
            var selectedType = originalProduct.ProductTypes.FirstOrDefault(pt =>
                pt.Type == SelectedProductEntry.Product?.SelectedType?.Type);

            if (selectedType is not null)
            {
                CurrentProductEntry.Product.SelectedType = selectedType;
            }
        }
        else
        {
            // Agar AvailableProducts da yo'q bo'lsa, yangi obyekt yaratish
            CurrentProductEntry.Product = new ProductViewModel
            {
                Id = SelectedProductEntry.Product!.Id,
                Code = SelectedProductEntry.Product.Code,
                Name = SelectedProductEntry.Product.Name,
                UnitMeasureId = SelectedProductEntry.Product.UnitMeasureId,
                ProductionOrigin = SelectedProductEntry.ProductionOrigin,
                ProductTypes = []
            };

            if (SelectedProductEntry.Product.Id > 0)
            {
                await LoadProductTypesForProduct(CurrentProductEntry.Product);
            }

            // SelectedType ni qayta yaratish
            var newType = new ProductTypeViewModel
            {
                Id = SelectedProductEntry.Product.SelectedType!.Id,
                Type = SelectedProductEntry.Product.SelectedType.Type,
                BundleItemCount = SelectedProductEntry.Product.SelectedType.BundleItemCount,
                UnitPrice = SelectedProductEntry.Product.SelectedType.UnitPrice,
                ProductTypeItems = new ObservableCollection<ProductTypeItemViewModel>(
                    SelectedProductEntry.Product.SelectedType.ProductTypeItems ?? []
                )
            };

            CurrentProductEntry.Product.ProductTypes.Add(newType);
            CurrentProductEntry.Product.SelectedType = newType;
        }

        // Qolgan ma'lumotlarni ko'chirish
        CurrentProductEntry.BundleCount = SelectedProductEntry.BundleCount;
        CurrentProductEntry.Count = SelectedProductEntry.Count;
        CurrentProductEntry.ProductionOriginName = SelectedProductEntry.ProductionOriginName;
        CurrentProductEntry.ProductionOrigin = SelectedProductEntry.ProductionOrigin;

        // PropertyChanged eventini qayta ulash
        CurrentProductEntry.PropertyChanged += OnCurrentProductEntryPropertyChanged;

        // DataGrid dan o'chirish
        ProductEntries.Remove(SelectedProductEntry);
        SelectedProductEntry = null;
    }

    private async Task LoadProductTypesForProduct(ProductViewModel product)
    {
        if (product.ProductTypes.Any() || product.Id <= 0)
            return;

        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["productid"] = [product.Id.ToString()]
            }
        };

        var response = await Client.ProductTypes.Filter(request)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
            product.ProductTypes = Mapper.Map<ObservableCollection<ProductTypeViewModel>>(response.Data);
        else
            ErrorMessage = response.Message ?? "Maxsulot turlarini yuklashda xatolik";
    }

    private void ClearCurrentEntry()
    {
        CurrentProductEntry.Product = null;
        CurrentProductEntry.BundleCount = null;
        CurrentProductEntry.Count = null;
        CurrentProductEntry.ProductionOriginName = null!;
        CurrentProductEntry.IsEditing = false;
    }

    #endregion
}