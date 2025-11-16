namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;
using System.Collections.ObjectModel;

public partial class InvoiceViewModel : ViewModelBase
{
    public long Id { get; set; }
    public long ManufactoryId { get; set; }
    [ObservableProperty] private ManufactoryViewModel manufactory = default!;
    [ObservableProperty] private ObservableCollection<InvoicePaymentViewModel> payments = [];
    [ObservableProperty] private ObservableCollection<SemiProductEntryViewModel> semiProductEntries = [];

    [ObservableProperty] private DateTime date = DateTime.Now;
    [ObservableProperty] private string? number;
    [ObservableProperty] private decimal? costPrice;
    [ObservableProperty] private decimal? costDelivery;
    [ObservableProperty] private int? containerCount;
    [ObservableProperty] private decimal? pricePerUnitContainer;
    [ObservableProperty] private decimal? consolidatorFee;
    [ObservableProperty] private bool viaConsolidator;

    [ObservableProperty] private decimal? totalAmount;

    // for UI
    [ObservableProperty] private InvoicePaymentViewModel? paymentToSupplier;
    [ObservableProperty] private InvoicePaymentViewModel? paymentToConsolidator;
}
