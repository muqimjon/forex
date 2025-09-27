namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;
using System.Collections.ObjectModel;

public partial class ProductTypeViewModel : ViewModelBase
{
    [ObservableProperty] private string type = string.Empty;
    [ObservableProperty] private int quantity;

    [ObservableProperty] private bool isEditing;
    [ObservableProperty] private bool isSelected;

    [ObservableProperty] private ObservableCollection<ProductTypeItemViewModel> items = [];

    [ObservableProperty] private ProductViewModel product = default!;

    private ProductViewModel? previousProduct;

    partial void OnProductChanged(ProductViewModel value)
    {
        var vm = (SemiProductPageViewModel)App.Current.MainWindow.DataContext;

        if (previousProduct is not null &&
            previousProduct.Code != 0 &&
            vm.Products.Contains(previousProduct) &&
            previousProduct.Types.Count == 0)
        {
            vm.Products.Remove(previousProduct);
        }

        if (value is not null && !value.Types.Contains(this))
        {
            value.Types.Add(this);
        }

        previousProduct = value;
    }
}
