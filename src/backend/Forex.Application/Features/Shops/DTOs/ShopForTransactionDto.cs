namespace Forex.Application.Features.Shops.DTOs;

using Forex.Application.Features.Accounts.DTOs;
using Forex.Application.Features.Products.ProductEntries.DTOs;
using Forex.Application.Features.Products.ProductResidues.DTOs;

public sealed record ShopForTransactionDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<AccountForShopDto> ShopAccounts { get; set; } = default!;
    public ICollection<ProductEntryForShopDto> ProductEntries { get; set; } = default!;
    public ICollection<ProductResidueForShopDto> ProductResidues { get; set; } = default!;
}