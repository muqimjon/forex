namespace Forex.Domain.Entities;

using Forex.Domain.Commons;

public class ProductEntry : Auditable
{
    public int ProductId { get; set; }
    public int ShopId { get; set; }
    public int UserId { get; set; }
    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CostPreparation { get; set; }

    public Product Product { get; set; } = default!;
    public Shop Shop { get; set; } = default!;
    public User User { get; set; } = default!;
}