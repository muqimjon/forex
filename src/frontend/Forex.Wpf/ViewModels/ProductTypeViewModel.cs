namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

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
}
