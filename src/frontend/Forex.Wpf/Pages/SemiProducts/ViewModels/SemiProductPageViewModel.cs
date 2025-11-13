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

    public SemiProductPageViewModel(IServiceProvider services, IMapper mapper)
    {
        this.services = services;
        this.mapper = mapper;

        Products = new ObservableCollection<ProductViewModel>();
        Products.CollectionChanged += Products_CollectionChanged;
        AddNewProduct(); // Birinchi default product qo'shamiz

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

    #region Product Management

    [RelayCommand]
    private void AddNewProduct()
    {
        var product = new ProductViewModel();
        product.PropertyChanged += Product_PropertyChanged;
        product.ProductTypes.CollectionChanged += (s, e) => HandleProductTypesChanged(product, e);

        // Birinchi default type qo'shamiz
        AddNewProductType(product);

        Products.Add(product);
    }

    [RelayCommand]
    private void RemoveProduct(ProductViewModel product)
    {
        if (Products.Count <= 1)
        {
            WarningMessage = "Kamida bitta mahsulot bo'lishi kerak!";
            return;
        }

        DetachProduct(product);
        Products.Remove(product);
    }

    [RelayCommand]
    private void AddNewProductType(ProductViewModel product)
    {
        var productType = new ProductTypeViewModel();
        productType.ProductTypeItems.CollectionChanged += (s, e) => HandleProductTypeItemsChanged(productType, e);

        // Birinchi default item qo'shamiz
        AddNewProductTypeItem(productType);

        product.ProductTypes.Add(productType);
    }

    [RelayCommand]
    private void RemoveProductType(object[] parameters)
    {
        if (parameters?.Length != 2) return;

        var product = parameters[0] as ProductViewModel;
        var productType = parameters[1] as ProductTypeViewModel;

        if (product == null || productType == null) return;

        if (product.ProductTypes.Count <= 1)
        {
            WarningMessage = "Kamida bitta o'lcham bo'lishi kerak!";
            return;
        }

        DetachProductType(productType);
        product.ProductTypes.Remove(productType);
    }

    [RelayCommand]
    private void AddNewProductTypeItem(ProductTypeViewModel productType)
    {
        var item = new ProductTypeItemViewModel
        {
            SemiProduct = new SemiProductViewModel()
        };
        item.SemiProduct.PropertyChanged += SemiProduct_PropertyChanged;

        productType.ProductTypeItems.Add(item);
    }

    [RelayCommand]
    private void RemoveProductTypeItem(object[] parameters)
    {
        if (parameters?.Length != 2) return;

        var productType = parameters[0] as ProductTypeViewModel;
        var item = parameters[1] as ProductTypeItemViewModel;

        if (productType == null || item == null) return;

        if (productType.ProductTypeItems.Count <= 1)
        {
            WarningMessage = "Kamida bitta element bo'lishi kerak!";
            return;
        }

        DetachSemiProduct(item.SemiProduct);
        productType.ProductTypeItems.Remove(item);
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (!ValidateProducts())
            return;

        var requestObject = new SemiProductIntakeRequest
        {
            Invoice = mapper.Map<InvoiceRequest>(Invoice),
            Products = mapper.Map<ICollection<ProductRequest>>(Products),
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
            WarningMessage = "Default valyuta UZS emas";
            return;
        }

        requestObject.Invoice.CurrencyId = currency.Id;

        var client = services.GetRequiredService<IApiSemiProductEntry>();
        var response = await client.Create(requestObject).Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            SuccessMessage = "Yarim tayyor mahsulot muvaffaqiyatli yuklandi.";
            ResetForm();
        }
        else
        {
            ErrorMessage = response.Message ?? "Yuklashda xatolik yuz berdi.";
        }
    }

    private bool ValidateProducts()
    {
        var validProducts = Products.Where(p => !string.IsNullOrWhiteSpace(p.Name)).ToList();

        if (validProducts.Count == 0)
        {
            ErrorMessage = "Hech qanday yarim tayyor mahsulot kiritilmadi.";
            return false;
        }

        return true;
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

        AddNewProduct();
    }

    #endregion

    #region Event Handlers

    private void Products_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (ProductViewModel product in e.NewItems)
            {
                product.PropertyChanged += Product_PropertyChanged;
            }
        }

        UpdateInvoiceTotal();
        CheckAndAddNewProduct();
    }

    private void HandleProductTypesChanged(ProductViewModel product, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (ProductTypeViewModel type in e.NewItems)
            {
                AttachProductType(type);
            }
        }

        UpdateInvoiceTotal();
        CheckAndAddNewProductType(product);
    }

    private void HandleProductTypeItemsChanged(ProductTypeViewModel productType, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (ProductTypeItemViewModel item in e.NewItems)
            {
                AttachSemiProduct(item.SemiProduct);
            }
        }

        UpdateInvoiceTotal();
        CheckAndAddNewProductTypeItem(productType);
    }

    private void Product_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProductViewModel.Name) ||
            e.PropertyName == nameof(ProductViewModel.Code))
        {
            CheckAndAddNewProduct();
        }

        UpdateInvoiceTotal();
    }

    private void SemiProduct_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(SemiProductViewModel.Quantity) or
            nameof(SemiProductViewModel.CostPrice))
        {
            var semi = sender as SemiProductViewModel;
            if (semi != null)
            {
                CheckAndAddNewProductTypeItemForSemi(semi);
            }

            UpdateInvoiceTotal();
        }
    }

    private void InvoicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Invoice.ViaMiddleman))
        {
            var usdCurrency = AvailableCurrencies.FirstOrDefault(c =>
                c.Code.Equals("USD", StringComparison.InvariantCultureIgnoreCase));

            if (usdCurrency != null)
            {
                Invoice.Currency = usdCurrency;
            }
        }
    }

    #endregion

    #region Auto Add Logic

    private void CheckAndAddNewProduct()
    {
        var lastProduct = Products.LastOrDefault();
        if (lastProduct != null && !string.IsNullOrWhiteSpace(lastProduct.Name))
        {
            AddNewProduct();
        }
    }

    private void CheckAndAddNewProductType(ProductViewModel product)
    {
        var lastType = product.ProductTypes.LastOrDefault();
        if (lastType != null && !string.IsNullOrWhiteSpace(lastType.Type))
        {
            AddNewProductType(product);
        }
    }

    private void CheckAndAddNewProductTypeItem(ProductTypeViewModel productType)
    {
        var lastItem = productType.ProductTypeItems.LastOrDefault();
        if (lastItem?.SemiProduct != null &&
            lastItem.SemiProduct.Quantity > 0 &&
            lastItem.SemiProduct.CostPrice > 0)
        {
            AddNewProductTypeItem(productType);
        }
    }

    private void CheckAndAddNewProductTypeItemForSemi(SemiProductViewModel semi)
    {
        foreach (var product in Products)
        {
            foreach (var productType in product.ProductTypes)
            {
                var item = productType.ProductTypeItems.FirstOrDefault(i => i.SemiProduct == semi);
                if (item != null && semi.Quantity > 0 && semi.CostPrice > 0)
                {
                    var lastItem = productType.ProductTypeItems.LastOrDefault();
                    if (lastItem == item)
                    {
                        AddNewProductTypeItem(productType);
                    }
                    return;
                }
            }
        }
    }

    #endregion

    #region Attach/Detach

    private void AttachProduct(ProductViewModel product)
    {
        product.PropertyChanged += Product_PropertyChanged;

        foreach (var type in product.ProductTypes)
        {
            AttachProductType(type);
        }
    }

    private void DetachProduct(ProductViewModel product)
    {
        product.PropertyChanged -= Product_PropertyChanged;

        foreach (var type in product.ProductTypes.ToList())
        {
            DetachProductType(type);
        }
    }

    private void AttachProductType(ProductTypeViewModel type)
    {
        foreach (var item in type.ProductTypeItems)
        {
            AttachSemiProduct(item.SemiProduct);
        }
    }

    private void DetachProductType(ProductTypeViewModel type)
    {
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