//namespace Forex.Wpf.ViewModels;

//using CommunityToolkit.Mvvm.ComponentModel;
//using Forex.Wpf.Pages.Common;
//using System.Collections.ObjectModel;

//public partial class ProductEntryViewModel : ViewModelBase
//{
//    public long ProductTypeId { get; set; }
//    public long ProductId { get; set; }
//    public long Id { get; set; }
//    [ObservableProperty] private int? bundleCount;
//    [ObservableProperty] private int? totalCount;
//    [ObservableProperty] private decimal? preparationCostPerUnit;
//    [ObservableProperty] private decimal? totalAmount;

//    [ObservableProperty] private ProductViewModel product = default!;
//    [ObservableProperty] private ProductTypeViewModel productType = default!;
//    [ObservableProperty] private UserViewModel employee = default!;

//    [ObservableProperty] private ObservableCollection<ProductTypeViewModel> availableProductTypes = [];


//    #region Property Changes

//    partial void OnBundleCountChanged(int? value) => CalculateTotalCount();
//    partial void OnTotalCountChanged(int? value) => CalculateTotalAmount();
//    partial void OnPreparationCostPerUnitChanged(decimal? value) => CalculateTotalAmount();

//    #endregion Property Changes

//    #region Private Helpers

//    private void CalculateTotalCount()
//    {
//        if (BundleCount is null || ProductType is null)
//        {
//            TotalCount = null;
//            return;
//        }

//        TotalCount = BundleCount * ProductType.Count;
//    }

//    private void CalculateTotalAmount()
//    {
//        if (PreparationCostPerUnit is null || TotalCount is null)
//        {
//            TotalAmount = null;
//            return;
//        }

//        TotalAmount = PreparationCostPerUnit * TotalCount;
//    }

//    #endregion Private Helpers
//}


namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.Pages.Products.ViewModels;
using System.Collections.ObjectModel;

public partial class ProductEntryViewModel : ViewModelBase
{
    private ProductPageViewModel? _parentViewModel;
    [ObservableProperty] private ProductViewModel? product;
    [ObservableProperty] private ProductTypeViewModel? productType;
    [ObservableProperty] private decimal quantity = 1;
    [ObservableProperty] private ObservableCollection<ProductTypeViewModel> availableProductTypes = [];
    [ObservableProperty] private ObservableCollection<ProductTypeItemViewModel> availableSemiProducts = [];

    public decimal TotalCount => ProductType?.ProductResidue?.TotalCount ?? 0;

    public void SetParentViewModel(ProductPageViewModel viewModel)
    {
        _parentViewModel = viewModel;
    }

    partial void OnProductChanged(ProductViewModel? value)
    {
        if (value is not null && _parentViewModel is not null)
        {
            _ = _parentViewModel.OnProductChanged(this);
        }
    }

    partial void OnProductTypeChanged(ProductTypeViewModel? value)
    {
        if (value is not null && _parentViewModel is not null)
        {
            _ = _parentViewModel.OnProductTypeChanged(this);
        }

        OnPropertyChanged(nameof(TotalCount));
    }

    partial void OnQuantityChanged(decimal value)
    {
        OnPropertyChanged(nameof(TotalCount));
    }
}