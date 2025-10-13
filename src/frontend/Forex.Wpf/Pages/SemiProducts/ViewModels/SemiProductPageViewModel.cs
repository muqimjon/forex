namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.Pages.SemiProducts.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

public partial class SemiProductPageViewModel : ViewModelBase
{
    [ObservableProperty] private InvoiceViewModel invoice = new();
    [ObservableProperty] private ManufactoryViewModel? selectedManufactory;
    [ObservableProperty] private string selectedProductInfo = "Barcha yarim tayyor mahsulotlar";

    [ObservableProperty]
    private ObservableCollection<UserViewModel> suppliers = [
            new() { Id = 1, Name = "Supplier A", Phone = "998901234567", Address = "Toshkent", Email = "a@supplier.uz", Description = "Mahalliy yetkazuvchi" },
        new() { Id = 2, Name = "Supplier B", Phone = "998907654321", Address = "Samarqand", Email = "b@supplier.uz", Description = "Viloyat yetkazuvchi" }
        ];

    public ObservableCollection<UnitMeasuerViewModel> AvailableMeasures { get; } =
        [
            new() { Id = 1, Name = "Dona", Description = "Soni bilan o‘lchanadi" },
            new() { Id = 2, Name = "Kg", Description = "Og‘irlik birligi" },
            new() { Id = 3, Name = "Litr", Description = "Hajm birligi" }
        ];

    public List<ManufactoryViewModel> Manufactories =
        [
            new() { Id = 1, Name = "Farg‘ona Non Sexi" },
        new() { Id = 2, Name = "Qo‘qon Ichimlik Zavodi" }
        ];


    [ObservableProperty] private ObservableCollection<ProductViewModel> products = [];
    [ObservableProperty] private ObservableCollection<ProductTypeViewModel> productTypes = [];



    [ObservableProperty] private decimal totalAmount;
    [ObservableProperty] private ObservableCollection<SemiProductViewModel> semiProducts = [];
    [ObservableProperty] private ObservableCollection<SemiProductViewModel> filteredSemiProducts = [];


    private ProductTypeViewModel? selectedProductType;
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
            }
        }
    }

    public void UpdateSemiProductsForSelectedProduct()
    {
        FilteredSemiProducts.Clear();

        if (SelectedProductType is null)
        {
            foreach (var semi in SemiProducts)
                FilteredSemiProducts.Add(semi);

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

    #region Seeding...

    public void Seeding()
    {
        // 🧩 Yarim tayyor mahsulotlar
        SemiProducts =
        [
            new() { Name = "Tufli padoji", CostPrice = 2000, Measure = AvailableMeasures[0], Quantity = 6542 },
            new() { Name = "qora guli", CostPrice = 1500, Measure = AvailableMeasures[0], Quantity = 80020 },
            new() { Name = "sariq guli", CostPrice = 5000, Measure = AvailableMeasures[0], Quantity = 2400 },
            new() { Name = "qishki padoj", CostPrice = 7000, Measure = AvailableMeasures[0], Quantity = 5648 },
            new() { Name = "yozgi tapichka usti", CostPrice = 3000, Measure = AvailableMeasures[0], Quantity = 5648 },
            new() { Name = "kuzgi tapichka usti", CostPrice = 3500, Measure = AvailableMeasures[0], Quantity = 5648 }
        ];

        // 🧱 Mahsulot turlari (ProductType)
        ProductTypes =
        [
            new()
        {
            Type = "24-29",
            Items =
            [
                new() { SemiProduct = SemiProducts[0], Quantity = 2 }
            ]
        },
        new()
        {
            Type = "30-35",
            Items =
            [
                new() { SemiProduct = SemiProducts[3], Quantity = 2 }
            ]
        },
        new()
        {
            Type = "22-27",
            Items =
            [
                new() { SemiProduct = SemiProducts[3], Quantity = 2 }
            ]
        }
        ];

        // 🧾 Mahsulotlar
        Products =
        [
            new() { Code = 1001, Name = "Tufli", Measure = AvailableMeasures[0], Types = [ProductTypes[0], ProductTypes[1], ProductTypes[2]] },
            new() { Code = 1002, Name = "X-tapichka", Measure = AvailableMeasures[0], Types = [ProductTypes[1], ProductTypes[2], ProductTypes[0]] },
            new() { Code = 1003, Name = "Gulli tufli", Measure = AvailableMeasures[1], Types = [ProductTypes[0], ProductTypes[2]] }
        ];

        // 🔗 Bog‘lash: ProductType → Product
        ProductTypes[0].Product = Products[0];
        ProductTypes[1].Product = Products[1];
        ProductTypes[2].Product = Products[2];

        // 📦 Invoice
        Invoice = new InvoiceViewModel
        {
            EntryDate = DateTime.Today,
            Number = "INV-2025-001",
            CostPrice = 120000,
            CostDelivery = 15000,
            TransferFee = 5000,
            Currency = new CurrencyViewModel { Name = "So'm", Code = "UZS" },
            TotalSum = 140000,
            ViaMiddleman = false,
            Supplier = Suppliers[0],
            Sender = Suppliers[1],
            Manufactory = Manufactories[0]
        };

        // 🔄 Filterlash (agar kerak bo‘lsa)
        UpdateSemiProductsForSelectedProduct();
    }

    #endregion

    #region Commands

    #region Product Commands

    [RelayCommand]
    private void AddProduct()
    {
        ProductTypeViewModel type = new() { Product = new() };
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
    private void SaveProduct(ProductTypeViewModel? item)
    {
        if (item is not null)
            item.IsEditing = false;
    }

    [RelayCommand]
    private void RemoveProduct(ProductTypeViewModel item)
    {
        ProductTypes.Remove(item);
    }

    #endregion Product Commands

    #region SemiProduct Commands

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

        EditSemiProduct(newSemi);
        newSemi.PropertyChanged += SemiProduct_PropertyChanged;
        FilteredSemiProducts.Add(newSemi);
    }

    private void SemiProduct_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SemiProductViewModel.TotalAmount))
            RecalculateTotalQuantity();
    }

    private void RecalculateTotalQuantity()
    {
        TotalAmount = FilteredSemiProducts.Sum(x => x.TotalAmount);
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
    {
        if (item is not null)
            item.IsEditing = false;
    }

    [RelayCommand]
    private void RemoveSemiProduct(SemiProductViewModel item)
    {
        SemiProducts.Remove(item);

        foreach (var types in ProductTypes)
        {
            var toRemove = types.Items.Where(i => i.SemiProduct == item).ToList();
            foreach (var i in toRemove)
                types.Items.Remove(i);
        }

        UpdateSemiProductsForSelectedProduct();
    }

    [RelayCommand]
    private void ShowAllSemiProducts()
    {
        SelectedProductType = default!;
        UpdateSemiProductsForSelectedProduct();
        SelectedProductInfo = "Barcha yarim tayyor mahsulotlar";
    }

    #endregion SemiProduct Commands

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
    #endregion
}