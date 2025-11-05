namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;

public partial class InvoiceViewModel : ViewModelBase
{
    public long Id { get; set; }
    [ObservableProperty] private ManufactoryViewModel manufactory = default!;
    [ObservableProperty] private UserViewModel supplier = default!;
    [ObservableProperty] private UserViewModel? agent = default!;
    [ObservableProperty] private CurrencyViewModel currency = default!;
    [ObservableProperty] private DateTime date = DateTime.Now;
    [ObservableProperty] private string? number;
    [ObservableProperty] private decimal? costPrice;
    [ObservableProperty] private decimal? costDelivery;
    [ObservableProperty] private int? containerCount;
    [ObservableProperty] private decimal? pricePerUnitContainer;
    [ObservableProperty] private decimal? transferFee;
    [ObservableProperty] private bool viaMiddleman;

    [ObservableProperty] private decimal? totalAmount;
    [ObservableProperty] private string totalAmountWithCurrency = string.Empty;


    #region Property Changes

    partial void OnCostDeliveryChanged(decimal? value) => ReCalculateTotal();
    partial void OnCostPriceChanged(decimal? value) => ReCalculateTotal();
    partial void OnTransferFeeChanged(decimal? value) => ReCalculateTotal();
    partial void OnViaMiddlemanChanged(bool value) => ReCalculateTotal();
    partial void OnContainerCountChanged(int? value) => ReCalculateTransferFee();
    partial void OnPricePerUnitContainerChanged(decimal? value) => ReCalculateTransferFee();
    partial void OnTotalAmountChanged(decimal? value) => RefreshAmount();
    partial void OnCurrencyChanged(CurrencyViewModel value) => RefreshAmount();

    #endregion Property Changes

    #region Private Helpers

    private void RefreshAmount()
    {
        if (TotalAmount is not null && Currency is not null)
            TotalAmountWithCurrency = $"{TotalAmount:N2} {Currency.Code}";
    }

    private void ReCalculateTransferFee()
    {
        TransferFee = ContainerCount * PricePerUnitContainer;
    }

    private void ReCalculateTotal()
    {
        if (CostDelivery is not null || CostPrice is not null || TransferFee is not null)
            if (ViaMiddleman) TotalAmount = (CostPrice ?? 0) + (CostDelivery ?? 0) + (TransferFee ?? 0);
            else TotalAmount = (CostPrice ?? 0) + (CostDelivery ?? 0);
    }

    #endregion Private Helpers
}

