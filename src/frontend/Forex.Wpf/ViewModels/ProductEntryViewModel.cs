namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.ClientService.Enums;
using Forex.Wpf.Pages.Common;
using System.ComponentModel;

public partial class ProductEntryViewModel : ViewModelBase
{
    public long Id { get; set; }
    [ObservableProperty] private ProductViewModel? product;
    [ObservableProperty] private uint? count;
    [ObservableProperty] private uint availableCount;
    [ObservableProperty] private string productionOriginName = string.Empty;
    [ObservableProperty] private ProductionOrigin productionOrigin;
    [ObservableProperty] private uint? bundleCount;
    [ObservableProperty] private decimal? unitPrice;
    [ObservableProperty] private ProductTypeViewModel? productType;
    [ObservableProperty] private DateTime date = DateTime.Now;
    [ObservableProperty] private uint? bundleItemCount;
    [ObservableProperty] private decimal? costPrice;
    [ObservableProperty] private decimal? preparationCostPerUnit;
    [ObservableProperty] private decimal? totalAmount;

    #region Property Changes

    partial void OnCountChanged(uint? value) => RecalculateBundleCount();
    partial void OnBundleItemCountChanged(uint? value) => RecalculateBundleCount();

    partial void OnProductChanged(ProductViewModel? value)
    {
        if (value is not null)
        {
            ProductionOriginName = value.ProductionOrigin.ToString();
            value.PropertyChanged += OnProductChangedPropertyChanged;
        }
    }

    private void OnProductChangedPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProductViewModel.SelectedType))
            if (Product is not null && Product.SelectedType is not null)
                Product.SelectedType.PropertyChanged += OnProductTypePropertyChanged;
    }

    partial void OnBundleCountChanged(uint? value) => RecalculateCount();

    #endregion Property Changes

    #region Private Helpers

    private void RecalculateCount()
    {
        if (BundleCount is not null && Product is not null && Product.SelectedType is not null && Product.SelectedType.BundleItemCount is not null)
            Count = BundleCount * Product.SelectedType.BundleItemCount;
    }

    private void OnProductTypePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProductType.BundleItemCount))
            RecalculateCount();
    }

    partial void OnProductionOriginNameChanged(string value)
    {
        ProductionOrigin = Enum.TryParse<ProductionOrigin>(ProductionOriginName, out var productionOrigin) ? productionOrigin : ProductionOrigin.Eva;
        if (Product is not null)
            Product!.ProductionOrigin = ProductionOrigin;
    }

    private void RecalculateBundleCount()
    {
        BundleCount = Count / BundleItemCount;
    }

    #endregion
}