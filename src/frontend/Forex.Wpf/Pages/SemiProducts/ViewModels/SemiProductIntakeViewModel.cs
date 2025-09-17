namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using Forex.Wpf.Pages.Common;
using System.Collections.ObjectModel;

public class SemiProductIntakeViewModel : ViewModelBase
{
    private DateTime? date = DateTime.Today;
    private long senderId;
    private long manufactoryId;
    private int containerCount = 1;
    private decimal transferFeePerContainer;
    private decimal deliveryPrice;
    private string? note;

    public DateTime? Date
    {
        get => date;
        set => SetProperty(ref date, value);
    }

    public long SenderId
    {
        get => senderId;
        set => SetProperty(ref senderId, value);
    }

    public long ManufactoryId
    {
        get => manufactoryId;
        set => SetProperty(ref manufactoryId, value);
    }

    public int ContainerCount
    {
        get => containerCount;
        set => SetProperty(ref containerCount, value);
    }

    public decimal TransferFeePerContainer
    {
        get => transferFeePerContainer;
        set => SetProperty(ref transferFeePerContainer, value);
    }

    public decimal DeliveryPrice
    {
        get => deliveryPrice;
        set => SetProperty(ref deliveryPrice, value);
    }

    public string? Note
    {
        get => note;
        set => SetProperty(ref note, value);
    }

    public decimal TotalTransferFee => ContainerCount * TransferFeePerContainer;
    public decimal TotalCost => TotalTransferFee + DeliveryPrice;

    public ObservableCollection<SemiProductItemViewModel> Items { get; } = [];
    public ObservableCollection<ContainerViewModel> Containers { get; } = [];
}