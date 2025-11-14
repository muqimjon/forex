namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.ClientService.Models.Responses;
using Forex.Wpf.Pages.Common;
using System.Collections.ObjectModel;

public partial class ProductTypeViewModel : ViewModelBase
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    [ObservableProperty] private string type = string.Empty;
    [ObservableProperty] private int? bundleItemCount;
    [ObservableProperty] private decimal? cost;
    [ObservableProperty] private ObservableCollection<ProductTypeItemViewModel> productTypeItems = [];
    [ObservableProperty] private ProductResidueResponse productResidue = default!;
    [ObservableProperty] private ICollection<SemiProductEntryResponse> productEntries = default!;

    // for UI only
    [ObservableProperty] private int availableCount;
    [ObservableProperty] private ProductViewModel product = default!;
    private ProductTypeViewModel? selected;

    #region Property Changes

    partial void OnProductChanged(ProductViewModel value)
    {
        if (value is not null && value.ProductTypes is not null && !value.ProductTypes.Contains(this))
            value.ProductTypes.Add(this);
    }

    public ProductTypeViewModel? Selected
    {
        get => selected;
        set
        {
            if (SetProperty(ref selected, value) && value is not null)
            {
                Id = value.Id;
                Type = value.Type;
                BundleItemCount = value.BundleItemCount;
                Cost = value.Cost;
                ProductTypeItems = value.ProductTypeItems;
            }
        }
    }

    #endregion Property Changes
}
