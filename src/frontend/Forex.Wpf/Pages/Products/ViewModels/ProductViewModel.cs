namespace Forex.Wpf.Pages.Products.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using Forex.ClientService.Models.Responses;
using Forex.Wpf.Pages.Common;
using System.Collections.ObjectModel;

public partial class ProductViewModel : ViewModelBase
{
    [ObservableProperty] private string photoPath = string.Empty;

    [ObservableProperty] private ProductResponse? selectedCode;
    [ObservableProperty] private ProductResponse? selectedProduct;
    [ObservableProperty] private ObservableCollection<ProductResponse> products = new();

    [ObservableProperty] private ProductTypeResponse? selectedType;
    [ObservableProperty] private ObservableCollection<ProductTypeResponse> types = new();

    [ObservableProperty] private int countOfType;
    [ObservableProperty] private int typeCount;
    [ObservableProperty] private int totalCount;
    [ObservableProperty] private decimal perPairRate;
    [ObservableProperty] private decimal totalAmount;


    // 🔹 Kod tanlanganda — product ma’lumotlarini to‘ldiramiz
    partial void OnSelectedCodeChanged(ProductResponse? value)
    {
        if (value != null)
        {
            SelectedProduct = value;
            PhotoPath = value.PhotoPath ?? string.Empty;

            // Product uchun razmer turlari (demo uchun misol)
            Types = new ObservableCollection<ProductTypeResponse>(
                value.ProductTypes ?? new List<ProductTypeResponse>()
            );
        }
        else
        {
            SelectedProduct = null;
            PhotoPath = string.Empty;
            Types.Clear();
            CountOfType = 0;
        }
    }

    // Mahsulot tanlanganda - product malumotlarni to'ldiramiz
    partial void OnSelectedProductChanged(ProductResponse? value)
    {
        if (value != null)
        {
            SelectedCode = value;
            PhotoPath = value.PhotoPath ?? string.Empty;

            Types = new ObservableCollection<ProductTypeResponse>(
                value.ProductTypes ?? new List<ProductTypeResponse>()
            );
        }
        else
        {
            SelectedCode = null;
            PhotoPath = string.Empty;
            Types.Clear();
            CountOfType = 0;
        }
    }

    // 🔹 Razmer tanlanganda — razmerga mos countni to‘ldiramiz
    partial void OnSelectedTypeChanged(ProductTypeResponse? value)
    {
        if (value != null)
            CountOfType = value.Count;
        else
            CountOfType = 0;
    }
    partial void OnTotalCountChanged(int value)
    {
        TotalAmount = value * PerPairRate;
    }
    partial void OnPerPairRateChanged(decimal value)
    {
        TotalAmount = value * TotalCount;
    }
    partial void OnCountOfTypeChanged(int value)
    {
        TotalCount = value * TypeCount;
    }
    partial void OnTypeCountChanged(int value)
    {
        TotalCount = value * CountOfType;
    }
}