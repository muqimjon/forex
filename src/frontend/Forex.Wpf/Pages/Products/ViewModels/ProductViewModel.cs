namespace Forex.Wpf.Pages.Products.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using Forex.ClientService.Models.Responses;
using Forex.Wpf.Pages.Common;
using System.Collections.ObjectModel;
using System.Linq;

public partial class ProductViewModel : ViewModelBase
{
    // 🔹 Parent ViewModel (ProductPageViewModel) reference
    public ProductPageViewModel? Parent { get; set; }

    [ObservableProperty] private string photoPath = string.Empty;
    [ObservableProperty] private ProductResponse? selectedCode;
    [ObservableProperty] private ProductResponse? selectedProduct;
    [ObservableProperty] private string name = string.Empty;

    [ObservableProperty] private ObservableCollection<ProductTypeResponse> types = [];
    [ObservableProperty] private ProductTypeResponse? selectedType;

    [ObservableProperty] private int countOfType;
    [ObservableProperty] private int typeCount;
    [ObservableProperty] private int totalCount;
    [ObservableProperty] private decimal perPairRate;
    [ObservableProperty] private decimal totalAmount;


    partial void OnSelectedCodeChanged(ProductResponse? value)
    {
        if (value == null)
        {
            SelectedProduct = null;
            PhotoPath = string.Empty;
            Name = string.Empty;
            Types.Clear();
            CountOfType = 0;
            return;
        }

        SelectedProduct.Name = value.Name;
        Name = value.Name;
        PhotoPath = value.PhotoPath ?? string.Empty;

        // 🔹 Agar Parent bor bo‘lsa — AllTypes dan faqat shu productga tegishlisini olish
        if (Parent?.AllTypes is not null)
        {
            var filtered = Parent.AllTypes
                .Where(t => t.ProductId == value.Id)
                .ToList();
            
            Types = new ObservableCollection<ProductTypeResponse>(filtered);
        }
        else
        {
            // fallback
            Types = new ObservableCollection<ProductTypeResponse>(value.ProductTypes ?? new List<ProductTypeResponse>());
        }
    }

    partial void OnSelectedProductChanged(ProductResponse? value)
    {
        if (value == null)
        {
            SelectedCode = null;
            PhotoPath = string.Empty;
            Name = string.Empty;
            Types.Clear();
            CountOfType = 0;
            return;
        }

        SelectedProduct!.Code = value.Code;
        Name = value.Name;
        PhotoPath = value.PhotoPath ?? string.Empty;

        // 🔹 Agar Parent bor bo‘lsa — AllTypes dan faqat shu productga tegishlisini olish
        if (Parent?.AllTypes is not null)
        {
            var filtered = Parent.AllTypes
                .Where(t => t.ProductId == value.Id)
                .ToList();

            Types = new ObservableCollection<ProductTypeResponse>(filtered);
        }
        else
        {
            // fallback
            Types = new ObservableCollection<ProductTypeResponse>(value.ProductTypes ?? new List<ProductTypeResponse>());
        }

    }
    partial void OnSelectedTypeChanged(ProductTypeResponse? value)
        => CountOfType = value?.Count ?? 0;

    partial void OnTotalCountChanged(int value)
        => TotalAmount = value * PerPairRate;

    partial void OnCountOfTypeChanged(int value) => RecalculateTotals();
    partial void OnTypeCountChanged(int value) => RecalculateTotals();
    partial void OnPerPairRateChanged(decimal value) => RecalculateTotals();

    private void RecalculateTotals()
    {
        TotalCount = CountOfType * TypeCount;
        TotalAmount = TotalCount * PerPairRate;
    }
}
