namespace Forex.ClientService.Models.Responses;

public record ShopResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<ShopAccountResponse> ShopAccounts { get; set; } = default!;
    public ICollection<ProductEntryResponse> ProductEntries { get; set; } = default!;
    public ICollection<ProductResidueResponse> ProductResidues { get; set; } = default!;
    public ICollection<TransactionResponse> Transactions { get; set; } = default!;
}