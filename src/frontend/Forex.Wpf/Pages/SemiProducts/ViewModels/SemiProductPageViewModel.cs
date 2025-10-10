namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.Wpf.Pages.Common;
using System.Collections.ObjectModel;

public partial class SemiProductPageViewModel : ViewModelBase
{
    [ObservableProperty] private InvoiceViewModel invoice = new();
    [ObservableProperty] private UserViewModel? selectedSupplier;
    [ObservableProperty] private ObservableCollection<UserViewModel> suppliers = [
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


    [ObservableProperty] private bool showQuantityColumn;
    [ObservableProperty] private ObservableCollection<ProductTypeItemViewModel> items = [];
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
                ShowQuantityColumn = true;
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

            ShowQuantityColumn = false;
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
            new() { Name = "Xamir", CostPrice = 2000, Measure = AvailableMeasures[0], Code = 2001, TotalQuantity = 6542 },
        new() { Name = "Sous", CostPrice = 1500, Measure = AvailableMeasures[0], Code = 2002, TotalQuantity = 80020 },
        new() { Name = "Pishloq", CostPrice = 5000, Measure = AvailableMeasures[0], Code = 2003, TotalQuantity = 2400 },
        new() { Name = "Go‘sht bo‘lagi", CostPrice = 7000, Measure = AvailableMeasures[0], Code = 2004, TotalQuantity = 5648 },
        new() { Name = "Qaymoq", CostPrice = 3000, Measure = AvailableMeasures[0], Code = 2005, TotalQuantity = 5648 },
        new() { Name = "Tuxumli qatlam", CostPrice = 3500, Measure = AvailableMeasures[0], Code = 2006, TotalQuantity = 5648 }
        ];

        // 🧱 Mahsulot turlari (ProductType)
        ProductTypes =
        [
            new()
        {
            Type = "Oddiy Pizza",
            Quantity = 10,
            Items =
            [
                new() { SemiProduct = SemiProducts[0], Quantity = 2 }, // Xamir
                new() { SemiProduct = SemiProducts[1], Quantity = 1 }, // Sous
                new() { SemiProduct = SemiProducts[2], Quantity = 1 }  // Pishloq
            ]
        },
        new()
        {
            Type = "Burger Max",
            Quantity = 5,
            Items =
            [
                new() { SemiProduct = SemiProducts[3], Quantity = 2 }, // Go‘sht bo‘lagi
                new() { SemiProduct = SemiProducts[4], Quantity = 1 }  // Qaymoq
            ]
        }
        ];

        // 🧾 Mahsulotlar
        Products =
        [
            new() { Code = 1001, Name = "Pizza", Measure = AvailableMeasures[0], Types = [ProductTypes[0]] },
        new() { Code = 1002, Name = "Burger", Measure = AvailableMeasures[0], Types = [ProductTypes[1]] },
        new() { Code = 1003, Name = "Ichimlik", Measure = AvailableMeasures[2], Types = [] }
        ];

        // 🔗 Bog‘lash: ProductType → Product
        ProductTypes[0].Product = Products[0];
        ProductTypes[1].Product = Products[1];

        // 📦 Invoice
        Invoice = new InvoiceViewModel
        {
            EntryDate = DateTime.Today,
            Number = "INV-2025-001",
            CostPrice = 120000,
            CostDelivery = 15000,
            TransferFee = 5000,
            CurrencyId = 1,
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

    // ➕ Product qo‘shish
    [RelayCommand]
    private void AddProduct()
    {
        ProductTypeViewModel type = new() { Product = new() };
        ProductTypes.Add(type);
        EditProduct(type);
    }

    // ✏️ Mahsulotni tahrirlash
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

    // ✅ Mahsulotni saqlash
    [RelayCommand]
    private void SaveProduct(ProductTypeViewModel? item)
    {
        if (item is not null)
            item.IsEditing = false;
    }

    // ❌ M ahsulotni o‘chirish
    [RelayCommand]
    private void RemoveProduct(ProductTypeViewModel item)
    {
        ProductTypes.Remove(item);
    }
    #endregion Product Commands

    #region SemiProduct Commands

    // ➕ Yarim tayyor mahsulot qo‘shish
    [RelayCommand]
    private void AddSemiProduct()
    {
        var newSemi = new SemiProductViewModel();

        if (SelectedProductType is not null)
        {
            var pti = new ProductTypeItemViewModel { SemiProduct = newSemi };
            newSemi.LinkedItem = pti;
            SelectedProductType.Items.Add(pti);
        }
        EditSemiProduct(newSemi);
        FilteredSemiProducts.Add(newSemi);
    }

    // ✏️ Yarim tayyor mahsulotni tahrirlash
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

    // ✅ Yarim tayyor mahsulotni saqlash
    [RelayCommand]
    private void SaveSemiProduct(SemiProductViewModel item)
    {
        if (item is not null)
            item.IsEditing = false;
    }

    // ❌ Yarim tayyor mahsulotni o‘chirish
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
    }

    #endregion SemiProduct Commands

    // ✅ Saqlash / Yuborish
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

    #endregion
}
