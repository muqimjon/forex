namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService.Enums;
using Forex.ClientService.Extensions;
using Forex.ClientService.Interfaces;
using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

public partial class SemiProductPageViewModel : ViewModelBase
{
    private readonly IServiceProvider services;
    private readonly IMapper mapper;
    private bool isAutoAdding = false;

    public SemiProductPageViewModel(IServiceProvider services, IMapper mapper)
    {
        this.services = services;
        this.mapper = mapper;

        Products = [];
        Products.CollectionChanged += Products_CollectionChanged;

        AddEmptyProduct();

        Invoice.PropertyChanged += InvoicePropertyChanged;
        _ = LoadDataAsync();
    }

    [ObservableProperty] private InvoiceViewModel invoice = new();
    [ObservableProperty] private ObservableCollection<ProductViewModel> products;
    [ObservableProperty] private ObservableCollection<UnitMeasuerViewModel> availableUnitMeasures = [];
    [ObservableProperty] private ObservableCollection<UserViewModel> availableSuppliers = [];
    [ObservableProperty] private ObservableCollection<UserViewModel> availableAgents = [];
    [ObservableProperty] private ObservableCollection<CurrencyViewModel> availableCurrencies = [];

    #region Load Data

    private async Task LoadDataAsync()
    {
        await Task.WhenAll(
            LoadUnitMeasures(),
            LoadUsersAsync(),
            LoadCurrenciesAsync()
        );
    }

    private async Task LoadCurrenciesAsync()
    {
        var client = services.GetRequiredService<IApiCurrency>();
        var response = await client.GetAllAsync().Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            AvailableCurrencies = mapper.Map<ObservableCollection<CurrencyViewModel>>(response.Data);
            Invoice.Currency = AvailableCurrencies.FirstOrDefault()!;
        }
        else
        {
            ErrorMessage = response.Message ?? "Valyuta turlarini yuklashda xatolik";
        }
    }

    private async Task LoadUnitMeasures()
    {
        var client = services.GetRequiredService<IApiUnitMeasures>();
        var response = await client.GetAllAsync().Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            AvailableUnitMeasures = mapper.Map<ObservableCollection<UnitMeasuerViewModel>>(response.Data);
        }
        else
        {
            ErrorMessage = response.Message ?? "O'lchov birliklarini yuklashda xatolik";
        }
    }

    private async Task LoadUsersAsync()
    {
        var request = new FilteringRequest
        {
            Filters = new()
            {
                ["role"] = ["in:Taminotchi,Vositachi"]
            }
        };

        var client = services.GetRequiredService<IApiUser>();
        var response = await client.Filter(request).Handle(isLoading => IsLoading = isLoading);

        if (!response.IsSuccess)
        {
            ErrorMessage = response.Message ?? "Foydalanuvchilarni yuklashda noma'lum xatolik yuz berdi.";
            return;
        }

        var suppliers = response.Data!.Where(u => u.Role == UserRole.Taminotchi);
        var agents = response.Data!.Where(u => u.Role == UserRole.Vositachi);

        AvailableSuppliers = mapper.Map<ObservableCollection<UserViewModel>>(suppliers);
        AvailableAgents = mapper.Map<ObservableCollection<UserViewModel>>(agents);

        Invoice.Supplier = AvailableSuppliers.FirstOrDefault() ?? new();
    }

    #endregion

    #region Check - Tekshirish funksiyalari

    private bool IsProductModified(ProductViewModel product)
    {
        return !string.IsNullOrWhiteSpace(product.Name) ||
               !string.IsNullOrWhiteSpace(product.Code);
    }

    private bool IsProductTypeModified(ProductTypeViewModel type)
    {
        return !string.IsNullOrWhiteSpace(type.Type);
    }

    private bool IsSemiProductModified(SemiProductViewModel semi)
    {
        return semi.Quantity != 0 || semi.CostPrice != 0;
    }

    private bool IsProductEmpty(ProductViewModel product)
    {
        return string.IsNullOrWhiteSpace(product.Name) &&
               string.IsNullOrWhiteSpace(product.Code);
    }

    private bool IsProductTypeEmpty(ProductTypeViewModel type)
    {
        return string.IsNullOrWhiteSpace(type.Type);
    }

    private bool IsSemiProductEmpty(SemiProductViewModel semi)
    {
        return semi.Quantity == 0 && semi.CostPrice == 0;
    }

    private bool IsLastProduct(ProductViewModel product)
    {
        return Products.LastOrDefault() == product;
    }

    private bool IsLastProductType(ProductViewModel product, ProductTypeViewModel type)
    {
        return product.ProductTypes.LastOrDefault() == type;
    }

    private bool IsLastItem(ProductTypeViewModel type, ProductTypeItemViewModel item)
    {
        return type.ProductTypeItems.LastOrDefault() == item;
    }

    private int CountEmptyProducts()
    {
        return Products.Count(p => IsProductEmpty(p));
    }

    private int CountEmptyProductTypes(ProductViewModel product)
    {
        return product.ProductTypes.Count(t => IsProductTypeEmpty(t));
    }

    private int CountEmptyItems(ProductTypeViewModel type)
    {
        return type.ProductTypeItems.Count(i => IsSemiProductEmpty(i.SemiProduct));
    }

    // ✅ TO'LIQ VALIDATSIYA - Kod va Name majburiy
    private bool IsProductCompleted(ProductViewModel product)
    {
        return !string.IsNullOrWhiteSpace(product.Code) &&
               !string.IsNullOrWhiteSpace(product.Name);
    }

    // ✅ TO'LIQ VALIDATSIYA - Type majburiy
    private bool IsProductTypeCompleted(ProductTypeViewModel type)
    {
        return !string.IsNullOrWhiteSpace(type.Type);
    }

    // ✅ TO'LIQ VALIDATSIYA - Name EMAS, faqat Quantity, UnitMeasure, CostPrice
    private bool IsSemiProductCompleted(SemiProductViewModel semi)
    {
        return semi.Quantity > 0 &&
               semi.UnitMeasure is not null &&
               semi.CostPrice > 0;
    }

    #endregion

    #region Add - Qo'shish funksiyalari

    private void AddEmptyProduct()
    {
        var product = new ProductViewModel();
        AttachProduct(product);
        Products.Add(product);
    }

    private void AddEmptyProductType(ProductViewModel product)
    {
        var type = new ProductTypeViewModel();
        AttachProductType(type);
        product.ProductTypes.Add(type);
    }

    private void AddEmptyItem(ProductTypeViewModel type)
    {
        var item = new ProductTypeItemViewModel
        {
            SemiProduct = new SemiProductViewModel()
        };
        AttachSemiProduct(item.SemiProduct);
        type.ProductTypeItems.Add(item);
    }

    #endregion

    #region Auto Add - Avtomatik qo'shish mantiqasi

    private void CheckAndAutoAdd(ProductViewModel product)
    {
        if (isAutoAdding) return;

        isAutoAdding = true;

        try
        {
            if (IsProductModified(product) && IsLastProduct(product))
            {
                if (product.ProductTypes.Count == 0)
                {
                    AddEmptyProductType(product);
                }

                if (CountEmptyProducts() == 0)
                {
                    AddEmptyProduct();
                }
            }
        }
        finally
        {
            isAutoAdding = false;
        }
    }

    private void CheckAndAutoAdd(ProductViewModel product, ProductTypeViewModel type)
    {
        if (isAutoAdding) return;

        isAutoAdding = true;

        try
        {
            if (IsProductTypeModified(type) && IsLastProductType(product, type))
            {
                if (type.ProductTypeItems.Count == 0)
                {
                    AddEmptyItem(type);
                }

                if (CountEmptyProductTypes(product) == 0)
                {
                    AddEmptyProductType(product);
                }
            }
        }
        finally
        {
            isAutoAdding = false;
        }
    }

    private void CheckAndAutoAdd(ProductViewModel product, ProductTypeViewModel type, ProductTypeItemViewModel item)
    {
        if (isAutoAdding) return;

        isAutoAdding = true;

        try
        {
            if (IsSemiProductModified(item.SemiProduct) && IsLastItem(type, item))
            {
                if (CountEmptyItems(type) == 0)
                {
                    AddEmptyItem(type);
                }
            }
        }
        finally
        {
            isAutoAdding = false;
        }
    }

    #endregion

    #region Remove Commands

    [RelayCommand]
    private void RemoveProduct(ProductViewModel product)
    {
        if (IsProductEmpty(product))
        {
            WarningMessage = "Bo'sh mahsulotni o'chirib bo'lmaydi!";
            return;
        }

        if (Products.Count <= 1)
        {
            WarningMessage = "Kamida bitta mahsulot bo'lishi kerak!";
            return;
        }

        DetachProduct(product);
        Products.Remove(product);
        UpdateInvoiceTotal();
    }

    [RelayCommand]
    private void RemoveProductType(object[] parameters)
    {
        if (parameters?.Length != 2) return;

        if (parameters[0] is not ProductViewModel product
            || parameters[1] is not ProductTypeViewModel productType) return;

        if (IsProductTypeEmpty(productType))
        {
            WarningMessage = "Bo'sh o'lchamni o'chirib bo'lmaydi!";
            return;
        }

        if (product.ProductTypes.Count <= 1)
        {
            WarningMessage = "Kamida bitta o'lcham bo'lishi kerak!";
            return;
        }

        DetachProductType(productType);
        product.ProductTypes.Remove(productType);
        UpdateInvoiceTotal();
    }

    [RelayCommand]
    private void RemoveProductTypeItem(object[] parameters)
    {
        if (parameters?.Length != 2) return;

        if (parameters[0] is not ProductTypeViewModel productType
            || parameters[1] is not ProductTypeItemViewModel item) return;

        if (IsSemiProductEmpty(item.SemiProduct))
        {
            WarningMessage = "Bo'sh elementni o'chirib bo'lmaydi!";
            return;
        }

        if (productType.ProductTypeItems.Count <= 1)
        {
            WarningMessage = "Kamida bitta element bo'lishi kerak!";
            return;
        }

        DetachSemiProduct(item.SemiProduct);
        productType.ProductTypeItems.Remove(item);
        UpdateInvoiceTotal();
    }

    #endregion

    #region Submit Command

    [RelayCommand]
    private async Task SubmitAsync()
    {
        try
        {
            var validProducts = Products
                .Where(IsProductCompleted)
                .Select(p =>
                {
                    var validTypes = p.ProductTypes
                        .Where(IsProductTypeCompleted)
                        .Select(t =>
                        {
                            var validItems = t.ProductTypeItems
                                .Where(i => IsSemiProductCompleted(i.SemiProduct))
                                .ToList();

                            if (validItems.Count == 0)
                                return null;

                            return new ProductTypeViewModel
                            {
                                Id = t.Id,
                                Type = t.Type,
                                BundleItemCount = t.BundleItemCount,
                                Cost = t.Cost,
                                ProductTypeItems = new ObservableCollection<ProductTypeItemViewModel>(validItems)
                            };
                        })
                        .Where(t => t is not null)
                        .ToList();

                    if (validTypes.Count == 0)
                        return null;

                    return new ProductViewModel
                    {
                        Id = p.Id,
                        Code = p.Code,
                        Name = p.Name,
                        UnitMeasure = p.UnitMeasure,
                        Image = p.Image,
                        ProductTypes = new ObservableCollection<ProductTypeViewModel>(validTypes!)
                    };
                })
                .Where(p => p is not null)
                .ToList();

            if (validProducts.Count == 0)
            {
                ErrorMessage = "Hech qanday to'liq ma'lumot kiritilmagan. Kamida: 1 tadan qiymat to'ldirilishi kerak!";
                return;
            }

            var requestObject = new SemiProductIntakeRequest
            {
                Invoice = mapper.Map<InvoiceRequest>(Invoice),
                Products = mapper.Map<ICollection<ProductRequest>>(validProducts),
                SemiProducts = []
            };

            var currency = AvailableCurrencies.FirstOrDefault(c => c.IsDefault);

            if (currency is null)
            {
                WarningMessage = "Default valyuta tanlanmagan";
                return;
            }

            if (!currency.Code.Equals("UZS", StringComparison.InvariantCultureIgnoreCase))
            {
                WarningMessage = "Default valyuta UZS bo'lishi kerak";
                return;
            }

            requestObject.Invoice.CurrencyId = currency.Id;

            var client = services.GetRequiredService<IApiSemiProductEntry>();
            var response = await client.Create(requestObject).Handle(isLoading => IsLoading = isLoading);

            if (response.IsSuccess)
            {
                SuccessMessage = $"Muvaffaqiyatli yuklandi!\n" +
                               $"- {validProducts.Count} ta mahsulot\n" +
                               $"- {validProducts.Sum(p => p.ProductTypes.Count)} ta o'lcham\n" +
                               $"- {validProducts.Sum(p => p.ProductTypes.Sum(t => t.ProductTypeItems.Count))} ta element";
                ResetForm();
            }
            else
            {
                ErrorMessage = response.Message ?? "Yuklashda xatolik yuz berdi.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Xatolik yuz berdi: {ex.Message}";
        }
    }

    private void ResetForm()
    {
        foreach (var product in Products.ToList())
        {
            DetachProduct(product);
        }

        Products.Clear();
        Invoice = new InvoiceViewModel();
        Invoice.PropertyChanged += InvoicePropertyChanged;

        AddEmptyProduct();
    }

    #endregion

    #region Event Handlers

    private void Products_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (ProductViewModel product in e.NewItems)
            {
                AttachProduct(product);
            }
        }

        if (e.OldItems is not null)
        {
            foreach (ProductViewModel product in e.OldItems)
            {
                DetachProduct(product);
            }
        }
    }

    private void ProductTypes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (ProductTypeViewModel type in e.NewItems)
            {
                AttachProductType(type);
            }
        }

        if (e.OldItems is not null)
        {
            foreach (ProductTypeViewModel type in e.OldItems)
            {
                DetachProductType(type);
            }
        }

        UpdateInvoiceTotal();
    }

    private void ProductTypeItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (ProductTypeItemViewModel item in e.NewItems)
            {
                AttachSemiProduct(item.SemiProduct);
            }
        }

        if (e.OldItems is not null)
        {
            foreach (ProductTypeItemViewModel item in e.OldItems)
            {
                DetachSemiProduct(item.SemiProduct);
            }
        }

        UpdateInvoiceTotal();
    }

    private void Product_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ProductViewModel product) return;

        if (e.PropertyName == nameof(ProductViewModel.Name) ||
            e.PropertyName == nameof(ProductViewModel.Code))
        {
            CheckAndAutoAdd(product);
        }

        UpdateInvoiceTotal();
    }

    private void ProductType_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ProductTypeViewModel type) return;

        if (e.PropertyName == nameof(ProductTypeViewModel.Type))
        {
            var product = FindProductByType(type);
            if (product is not null)
            {
                CheckAndAutoAdd(product, type);
            }
        }

        UpdateInvoiceTotal();
    }

    private void SemiProduct_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not SemiProductViewModel semi) return;

        if (e.PropertyName == nameof(SemiProductViewModel.Name) ||
            e.PropertyName == nameof(SemiProductViewModel.Quantity) ||
            e.PropertyName == nameof(SemiProductViewModel.CostPrice))
        {
            var (product, type, item) = FindHierarchyBySemiProduct(semi);
            if (product is not null && type is not null && item is not null)
            {
                CheckAndAutoAdd(product, type, item);
            }
        }

        UpdateInvoiceTotal();
    }

    private void InvoicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Invoice.ViaMiddleman))
        {
            var usdCurrency = AvailableCurrencies.FirstOrDefault(c =>
                c.Code.Equals("USD", StringComparison.InvariantCultureIgnoreCase));

            if (usdCurrency is not null)
            {
                Invoice.Currency = usdCurrency;
            }
        }
    }

    #endregion

    #region Find - Qidirish funksiyalari

    private ProductViewModel? FindProductByType(ProductTypeViewModel type)
    {
        return Products.FirstOrDefault(p => p.ProductTypes.Contains(type));
    }

    private (ProductViewModel? product, ProductTypeViewModel? type, ProductTypeItemViewModel? item)
        FindHierarchyBySemiProduct(SemiProductViewModel semi)
    {
        foreach (var product in Products)
        {
            foreach (var type in product.ProductTypes)
            {
                var item = type.ProductTypeItems.FirstOrDefault(i => i.SemiProduct == semi);
                if (item is not null)
                {
                    return (product, type, item);
                }
            }
        }
        return (null, null, null);
    }

    #endregion

    #region Attach/Detach

    private void AttachProduct(ProductViewModel product)
    {
        product.PropertyChanged += Product_PropertyChanged;
        product.ProductTypes.CollectionChanged += ProductTypes_CollectionChanged;

        foreach (var type in product.ProductTypes)
        {
            AttachProductType(type);
        }
    }

    private void DetachProduct(ProductViewModel product)
    {
        product.PropertyChanged -= Product_PropertyChanged;
        product.ProductTypes.CollectionChanged -= ProductTypes_CollectionChanged;

        foreach (var type in product.ProductTypes.ToList())
        {
            DetachProductType(type);
        }
    }

    private void AttachProductType(ProductTypeViewModel type)
    {
        type.PropertyChanged += ProductType_PropertyChanged;
        type.ProductTypeItems.CollectionChanged += ProductTypeItems_CollectionChanged;

        foreach (var item in type.ProductTypeItems)
        {
            AttachSemiProduct(item.SemiProduct);
        }
    }

    private void DetachProductType(ProductTypeViewModel type)
    {
        type.PropertyChanged -= ProductType_PropertyChanged;
        type.ProductTypeItems.CollectionChanged -= ProductTypeItems_CollectionChanged;

        foreach (var item in type.ProductTypeItems.ToList())
        {
            DetachSemiProduct(item.SemiProduct);
        }
    }

    private void AttachSemiProduct(SemiProductViewModel semi)
    {
        semi.PropertyChanged += SemiProduct_PropertyChanged;
    }

    private void DetachSemiProduct(SemiProductViewModel semi)
    {
        semi.PropertyChanged -= SemiProduct_PropertyChanged;
    }

    #endregion

    #region Total Calculation

    private void UpdateInvoiceTotal()
    {
        Invoice.CostPrice = Products?
            .Where(p => !string.IsNullOrWhiteSpace(p.Name))
            .Sum(p => p.ProductTypes?
                .Where(t => !string.IsNullOrWhiteSpace(t.Type))
                .Sum(t => t.ProductTypeItems?
                    .Sum(i => i.SemiProduct?.TotalAmount ?? 0) ?? 0) ?? 0) ?? 0;
    }

    #endregion
}