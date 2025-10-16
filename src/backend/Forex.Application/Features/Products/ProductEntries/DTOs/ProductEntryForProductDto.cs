namespace Forex.Application.Features.Products.ProductEntries.DTOs;

using Forex.Application.Features.Shops.DTOs;
using Forex.Application.Features.Users.DTOs;

public sealed record ProductEntryForProductTypeDto
{
    public long Id { get; set; }
    public int Count { get; set; }
    public decimal CostPrice { get; set; }     // tannarxi
    public decimal CostPreparation { get; set; }  // tayyorlashga ketgan xarajat summasi

    public long ProductTypeId { get; set; }

    public long ShopId { get; set; }
    public ShopForProductEntryDto Shop { get; set; } = default!;

    public long EmployeeId { get; set; }
    public UserForProductEntryDto Employee { get; set; } = default!;
}