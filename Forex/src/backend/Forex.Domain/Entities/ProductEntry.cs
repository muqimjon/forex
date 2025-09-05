namespace Forex.Domain.Entities;

using Forex.Domain.Commons;

public class ProductEntry : Auditable
{
    public long ProductId { get; set; }
    public long ShopId { get; set; }
    public long UserId { get; set; }
    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CostPreparation { get; set; }

    public Product Product { get; set; } = default!;
    public Shop Shop { get; set; } = default!;
    public User User { get; set; } = default!;
}