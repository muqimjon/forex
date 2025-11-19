namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;
using System.Collections.ObjectModel;

public partial class ProductEntryViewModel : ViewModelBase
{
    [ObservableProperty] private ProductViewModel? product;
    [ObservableProperty] private ProductTypeViewModel? productType;
    [ObservableProperty] private int count;
    [ObservableProperty] private int? bundleItemCount;
    [ObservableProperty] private int availableCount;

    [ObservableProperty] private int? totalCount;
    [ObservableProperty] private int? bundleCount;
    [ObservableProperty] private ObservableCollection<ProductTypeViewModel> availableProductTypes = [];

    [ObservableProperty] private decimal? unitPrice;

    #region Property Changes

    partial void OnProductTypeChanged(ProductTypeViewModel? value)
    {
        BundleItemCount = value?.BundleItemCount ?? 0;
    }

    partial void OnBundleItemCountChanged(int? value) => RecalculateTotalCount();
    partial void OnBundleCountChanged(int? value) => RecalculateTotalCount();

    #endregion Property Changes

    #region Private Helpers

    private void RecalculateTotalCount()
    {
        TotalCount = BundleCount * BundleItemCount;
    }

    #endregion
}