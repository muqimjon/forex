namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.Wpf.Pages.Common;
using System.Collections.ObjectModel;

public partial class SemiProductPageViewModel : ViewModelBase
{
    [ObservableProperty] private InvoiceViewModel invoice = new();

    [ObservableProperty] private ObservableCollection<UserViewModel> suppliers = [];
    [ObservableProperty] private UserViewModel? selectedSupplier;

    private ProductViewModel? selectedProduct;
    [ObservableProperty] private ObservableCollection<ProductViewModel> products = [];
    [ObservableProperty] private ObservableCollection<ProductViewModel> productTypeItem = [];
    [ObservableProperty] private ObservableCollection<SemiProductViewModel> semiProducts = [];

    [ObservableProperty] private ObservableCollection<ProductTypeItemViewModel> items = [];
    [ObservableProperty] private ObservableCollection<SemiProductViewModel> filteredSemiProducts = [];


    [ObservableProperty] private bool showQuantityColumn;


    public ProductViewModel SelectedProduct
    {
        get => selectedProduct;
        set
        {
            if (selectedProduct != value)
            {
                selectedProduct = value;
                OnPropertyChanged();
                ShowQuantityColumn = true;
                UpdateSemiProductsForSelectedProduct();
            }
        }
    }

    private void UpdateSemiProductsForSelectedProduct()
    {
        FilteredSemiProducts.Clear();

        if (SelectedProduct is null)
        {
            foreach (var semi in SemiProducts)
            {
                semi.LinkedItem = default!;
                semi.LinkedQuantity = default;
                FilteredSemiProducts.Add(semi);
            }

            ShowQuantityColumn = false;
            return;
        }

        foreach (var item in SelectedProduct!.Items)
        {

            if (item.SemiProduct is not null)
            {
                item.SemiProduct.LinkedQuantity = item.Quantity;
                item.SemiProduct.LinkedItem = item;
                FilteredSemiProducts.Add(item.SemiProduct);
            }
        }
    }

    #region Seeding...

    public void Seeding()
    {
        Suppliers =
    [
        new() { Id = 1, Name = "Supplier A", Phone = "998901234567", Address = "Toshkent", Email = "a@supplier.uz", Description = "Mahalliy yetkazuvchi" },
        new() { Id = 2, Name = "Supplier B", Phone = "998907654321", Address = "Samarqand", Email = "b@supplier.uz", Description = "Viloyat yetkazuvchi" }
    ];

        Products =
    [
        new() { Name = "Un", Code = 1001, Quantity = 800, Type = "24 - 29", Measure = new() { Name = "Dona" } },
        new() { Name = "Shakar", Code = 1002, Quantity = 600, Type = "24 - 29", Measure = new() { Name = "Dona" } },
        new() { Name = "Yog‘", Code = 1003, Quantity = 200, Type = "24 - 29", Measure = new() { Name = "Dona" } },
        new() { Name = "Guruch", Code = 1004, Quantity = 150, Type = "30-35", Measure = new() { Name = "Dona" } },
        new() { Name = "Tuz", Code = 1005, Quantity = 500, Type = "24 - 29", Measure = new() { Name = "Dona" } },
        new() { Name = "Makaron", Code = 1006, Quantity = 180, Type = "24 - 29", Measure = new() { Name = "Dona" } }
    ];

        SemiProducts =
    [
        new() { Name = "Xamir", LinkedQuantity = 3, CostPrice = 2000, Measure = new() { Name = "Dona" }, Code = 1003, TotalQuantity = 6542 },
        new() { Name = "Sous", LinkedQuantity = 5, CostPrice = 1500, Measure = new() { Name = "Dona" }, Code = 1004, TotalQuantity = 80020 },
        new() { Name = "Pishloq", LinkedQuantity = 2, CostPrice = 5000, Measure = new() { Name = "Dona" }, Code = 1005, TotalQuantity = 2400 },
        new() { Name = "Go‘sht bo‘lagi", LinkedQuantity = 4, CostPrice = 7000, Measure = new() { Name = "Dona" }, Code = 1006, TotalQuantity = 5648 },
        new() { Name = "Qaymoq", LinkedQuantity = 6, CostPrice = 3000, Measure = new() { Name = "Dona" }, Code = 1006, TotalQuantity = 5648 },
        new() { Name = "Tuxumli qatlam", LinkedQuantity = 8, CostPrice = 3500, Measure = new() { Name = "Dona" }, Code = 1006, TotalQuantity = 5648 }
    ];

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
            Manufactory = new ManufactoryViewModel { Id = 1, Name = "Farg‘ona Non Sexi" }
        };
    }


    #endregion

    #region Commands

    #region Product Commands

    // ➕ Product qo‘shish
    [RelayCommand]
    private void AddProduct()
    {
        ProductViewModel product = new();
        Products.Add(product);
        EditProduct(product);
    }

    // ✏️ Mahsulotni tahrirlash
    [RelayCommand]
    private void EditProduct(ProductViewModel? item)
    {
        foreach (var row in Products)
            row.IsEditing = false;

        foreach (var row in SemiProducts)
            row.IsEditing = false;

        if (item is not null)
            item.IsEditing = true;
    }

    // ✅ Mahsulotni saqlash
    [RelayCommand]
    private void SaveProduct(ProductViewModel? item)
    {
        if (item is not null)
            item.IsEditing = false;
    }

    // ❌ Yarim tayyor mahsulotni o‘chirish
    [RelayCommand]
    private void RemoveProduct(ProductViewModel item)
    {
        Products.Remove(item);
    }

    #endregion Product Commands

    #region SemiProduct Commands

    // ➕ Yarim tayyor mahsulot qo‘shish
    [RelayCommand]
    private void AddSemiProduct()
    {
        var newSemi = new SemiProductViewModel();

        if (SelectedProduct is not null)
        {
            var pti = new ProductTypeItemViewModel { SemiProduct = newSemi };
            SelectedProduct.Items.Add(pti);
        }
        EditSemiProduct(newSemi);
        SemiProducts.Add(newSemi);
        UpdateSemiProductsForSelectedProduct();
    }

    // ✏️ Yarim tayyor mahsulotni tahrirlash
    [RelayCommand]
    private void EditSemiProduct(SemiProductViewModel item)
    {
        foreach (var row in SemiProducts)
            row.IsEditing = false;

        foreach (var row in Products)
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

        foreach (var product in Products)
        {
            var toRemove = product.Items.Where(i => i.SemiProduct == item).ToList();
            foreach (var i in toRemove)
                product.Items.Remove(i);
        }

        UpdateSemiProductsForSelectedProduct();
    }

    [RelayCommand]
    private void ShowAllSemiProducts()
    {
        SelectedProduct = default!;
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
