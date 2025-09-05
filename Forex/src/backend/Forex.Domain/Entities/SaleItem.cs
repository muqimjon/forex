namespace Forex.Domain.Entities;

using Forex.Domain.Commons;

public class SaleItem : Auditable
{
    public long SaleId { get; set; }
    public long ProductId { get; set; }
    public int Count { get; set; }
    public decimal CostPrice { get; set; }
    public decimal Benifit { get; set; }
    public decimal TotalSum { get; set; }

    public Sale Sale { get; set; } = default!;
    public Product Product { get; set; } = default!;
}