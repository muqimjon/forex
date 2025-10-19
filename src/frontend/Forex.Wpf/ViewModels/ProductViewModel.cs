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
    public long Id { get; set; }
    [ObservableProperty] private int? code = default!;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private UnitMeasuerViewModel measure = default!;
    [ObservableProperty] private ImageSource? image;

    
    [ObservableProperty] private ObservableCollection<ProductTypeViewModel> productTypes = [];
    [ObservableProperty] private ProductTypeViewModel? selectedType;
    [ObservableProperty] private int count;
    [ObservableProperty] private int typeCount;
    [ObservableProperty] private int totalCount;
    [ObservableProperty] private decimal perPairRate;
    [ObservableProperty] private decimal totalAmount;


    partial void OnSelectedTypeChanged(ProductTypeViewModel? value)
    {
        Count = value?.Count ?? 0;
    }

    partial void OnTypeCountChanged(int oldValue, int newValue)
    {
        CalculateTotalCount();
    }
   
    partial void OnCountChanged(int oldValue, int newValue)
    {
        CalculateTotalCount();
    }

    partial void OnTotalCountChanged(int oldValue, int newValue)
    {
        CalculateTotalAmount();
    }
    partial void OnPerPairRateChanged(decimal oldValue, decimal newValue)
    {
        CalculateTotalAmount();
    }

    private void CalculateTotalCount()
    {
        TotalCount = Count * TypeCount;
    }

    private void CalculateTotalAmount()
    {
        TotalAmount = PerPairRate * TotalCount;
    }


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

    private ProductViewModel? selected;
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
                Measure = value.Measure;
                Image = value.Image;

                SelectedType = value.SelectedType;
                ProductTypes = new ObservableCollection<ProductTypeViewModel>(value.ProductTypes ?? []);
            }
        }
    }
}
