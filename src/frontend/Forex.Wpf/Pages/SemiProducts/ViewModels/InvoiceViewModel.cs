namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;

public partial class InvoiceViewModel : ViewModelBase
{
    [ObservableProperty] private DateTime entryDate = DateTime.Now;
    [ObservableProperty] private decimal costPrice;
    [ObservableProperty] private decimal costDelivery;
    [ObservableProperty] private decimal transferFee;

    // Qo‘shimcha hisob-kitoblar uchun computed property
    public decimal TotalCost => CostPrice + CostDelivery + TransferFee;
}
