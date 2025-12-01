namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.ClientService.Enums;
using Forex.Wpf.Pages.Common;

public partial class TransactionViewModel : ViewModelBase
{
    public long Id { get; set; }
    public long ShopId { get; set; }
    public long UserId { get; set; }
    public long CurrencyId { get; set; }

    [ObservableProperty] private DateTime date = DateTime.Now;
    [ObservableProperty] private bool isIncome;
    [ObservableProperty] private decimal? amount;
    [ObservableProperty] private decimal? exchangeRate;
    [ObservableProperty] private decimal? discount;
    [ObservableProperty] private PaymentMethod paymentMethod;
    [ObservableProperty] private string? description;
    [ObservableProperty] private DateTime? dueDate;

    [ObservableProperty] private UserViewModel user = default!;
    [ObservableProperty] private CurrencyViewModel currency = default!;

    // UI helper properties for two-way binding
    [ObservableProperty] private decimal? income;
    [ObservableProperty] private decimal? expense;
}