namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.Pages.SemiProducts.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

public partial class SemiProductPageViewModel : ViewModelBase
{
    private readonly ForexClient client;
    private readonly IMapper mapper;

    public SemiProductPageViewModel()
    {
        mapper = App.Mapper;
        client = App.Client;
        _ = LoadPageAsync();
    }

    [ObservableProperty] private InvoiceViewModel invoice = new();
    [ObservableProperty] private ObservableCollection<UnitMeasuerViewModel> availableMeasures = [];

    private ProductTypeViewModel? selectedProductType;
    [ObservableProperty] private ObservableCollection<ProductViewModel> existProducts = [];
    [ObservableProperty] private ObservableCollection<ProductViewModel> products = [];
    [ObservableProperty] private ObservableCollection<ProductTypeViewModel> productTypes = [];
    [ObservableProperty] private string selectedProductInfo = "Barcha yarim tayyor mahsulotlar";

    [ObservableProperty] private ObservableCollection<SemiProductViewModel> semiProducts = [];
    [ObservableProperty] private ObservableCollection<SemiProductViewModel> filteredSemiProducts = [];
    [ObservableProperty] private ObservableCollection<SemiProductViewModel> independentSemiProducts = [];

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

        foreach (var item in SelectedProductType.Items)
        {
            if (item.SemiProduct is not null)
            {
                item.SemiProduct.LinkedItem = item;
                FilteredSemiProducts.Add(item.SemiProduct);
            }
        }
    }

    #endregion Loading Data

    #region Commands

    [RelayCommand]
    private void Submit()
    {
        if (Products.Count == 0)
        {
            ErrorMessage = "Hech qanday mahsulot kiritilmadi.";
            return;
        }

        SuccessMessage = "Kirim muvaffaqiyatli yuborildi!";
        // TODO: Backend API chaqirish
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

    // ==================== Product Commands

    [RelayCommand]
    private void AddProduct()
    {
        ProductTypeViewModel type = new() { Product = new() };
        ProductTypes.Add(type);
        EditProduct(type);

        var typ = type;
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

    // ==================== SemiProduct Commands

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

            SelectedProductType.Items.Add(pti);
        }
        else IndependentSemiProducts.Add(newSemi);

        newSemi.Measure = AvailableMeasures.FirstOrDefault(m => m.IsDefault)
            ?? AvailableMeasures.FirstOrDefault()!;

        EditSemiProduct(newSemi);
        newSemi.PropertyChanged += SemiProduct_PropertyChanged;
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
    private void SaveSemiProduct(SemiProductViewModel item)
        => item.IsEditing = false;

    [RelayCommand]
    private void RemoveSemiProduct(SemiProductViewModel item)
    {
        SemiProducts.Remove(item);
        IndependentSemiProducts?.Remove(item);

        foreach (var type in ProductTypes)
            type.Items = new ObservableCollection<ProductTypeItemViewModel>(
                type.Items.Where(i => i.SemiProduct != item));

        UpdateSemiProductsForSelectedProduct();
    }

    [RelayCommand]
    private void ShowAllSemiProducts()
    {
        SelectedProductType = default!;
        UpdateSemiProductsForSelectedProduct();
        SelectedProductInfo = "Barcha yarim tayyor mahsulotlar";
    }

    #endregion

    #region Helpers Calcs

    private void SemiProduct_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SemiProductViewModel.TotalAmount))
            RecalculateTotalQuantity();
    }

    private void RecalculateTotalQuantity()
    {
        Invoice.CostPrice = FilteredSemiProducts.Sum(x => x.TotalAmount);
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

                if (selectedProductType?.Product?.Code < 1)
                    SelectedProductInfo = $"{SelectedProductType.Product.Name} {SelectedProductType.Product.Code} Yangi mahsulot uchun detallar";
                else SelectedProductInfo = $"{SelectedProductType?.Product.Name} - {SelectedProductType?.Product.Code}";
            }
        }
    }

    #endregion Helpers
}