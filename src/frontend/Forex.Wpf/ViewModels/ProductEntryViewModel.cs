namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;

public partial class ProductEntryViewModel : ViewModelBase
{
    [ObservableProperty] private ProductViewModel? product;
    [ObservableProperty] private ProductTypeViewModel? productType;
    [ObservableProperty] private int count;
    [ObservableProperty] private int bundleItemCount;
    [ObservableProperty] private int availableCount;



    #region Property Changes

    partial void OnProductTypeChanged(ProductTypeViewModel? value)
    {
        BundleItemCount = value?.BundleItemCount ?? 0;
        //AvailableCount = value?.AvailableCount ?? 0;
    }

    #endregion Property Changes
}