namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;
using System.Collections.ObjectModel;
using System.Windows.Media;

public partial class ProductViewModel : ViewModelBase
{
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private int code;
    [ObservableProperty] private UnitMeasuerViewModel measure = default!;
    [ObservableProperty] private ImageSource? image;
    [ObservableProperty] private string type = string.Empty;
    [ObservableProperty] private int quantity;

    // UI-only
    [ObservableProperty] private bool isEditing;
    [ObservableProperty] private bool isSelected;

    [ObservableProperty] private ObservableCollection<ProductTypeItemViewModel> items = [];
}

