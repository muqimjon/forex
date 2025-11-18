namespace Forex.Wpf.Pages.Sales.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;

public partial class SaleItemViewModel : ViewModelBase
{
    [ObservableProperty] private int? bundleCount;
    [ObservableProperty] private int? totalCount;
    [ObservableProperty] private decimal? unitPrice;
    [ObservableProperty] private decimal? amount;

    [ObservableProperty] private ProductViewModel product = default!;
    [ObservableProperty] private ProductTypeViewModel productType = default!;


    #region Property Changes

    partial void OnUnitPriceChanged(decimal? value) => RecalculateTotalAmount();
    partial void OnBundleCountChanged(int? value) => ReCalculateTotalCount();
    partial void OnProductTypeChanged(ProductTypeViewModel value) => ReCalculateTotalCount();
    partial void OnTotalCountChanged(int? value) => RecalculateTotalAmount();

    #endregion Property Changes

    #region Private Helpers

    private void ReCalculateTotalCount()
    {
        if (ProductType is not null)
            TotalCount = ProductType.BundleItemCount * BundleCount;
    }

    private void RecalculateTotalAmount()
    {
        Amount = UnitPrice * TotalCount;
    }

    #endregion Private Helpers
}