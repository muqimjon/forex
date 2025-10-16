namespace Forex.Application.Features.Shops.DTOs;

using Forex.Application.Features.Products.ProductEntries.DTOs;
using Forex.Application.Features.Products.ProductResidues.DTOs;
using Forex.Application.Features.Transactions.DTOs;

public sealed record ShopForAccountDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<ProductEntryForShopDto> ProductEntries { get; set; } = default!;
    public ICollection<ProductResidueForShopDto> ProductResidues { get; set; } = default!;
    public ICollection<TransactionForShopDto> Transactions { get; set; } = default!;
}