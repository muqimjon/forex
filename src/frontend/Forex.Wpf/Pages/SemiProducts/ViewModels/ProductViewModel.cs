namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.Wpf.Pages.Common;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public partial class ProductViewModel : ViewModelBase
{
    [ObservableProperty] private int code;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private UnitMeasuerViewModel measure = default!;
    [ObservableProperty] private ImageSource? image;

    [ObservableProperty] private ObservableCollection<ProductTypeViewModel> types = [];

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
