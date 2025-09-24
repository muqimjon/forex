namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;

public partial class ProductTypeItemViewModel : ViewModelBase
{
    [ObservableProperty] private decimal quantity;

    // UI-only
    [ObservableProperty] private bool isSelected;
    [ObservableProperty] private SemiProductViewModel semiProduct = default!;
}
