namespace Forex.Wpf.Pages.Sales.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;

public partial class SaleItemViewModel : ViewModelBase
{
    [ObservableProperty] private string? productCode = string.Empty;
    [ObservableProperty] private string? productName = string.Empty;
    [ObservableProperty] private string? type = string.Empty;
    [ObservableProperty] private int? typeCount;  // razmerlardan nechadan sotilayotgani 1 razmerda 6 ta ayoq kiyim bor
    [ObservableProperty] private int? count;   // jami necha dona sotilayotgani typeCount * countOfType yoki typeCount * 6
    [ObservableProperty] private decimal? price;
    [ObservableProperty] private decimal? totalAmount;
    [ObservableProperty] private ProductViewModel? product;

    [ObservableProperty] private int? countOfType = 6;
    private readonly int? residueOfType = 50;

    partial void OnCountOfTypeChanged(int? value)
        => RecalculateQuantity();

    partial void OnTypeCountChanged(int? oldValue, int? newValue)
    {
        if (newValue > residueOfType)
        {
            WarningMessage = $" Omborda {residueOfType} ta qoldiq mavjud";
            TypeCount = oldValue;
        }
        else
            RecalculateQuantity();
    }

    partial void OnPriceChanged(decimal? value)
        => RecalculateTotalAmount();


    private void RecalculateQuantity()
    {
        Count = (CountOfType ?? 0) * (TypeCount ?? 0);
        RecalculateTotalAmount();
    }

    private void RecalculateTotalAmount()
    {
        if (Count > 0 && Price > 0)
            TotalAmount = (Count ?? 0) * (Price ?? 0);
    }
}
