namespace Forex.Application.Features.Currencies.DTOs;

using Forex.Application.Features.Accounts.DTOs;
using Forex.Application.Features.Invoices.DTOs;
using Forex.Application.Features.Transactions.DTOs;

public record CurrencyDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NormalizedName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<TransactionDto> Transactions { get; set; } = default!;
    public ICollection<InvoiceDto> Invoices { get; set; } = default!;
    public ICollection<UserAccountDto> UserAccounts { get; set; } = default!;
    public ICollection<ShopAccountDto> ShopCashAccounts { get; set; } = default!;
}