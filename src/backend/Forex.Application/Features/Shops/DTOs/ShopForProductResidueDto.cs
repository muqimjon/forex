namespace Forex.Application.Features.Shops.DTOs;

using Forex.Application.Features.Accounts.DTOs;
using Forex.Application.Features.Products.ProductEntries.DTOs;
using Forex.Application.Features.Transactions.DTOs;

public record ShopForProductResidueDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<AccountForShopDto> ShopAcounts { get; set; } = default!;
    public ICollection<ProductEntryForShopDto> ProductEntries { get; set; } = default!;
    public ICollection<TransactionForShopDto> Transactions { get; set; } = default!;
}