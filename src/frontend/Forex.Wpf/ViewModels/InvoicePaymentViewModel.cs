namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.ClientService.Enums;
using Forex.Wpf.Pages.Common;

public partial class InvoicePaymentViewModel : ViewModelBase
{
    public long Id { get; set; }
    public long InvoiceId { get; set; }
    public long UserId { get; set; }
    public long CurrencyId { get; set; }

    [ObservableProperty] InvoiceViewModel invoice = default!;
    [ObservableProperty] UserViewModel user = default!;
    [ObservableProperty] CurrencyViewModel currency = default!;
    [ObservableProperty] PaymentTarget target;
    [ObservableProperty] decimal exchangeRate;
    [ObservableProperty] decimal amount;
}