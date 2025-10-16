namespace Forex.Application.Features.Shops.DTOs;

using Forex.Application.Features.Accounts.DTOs;
using Forex.Application.Features.Products.ProductEntries.DTOs;
using Forex.Application.Features.Products.ProductResidues.DTOs;
using Forex.Application.Features.Transactions.DTOs;

public record ShopDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<ShopAccountDto> ShopAcounts { get; set; } = default!;
    public ICollection<ProductEntryDto> ProductEntries { get; set; } = default!;
    public ICollection<ProductResidueDto> ProductResidues { get; set; } = default!;
    public ICollection<TransactionDto> Transactions { get; set; } = default!;
}