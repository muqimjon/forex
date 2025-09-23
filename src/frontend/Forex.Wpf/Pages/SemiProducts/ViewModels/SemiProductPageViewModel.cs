namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.Wpf.Pages.Common;
using System.Collections.ObjectModel;

public partial class SemiProductPageViewModel : ViewModelBase
{
    [ObservableProperty] private InvoiceViewModel invoice = new();
    [ObservableProperty] private ObservableCollection<ProductViewModel> products = [];
    [ObservableProperty] private ObservableCollection<SemiProductViewModel> semiProducts = [];

    [ObservableProperty] private ObservableCollection<UserViewModel> suppliers = [];
    [ObservableProperty] private UserViewModel? selectedSupplier;

    [ObservableProperty] private ObservableCollection<string> productNames = [];
    [ObservableProperty] private ObservableCollection<int> productCodes = [];
    [ObservableProperty] private ObservableCollection<string> semiProductNames = [];

    [ObservableProperty] private ProductViewModel? selectedProduct;


    #region Seeding...

    public void Seeding()
    {
        // 🧑 Fake suppliers
        Suppliers = [
            new() { Id = 1, Name = "Supplier A", Phone = "998901234567", Address = "Toshkent" },
        new() { Id = 2, Name = "Supplier B", Phone = "998907654321", Address = "Samarqand" }
    ];

        // 📦 Fake product names & codes
        ProductNames = ["Un", "Shakar", "Yog‘", "Guruch", "Tuz"];
        ProductCodes = [1001, 1002, 1003, 1004, 1005];

        // ⚙️ Fake semi product names
        SemiProductNames = ["Xamir", "Sous", "Pishloq", "Go‘sht bo‘lagi", "Qaymoq"];

        // 🛠️ Seed Products (5 ta)
        Products =
        [
            new() { Name = "Un", Code = 1001, Quantity = 10, CostPrice = 5000 },
        new() { Name = "Shakar", Code = 1002, Quantity = 15, CostPrice = 6000 },
        new() { Name = "Yog‘", Code = 1003, Quantity = 5, CostPrice = 12000 },
        new() { Name = "Guruch", Code = 1004, Quantity = 20, CostPrice = 8000 },
        new() { Name = "Tuz", Code = 1005, Quantity = 7, CostPrice = 2000 },
    ];

        // 🛠️ Seed SemiProducts (5 ta)
        SemiProducts =
        [
            new() { Name = "Xamir", Quantity = 3, CostPrice = 2000 },
        new() { Name = "Sous", Quantity = 5, CostPrice = 1500 },
        new() { Name = "Pishloq", Quantity = 2, CostPrice = 5000 },
        new() { Name = "Go‘sht bo‘lagi", Quantity = 4, CostPrice = 7000 },
        new() { Name = "Qaymoq", Quantity = 6, CostPrice = 3000 },
    ];
    }

    public ObservableCollection<ProductViewModel> SeedingProducts()
    {
        return new ObservableCollection<ProductViewModel>
    {
        new() { Name="Un", Code=1001, Quantity=10, CostPrice=5000 },
        new() { Name="Shakar", Code=1002, Quantity=15, CostPrice=6000 },
        new() { Name="Yog‘", Code=1003, Quantity=5, CostPrice=12000 },
        new() { Name="Guruch", Code=1004, Quantity=20, CostPrice=8000 },
        new() { Name="Tuz", Code=1005, Quantity=7, CostPrice=2000 }
    };
    }


    #endregion

    #region Commands

    // ➕ Product qo‘shish
    [RelayCommand]
    private void AddProduct()
    {
        Products.Add(new ProductViewModel
        {
            Name = ProductNames.First(),
            Code = ProductCodes.First(),
            Quantity = 1,
            CostPrice = 10000
        });
    }

    // ➕ Yarim tayyor mahsulot qo‘shish
    [RelayCommand]
    private void AddSemiProduct()
    {
        var targetList = SelectedProduct?.SemiProducts ?? SemiProducts;

        targetList.Add(new SemiProductViewModel
        {
            Name = SemiProductNames.First(),
            Quantity = 1,
            CostPrice = 2000
        });
    }

    // ✏️ Mahsulotni tahrirlash
    [RelayCommand]
    private void EditProduct(ProductViewModel? product)
    {
        if (product is null) return;
        InfoMessage = $"{product.Name} tahrirlanmoqda...";
    }

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

    [RelayCommand]
    private void EditSemiProduct(SemiProductViewModel item)
    {
        // Boshqa qatordagi edit-mode’ni o‘chir
        foreach (var row in SemiProducts)
            row.IsEditing = false;

        // Shu qatorni tahrirga och
        if (item != null)
            item.IsEditing = true;
    }

    [RelayCommand]
    private void RemoveSemiProduct(SemiProductViewModel item)
    {
        SemiProducts.Remove(item);
    }

    #endregion


    // 🔄 SemiProductlarni tanlash
    public ObservableCollection<SemiProductViewModel> CurrentSemiProducts =>
        SelectedProduct?.SemiProducts ?? SemiProducts;
}
