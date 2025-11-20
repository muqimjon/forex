namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;
using System.ComponentModel;

public partial class ProductEntryViewModel : ViewModelBase
{
    [ObservableProperty] private ProductViewModel? product;
    [ObservableProperty] private ProductTypeViewModel? productType;
    [ObservableProperty] private int count;
    [ObservableProperty] private int availableCount;

    [ObservableProperty] private uint? totalCount;
    [ObservableProperty] private int? bundleCount;

    [ObservableProperty] private decimal? unitPrice;

    #region Property Changes

    partial void OnProductTypeChanged(ProductTypeViewModel? value)
    {
        if (value is not null)
            value.PropertyChanged += OnProductTypePropertyChanged;
    }

    partial void OnBundleCountChanged(int? value) => RecalculateTotalCount();

    #endregion Property Changes

    #region Private Helpers

    private void RecalculateTotalCount()
    {
        if (BundleCount is not null && ProductType is not null && ProductType.BundleItemCount is not null)
            TotalCount = (uint)(BundleCount * ProductType.BundleItemCount);
    }

    private void OnProductTypePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProductType.BundleItemCount))
            RecalculateTotalCount();
    }

    #endregion
}