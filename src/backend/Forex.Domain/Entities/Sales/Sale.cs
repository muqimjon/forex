namespace Forex.Domain.Entities.Sales;

using Forex.Domain.Commons;
using Forex.Domain.Entities.Users;

public class Sale : Auditable
{
    public DateTime Date { get; set; }
    public long UserId { get; set; }
    public decimal CostPrice { get; set; }
    public decimal BenifitPrice { get; set; }
    public int TotalCount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Note { get; set; }

    public User User { get; set; } = default!;
    public ICollection<SaleItem> SaleItems { get; set; } = [];
}