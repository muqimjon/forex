namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;

public partial class InvoiceViewModel : ViewModelBase
{
    [ObservableProperty] private DateTime entryDate = DateTime.Now;
    [ObservableProperty] private string? number;
    [ObservableProperty] private decimal costPrice;
    [ObservableProperty] private decimal costDelivery;
    [ObservableProperty] private int containerCount;
    [ObservableProperty] private decimal pricePerUnitContainer;
    [ObservableProperty] private decimal transferFee;
    [ObservableProperty] private decimal totalSum;
    [ObservableProperty] private long currencyId;
    [ObservableProperty] private ManufactoryViewModel manufactory = default!;

    [ObservableProperty] private UserViewModel supplier = new();
    [ObservableProperty] private bool viaMiddleman;
    [ObservableProperty] private UserViewModel? sender;

    public decimal TotalCost => CostPrice + CostDelivery + TransferFee;
}

