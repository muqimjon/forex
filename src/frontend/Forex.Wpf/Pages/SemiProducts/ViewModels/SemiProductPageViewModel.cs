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

        Products = [new()];
        AttachProducts(Products);

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
        await LoadUnitMeasures();
        await LoadUsersAsync();
        await LoadCurrenciesAsync();
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
        else ErrorMessage = response.Message ?? "Valyuta turlarini yuklashda xatolik";
    }

    private async Task LoadUnitMeasures()
    {
        var client = services.GetRequiredService<IApiUnitMeasures>();
        var response = await client.GetAllAsync().Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
            AvailableUnitMeasures = mapper.Map<ObservableCollection<UnitMeasuerViewModel>>(response.Data);
        else ErrorMessage = response.Message ?? "O'lchov birliklarini yuklashda xatolik";
    }

    private async Task LoadUsersAsync()
    {
        FilteringRequest request = new()
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

        AvailableSuppliers = mapper.Map<ObservableCollection<UserViewModel>>(response.Data!.Where(u => u.Role == UserRole.Taminotchi));
        AvailableAgents = mapper.Map<ObservableCollection<UserViewModel>>(response.Data!.Where(u => u.Role == UserRole.Vositachi));

        Invoice.Supplier = AvailableSuppliers.FirstOrDefault() ?? new();
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (Products.Count == 0)
        {
            ErrorMessage = "Hech qanday yarim tayyor mahsulot kiritilmadi.";
            return;
        }

        var requestObject = new SemiProductIntakeRequest
        {
            Invoice = mapper.Map<InvoiceRequest>(Invoice),
            Products = mapper.Map<ICollection<ProductRequest>>(Products),
            SemiProducts = []
        };

        requestObject.Invoice.CurrencyId = 1; // TODO: Make selectable

        var client = services.GetRequiredService<IApiSemiProductEntry>();
        var response = await client.Create(requestObject).Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            SuccessMessage = "Yarim tayyor mahsulot muvaffaqiyatli yuklandi.";
            Products.Clear();
        }
        else ErrorMessage = response.Message ?? "Yuklashda xatolik yuz berdi.";
    }

    #endregion

    #region Total Calculation

    partial void OnProductsChanged(ObservableCollection<ProductViewModel> value)
    {
        AttachProducts(value);
    }

    private void AttachProducts(ObservableCollection<ProductViewModel> collection)
    {
        collection.CollectionChanged += Products_CollectionChanged;

        foreach (var product in collection)
            AttachProduct(product);
    }

    private void Products_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (ProductViewModel product in e.NewItems)
                AttachProduct(product);
        }

        if (e.OldItems is not null)
        {
            foreach (ProductViewModel product in e.OldItems)
                DetachProduct(product);
        }

        UpdateInvoiceTotal();
    }

    private void AttachProduct(ProductViewModel product)
    {
        product.PropertyChanged += Product_PropertyChanged;
        product.ProductTypes.CollectionChanged += ProductTypes_CollectionChanged;

        foreach (var type in product.ProductTypes)
            AttachProductType(type);
    }

    private void DetachProduct(ProductViewModel product)
    {
        product.PropertyChanged -= Product_PropertyChanged;
        product.ProductTypes.CollectionChanged -= ProductTypes_CollectionChanged;

        foreach (var type in product.ProductTypes)
            DetachProductType(type);
    }

    private void ProductTypes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (ProductTypeViewModel type in e.NewItems)
                AttachProductType(type);
        }

        if (e.OldItems is not null)
        {
            foreach (ProductTypeViewModel type in e.OldItems)
                DetachProductType(type);
        }

        UpdateInvoiceTotal();
    }

    private void AttachProductType(ProductTypeViewModel type)
    {
        type.ProductTypeItems.CollectionChanged += ProductTypeItems_CollectionChanged;

        foreach (var item in type.ProductTypeItems)
            AttachSemiProduct(item.SemiProduct);
    }

    private void DetachProductType(ProductTypeViewModel type)
    {
        type.ProductTypeItems.CollectionChanged -= ProductTypeItems_CollectionChanged;

        foreach (var item in type.ProductTypeItems)
            DetachSemiProduct(item.SemiProduct);
    }

    private void ProductTypeItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (ProductTypeItemViewModel item in e.NewItems)
                AttachSemiProduct(item.SemiProduct);
        }

        if (e.OldItems is not null)
        {
            foreach (ProductTypeItemViewModel item in e.OldItems)
                DetachSemiProduct(item.SemiProduct);
        }

        UpdateInvoiceTotal();
    }

    private void AttachSemiProduct(SemiProductViewModel semi)
    {
        semi.PropertyChanged += SemiProduct_PropertyChanged;
    }

    private void DetachSemiProduct(SemiProductViewModel semi)
    {
        semi.PropertyChanged -= SemiProduct_PropertyChanged;
    }

    private void Product_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProductViewModel.ProductTypes))
        {
            var product = sender as ProductViewModel;
            if (product is not null)
            {
                product.ProductTypes.CollectionChanged += ProductTypes_CollectionChanged;
                foreach (var type in product.ProductTypes)
                    AttachProductType(type);
            }
        }

        UpdateInvoiceTotal();
    }

    private void SemiProduct_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(SemiProductViewModel.Quantity)
            or nameof(SemiProductViewModel.CostPrice)
            or nameof(SemiProductViewModel.TotalAmount))
        {
            UpdateInvoiceTotal();
        }
    }

    private void UpdateInvoiceTotal()
    {
        Invoice.CostPrice = Products?.Sum(p =>
            p.ProductTypes?.Sum(t =>
                t.ProductTypeItems?.Sum(i => i.SemiProduct.TotalAmount)
            ) ?? 0
        ) ?? 0;
    }

    #endregion
}
