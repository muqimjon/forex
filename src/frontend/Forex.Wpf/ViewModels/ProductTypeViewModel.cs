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
    [ObservableProperty] private bool isEditing;
    [ObservableProperty] private bool isSelected;

    [ObservableProperty] private ProductViewModel product = default!;
    private ProductViewModel? previousProduct;
    partial void OnProductChanged(ProductViewModel value)
    {
        if (value is not null && !value.ProductTypes.Contains(this))
        {
            value.ProductTypes.Add(this);
        }

        previousProduct = value;
    }


    private ProductTypeViewModel? selected;

    public ProductTypeViewModel? Selected
    {
        get => selected;
        set
        {
            if (SetProperty(ref selected, value) && value is not null)
            {
                Id = value.Id;
                Product = value.Product;
                Type = value.Type;
                Count = value.Count;
                Cost = value.Cost;
                ProductTypeItems = value.ProductTypeItems;
            }
        }
    }
}
