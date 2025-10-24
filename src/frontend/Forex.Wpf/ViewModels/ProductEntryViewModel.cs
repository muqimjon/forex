namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;
using System.Collections.ObjectModel;

public partial class ProductEntryViewModel : ViewModelBase
{
    public long ProductTypeId { get; set; }
    public long ProductId { get; set; }
    public long Id { get; set; }
    [ObservableProperty] private int? typeCount;
    [ObservableProperty] private int? totalCount;
    [ObservableProperty] private decimal? preparationCostPerUnit;
    [ObservableProperty] private decimal? totalAmount;

    [ObservableProperty] private ProductViewModel product = default!;
    [ObservableProperty] private ProductTypeViewModel productType = default!;
    [ObservableProperty] private UserViewModel employee = default!;

    [ObservableProperty] private ObservableCollection<ProductTypeViewModel> availableProductTypes = [];


    #region Property Changes

    partial void OnTypeCountChanged(int? value) => CalculateTotalCount();
    partial void OnTotalCountChanged(int? value) => CalculateTotalAmount();
    partial void OnPreparationCostPerUnitChanged(decimal? value) => CalculateTotalAmount();

    #endregion Property Changes

    #region Private Helpers

    private void CalculateTotalCount()
    {
        if (TypeCount is null || ProductType is null)
        {
            TotalCount = null;
            return;
        }

        TotalCount = TypeCount * ProductType.Count;
    }

    private void CalculateTotalAmount()
    {
        if (PreparationCostPerUnit is null || TotalCount is null)
        {
            TotalAmount = null;
            return;
        }

        TotalAmount = PreparationCostPerUnit * TotalCount;
    }

    #endregion Private Helpers
}