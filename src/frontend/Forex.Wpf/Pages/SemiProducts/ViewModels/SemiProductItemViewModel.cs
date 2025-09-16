namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using Forex.Wpf.Pages.Common;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public class SemiProductItemViewModel : ViewModelBase
{
    private string? name;
    private int? code;
    private string? measure;
    private string? photoPath;
    private ImageSource? previewImage;
    private decimal? quantity;
    private decimal? costPrice;
    private decimal? costDelivery;
    private decimal? transferFee;

    public string? Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    public int? Code
    {
        get => code;
        set => SetProperty(ref code, value);
    }

    public string? Measure
    {
        get => measure;
        set => SetProperty(ref measure, value);
    }

    public string? PhotoPath
    {
        get => photoPath;
        set
        {
            if (SetProperty(ref photoPath, value) && !string.IsNullOrEmpty(value))
            {
                try
                {
                    PreviewImage = new BitmapImage(new Uri(value));
                }
                catch
                {
                    PreviewImage = null;
                }
            }
        }
    }

    public ImageSource? PreviewImage
    {
        get => previewImage;
        private set => SetProperty(ref previewImage, value);
    }

    public decimal? Quantity
    {
        get => quantity;
        set => SetProperty(ref quantity, value);
    }

    public decimal? CostPrice
    {
        get => costPrice;
        set => SetProperty(ref costPrice, value);
    }

    public decimal? CostDelivery
    {
        get => costDelivery;
        set => SetProperty(ref costDelivery, value);
    }

    public decimal? TransferFee
    {
        get => transferFee;
        set => SetProperty(ref transferFee, value);
    }

    public decimal TotalCost
    {
        get => (decimal)(costDelivery + costPrice + transferFee)!;
    }
}
