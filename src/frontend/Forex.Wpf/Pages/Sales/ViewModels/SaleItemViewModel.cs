namespace Forex.Wpf.Pages.Sales.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;


public partial class SaleItemViewModel : ViewModelBase
{
    [ObservableProperty] private string? productCode = string.Empty;
    [ObservableProperty] private string? productName = string.Empty;
    [ObservableProperty] private string? type = string.Empty;
    [ObservableProperty] private int? typeCount;
    [ObservableProperty] private int? quantity;
    [ObservableProperty] private decimal? price;
    [ObservableProperty] private decimal? totalAmount;

    [ObservableProperty] private int? countOfType = 1;
    private int? residueOfType = 20;

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
        Quantity = (CountOfType ?? 0) * (TypeCount ?? 0);
        RecalculateTotalAmount();
    }

    private void RecalculateTotalAmount()
    {
        if (Quantity > 0 && Price > 0)
            TotalAmount = (Quantity ?? 0) * (Price ?? 0);
    }
}
