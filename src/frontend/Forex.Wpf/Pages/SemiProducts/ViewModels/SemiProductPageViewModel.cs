namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.Pages.SemiProducts.Views;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

public partial class SemiProductPageViewModel : ViewModelBase
{
    private readonly ForexClient client;
    private readonly IMapper mapper;

    public SemiProductPageViewModel(ForexClient client, IMapper mapper)
    {
        this.client = client;
        this.mapper = mapper;
        invoice = new(client, mapper);

        _ = LoadPageAsync();
        AddProduct();
        GetSelectedProductInfo(null);
    }

    [ObservableProperty] private InvoiceViewModel invoice;
    [ObservableProperty] private ObservableCollection<UnitMeasuerViewModel> availableMeasures = [];

    private ProductTypeViewModel? selectedProductType;
    [ObservableProperty] private ObservableCollection<ProductViewModel> existProducts = [];
    [ObservableProperty] private ObservableCollection<ProductViewModel> products = [];
    [ObservableProperty] private ObservableCollection<ProductTypeViewModel> productTypes = [];
    [ObservableProperty] private string selectedProductInfo = string.Empty;

    [ObservableProperty] private ObservableCollection<SemiProductViewModel> semiProducts = [];
    [ObservableProperty] private ObservableCollection<SemiProductViewModel> filteredSemiProducts = [];
    [ObservableProperty] private ObservableCollection<SemiProductViewModel> independentSemiProducts = [];

    #region Commands

    [RelayCommand]
    private void AddProduct()
    {
        ProductTypeViewModel type = new() { Product = new() };
        type.Product.PropertyChanged += ProductPropertyChanged;
        type.PropertyChanged += ProductTypePropertyChanged;
        ProductTypes.Add(type);
        EditProduct(type);
    }

    [RelayCommand]
    private void EditProduct(ProductTypeViewModel? item)
    {
        foreach (var row in ProductTypes)
            row.IsEditing = false;

        foreach (var row in SemiProducts)
            row.IsEditing = false;

        if (item is not null)
            item.IsEditing = true;
    }

    [RelayCommand]
    private static void SaveProduct(ProductTypeViewModel? item)
    {
        if (item is not null)
            item.IsEditing = false;
    }

    [RelayCommand]
    private void RemoveProduct(ProductTypeViewModel item)
    {
        ProductTypes.Remove(item);
    }

    [RelayCommand]
    private void AddSemiProduct()
    {
        var newSemi = new SemiProductViewModel();

        if (SelectedProductType is not null)
        {
            var pti = new ProductTypeItemViewModel
            {
                SemiProduct = newSemi,
                ParentType = SelectedProductType
            };
            newSemi.LinkedItem = pti;

            SelectedProductType.ProductTypeItems.Add(pti);
        }
        else IndependentSemiProducts.Add(newSemi);

        newSemi.UnitMeasure = AvailableMeasures.FirstOrDefault(m => m.IsDefault)
            ?? AvailableMeasures.FirstOrDefault()!;

        EditSemiProduct(newSemi);
        newSemi.PropertyChanged += SemiProductPropertyChanged;
        SemiProducts.Add(newSemi);
        UpdateSemiProductsForSelectedProduct();
    }

    [RelayCommand]
    private void EditSemiProduct(SemiProductViewModel item)
    {
        foreach (var row in SemiProducts)
            row.IsEditing = false;

        foreach (var row in ProductTypes)
            row.IsEditing = false;

        if (item is not null)
            item.IsEditing = true;
    }

    [RelayCommand]
    private static void SaveSemiProduct(SemiProductViewModel item)
        => item.IsEditing = false;

    [RelayCommand]
    private void RemoveSemiProduct(SemiProductViewModel item)
    {
        SemiProducts.Remove(item);
        IndependentSemiProducts?.Remove(item);

        foreach (var type in ProductTypes)
            type.ProductTypeItems = new ObservableCollection<ProductTypeItemViewModel>(
                type.ProductTypeItems.Where(i => i.SemiProduct != item));

        UpdateSemiProductsForSelectedProduct();
    }

    [RelayCommand]
    private void ShowAllSemiProducts()
    {
        SelectedProductType = default!;
        UpdateSemiProductsForSelectedProduct();
        SelectedProductInfo = "Barcha yarim tayyor mahsulotlar";
    }

    private Window? semiProductsWindow;
    [RelayCommand]
    private void ShowReportInPopup()
    {
        if (semiProductsWindow is null)
        {
            semiProductsWindow = new Window
            {
                Title = "Yarim tayyor mahsulotlar ro‘yxati",
                Content = new CheckSemiProductsPage(this)
            };

            semiProductsWindow.Closing += (s, e) =>
            {
                e.Cancel = true;
                semiProductsWindow.Hide();
            };
        }

        semiProductsWindow.Width = 1000;
        semiProductsWindow.Height = 700;
        semiProductsWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

        if (!semiProductsWindow.IsVisible)
            semiProductsWindow.Show();
        else
            semiProductsWindow.Activate();
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (SemiProducts.Count == 0)
        {
            ErrorMessage = "Hech qanday yarim tayyor mahsulot kiritilmadi.";
            return;
        }

        var requestObject = new SemiProductIntakeRequest
        {
            Invoice = mapper.Map<InvoiceRequest>(Invoice),
            SemiProducts = mapper.Map<ICollection<SemiProductRequest>>(IndependentSemiProducts),
            Products = mapper.Map<ICollection<ProductRequest>>(Products)
        };

        var response = await client.SemiProductEntry.Create(requestObject)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            SuccessMessage = "Yarim tayyor mahsulot muvaffaqiyatli yuklandi.";

            semiProductsWindow?.Close();
            semiProductsWindow = null;
            SemiProducts = [];
            Products = [];

            await LoadPageAsync();
        }
        else
            ErrorMessage = response.Message ?? "Yuklashda xatolik yuz berdi.";
    }

    #endregion

    #region Loading Data

    private async Task LoadPageAsync()
    {
        await LoadUnitMeasuresAsync();
        await LoadingProductsAsync();
    }

    private async Task LoadUnitMeasuresAsync()
    {
        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["isactive"] = ["true"],
                ["isdeleted"] = ["false"]
            }
        };

        var response = await client.UnitMeasure.Filter(request)
            .Handle(isLoading => IsLoading = isLoading);


        if (!response.IsSuccess)
        {
            ErrorMessage = response.Message ?? "Foydalanuvchilarni yuklashda noma'lum xatolik yuz berdi.";
            return;
        }

        AvailableMeasures = mapper.Map<ObservableCollection<UnitMeasuerViewModel>>(response.Data!);
    }

    private async Task LoadingProductsAsync()
    {
        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["type"] = ["include:items.semiProduct"]
            }
        };

        var response = await client.Products.Filter(request).Handle(isLoading => IsLoading = isLoading);
        if (!response.IsSuccess)
        {
            ErrorMessage = response.Message ?? "Mahsulotlarni yuklashda noma'lum xatolik yuz berdi.";
            return;
        }

        ExistProducts = mapper.Map<ObservableCollection<ProductViewModel>>(response.Data!);
    }

    public void UpdateSemiProductsForSelectedProduct()
    {
        FilteredSemiProducts.Clear();

        if (SelectedProductType is null)
        {
            foreach (var semi in SemiProducts)
                FilteredSemiProducts.Add(semi);

            SelectedProductInfo = "Barcha yarim tayyor mahsulotlar";
            return;
        }

        foreach (var item in SelectedProductType.ProductTypeItems)
        {
            if (item.SemiProduct is not null)
            {
                item.SemiProduct.LinkedItem = item;
                FilteredSemiProducts.Add(item.SemiProduct);
            }
        }
    }

    #endregion Loading Data

    #region Property Changes

    private void ProductTypePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ProductTypeViewModel productType)
            return;

        if (e.PropertyName == nameof(ProductTypeViewModel.Product))
        {
            GetSelectedProductInfo(productType.Product);
        }
    }

    private void SemiProductPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SemiProductViewModel.TotalAmount))
            RecalculateTotalQuantity();
    }

    private void ProductPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ProductViewModel product)
            return;

        GetSelectedProductInfo(product);
    }

    public ProductTypeViewModel SelectedProductType
    {
        get => selectedProductType!;
        set
        {
            if (selectedProductType != value)
            {
                selectedProductType = value;
                OnPropertyChanged();
                UpdateSemiProductsForSelectedProduct();
                GetSelectedProductInfo(SelectedProductType?.Product);
            }
        }
    }

    #endregion Property Changes

    #region Privat Helpers

    private void RecalculateTotalQuantity()
    {
        Invoice.CostPrice = SemiProducts.Sum(x => x.TotalAmount);
    }

    private void GetSelectedProductInfo(ProductViewModel? product)
    {
        if (product is null || string.IsNullOrEmpty(product.Name))
        {
            SelectedProductInfo = "Barcha yarim tayyor mahsulotlar";
            return;
        }

        SelectedProductInfo = $"{product.Name} {product.Code} mahsulot uchun detallar";
    }

    #endregion Helpers
}