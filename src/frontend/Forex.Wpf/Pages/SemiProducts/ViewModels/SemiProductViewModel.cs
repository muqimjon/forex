namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.Wpf.Pages.Common;
using Microsoft.Win32;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

    [RelayCommand]
    private void SelectImage()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Rasmlar (*.png;*.jpg)|*.png;*.jpg",
            Title = "Mahsulot rasmi tanlash"
        };

        if (dialog.ShowDialog() == true)
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(dialog.FileName);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            Image = bmp;
        }
    }
}


