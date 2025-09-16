namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using Forex.Wpf.Pages.Common;
using System.Collections.ObjectModel;

public class SemiProductIntakeViewModel : ViewModelBase
{
    private long senderId;
    private long manufactoryId;
    private DateTime entryDate = DateTime.Now;
    private decimal transferFeePerContainer;
    private string? note;

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

    public DateTime EntryDate
    {
        get => entryDate;
        set => SetProperty(ref entryDate, value);
    }

    public decimal TransferFeePerContainer
    {
        get => transferFeePerContainer;
        set => SetProperty(ref transferFeePerContainer, value);
    }

    public string? Note
    {
        get => note;
        set => SetProperty(ref note, value);
    }

    public ObservableCollection<SemiProductItemViewModel> Items { get; }
        = [];

    public ObservableCollection<ContainerViewModel> Containers { get; }
        = [];
}
