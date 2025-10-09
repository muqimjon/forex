namespace Forex.Domain.Entities.Shops;

using Forex.Domain.Commons;
using Forex.Domain.Entities.Users;

public class ProductEntry : Auditable
{
    public long ProductTypeId { get; set; }
    public long ShopId { get; set; }
    public long EmployeeId { get; set; }
    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CostPreparation { get; set; }

    public ProductType ProductType { get; set; } = default!;
    public Shop Shop { get; set; } = default!;
    public User Employee { get; set; } = default!;
}