namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using Forex.Wpf.Pages.Common;

public class ContainerViewModel : ViewModelBase
{
    private long count;
    private decimal price;

    public long Count
    {
        get => count;
        set => SetProperty(ref count, value);
    }

    public decimal Price
    {
        get => price;
        set => SetProperty(ref price, value);
    }
}