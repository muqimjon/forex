namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;
using System.Windows.Media;

public partial class SemiProductViewModel : ViewModelBase
{
    [ObservableProperty] private string? name;
    [ObservableProperty] private int code;
    [ObservableProperty] private UnitMeasuerViewModel measure = default!;
    [ObservableProperty] private decimal costPrice;
    [ObservableProperty] private ImageSource? image;
    [ObservableProperty] private int totalQuantity;

    // UI-only
    public ProductTypeItemViewModel? LinkedItem { get; set; }

    private decimal? linkedQuantity;
    public decimal? LinkedQuantity
    {
        get => linkedQuantity;
        set
        {
            if (SetProperty(ref linkedQuantity, value))
            {
                if (LinkedItem is not null)
                    LinkedItem.Quantity = (decimal)value!;
            }
        }
    }

    [ObservableProperty] private bool isEditing;
    [ObservableProperty] private bool isSelected;
}


