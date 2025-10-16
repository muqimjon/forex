namespace Forex.Domain.Entities.Sales;

using Forex.Domain.Commons;
using Forex.Domain.Entities;

public class Sale : Auditable
{
    public DateTime Date { get; set; }
    public decimal CostPrice { get; set; }      // 1 ta savdoda umumiy tannarxi
    public decimal BenifitPrice { get; set; }    // 1 ta savdoda umumiy foydasi
    public int TotalCount { get; set; }       // 1 ta savdoda jami necha dona sotildi
    public decimal TotalAmount { get; set; }   // 1 ta savdoda jami summa
    public string? Note { get; set; }

    public long UserId { get; set; }
    public User User { get; set; } = default!;

    public ICollection<SaleItem> SaleItems { get; set; } = default!;
}