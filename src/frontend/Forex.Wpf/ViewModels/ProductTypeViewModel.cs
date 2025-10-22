namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;
using System.Collections.ObjectModel;

public partial class ProductTypeViewModel : ViewModelBase
{
    public long Id { get; set; }
    [ObservableProperty] private string type = string.Empty;
    [ObservableProperty] private int? count;
    [ObservableProperty] private decimal? cost;
    [ObservableProperty] private ObservableCollection<ProductTypeItemViewModel> productTypeItems = [];

    // for UI only
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
                Count = value.Count;
                Cost = value.Cost;
                ProductTypeItems = value.ProductTypeItems;
            }
        }
    }

    #endregion Property Changes
}
