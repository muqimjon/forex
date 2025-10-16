namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.Wpf.Pages.Common;
using Microsoft.Win32;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public partial class SemiProductViewModel : ViewModelBase
{
    public long Id { get; set; }
    [ObservableProperty] private string? name;
    [ObservableProperty] private UnitMeasuerViewModel measure = default!;
    [ObservableProperty] private ImageSource? image;
    [ObservableProperty] private decimal quantity;
    [ObservableProperty] private decimal costPrice;
    [ObservableProperty] private decimal totalAmount;

    // UI-only
    public ProductTypeItemViewModel? LinkedItem { get; set; }

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

    partial void OnCostPriceChanged(decimal value) => CalculateTotalAmount();
    partial void OnQuantityChanged(decimal value) => CalculateTotalAmount();

    partial void OnTotalAmountChanged(decimal value)
    {
        CostPrice = Quantity != 0 ? value / Quantity : 0;
    }

    private void CalculateTotalAmount()
    {
        TotalAmount = CostPrice * Quantity;
    }
}


