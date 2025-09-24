namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;
using System.Windows.Media;

public partial class SemiProductViewModel : ViewModelBase
{
    [ObservableProperty] private string? name;
    [ObservableProperty] private int code;
    [ObservableProperty] private UnitMeasuerViewModel measure = default!;
    [ObservableProperty] private int quantity;
    [ObservableProperty] private decimal costPrice;
    [ObservableProperty] private ImageSource? image;
    [ObservableProperty] private int totalQuantity;

    // UI-only
    [ObservableProperty] private bool isEditing;
    [ObservableProperty] private bool isSelected;
}

