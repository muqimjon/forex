namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.Wpf.Pages.Common;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public partial class ProductViewModel : ViewModelBase
{
    public ProductViewModel()
    {
        ProductTypes = [];
        UnitMeasure = new();
        Image = null;
    }

    public long Id { get; set; }
    [ObservableProperty] private string code = string.Empty;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private UnitMeasuerViewModel unitMeasure = default!;
    [ObservableProperty] private ImageSource? image;

    [ObservableProperty] private ObservableCollection<ProductTypeViewModel> productTypes = [];
    [ObservableProperty] private ProductTypeViewModel? selectedType;
    private ProductViewModel? selected;

    public decimal TotalAmount =>
        ProductTypes?.Sum(pt =>
            pt.ProductTypeItems?.Sum(item => item.SemiProduct.TotalAmount) ?? 0
        ) ?? 0;


    #region Commands

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

    #endregion Commands

    #region Property Changes

    public ProductViewModel? Selected
    {
        get => selected;
        set
        {
            if (SetProperty(ref selected, value) && value is not null)
            {
                Id = value.Id;
                Code = value.Code;
                Name = value.Name;
                UnitMeasure = value.UnitMeasure;
                Image = value.Image;
                SelectedType = value.SelectedType;
                ProductTypes = new ObservableCollection<ProductTypeViewModel>(value.ProductTypes ?? []);
            }
        }
    }

    #endregion Property Changes
}
