namespace Forex.Wpf.ViewModels;

using Forex.ClientService.Models.Responses;
using Forex.Wpf.Pages.Common;

public partial class SaleViewModel : ViewModelBase
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public decimal CostPrice { get; set; }
    public decimal BenifitPrice { get; set; }
    public int TotalCount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Note { get; set; }

    public long CustomerId { get; set; }
    public UserResponse Customer { get; set; } = default!;

    public ICollection<SaleItemResponse> SaleItems { get; set; } = default!;
}