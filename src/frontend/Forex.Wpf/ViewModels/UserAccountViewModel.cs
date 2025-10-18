namespace Forex.Wpf.ViewModels;

using Forex.Wpf.Pages.Common;

public class UserAccountViewModel : ViewModelBase
{
    public long Id { get; set; }

    public decimal OpeningBalance { get; set; }
    public decimal Discount { get; set; }
    public decimal Balance { get; set; }

    public long UserId { get; set; }
    public UserViewModel User { get; set; } = default!;

    public long CurrencyId { get; set; }
    public CurrencyViewModel Currency { get; set; } = default!;
}