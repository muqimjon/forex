namespace Forex.Domain.Entities.Shops;

using Forex.Domain.Commons;
using Forex.Domain.Entities.Users;

public class ProductEntry : Auditable
{
    public long ProductId { get; set; }
    public long ShopId { get; set; }
    public long EmployeeId { get; set; }
    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CostPreparation { get; set; }

    public Product Product { get; set; } = default!;
    public Shop Shop { get; set; } = default!;
    public User Employee { get; set; } = default!;
}