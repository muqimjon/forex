namespace Forex.Application.Features.Users.DTOs;

using Forex.Application.Features.Accounts.DTOs;
using Forex.Application.Features.Invoices.Invoices.DTOs;
using Forex.Application.Features.Transactions.DTOs;
using Forex.Domain.Enums;

public sealed record UserForSaleDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NormalizedName { get; set; } = string.Empty;
    public string? Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? NormalizedEmail { get; set; }
    public UserRole Role { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    public string? PasswordHash { get; set; }

    public ICollection<AccountForUserDto> Accounts { get; set; } = default!;
    public ICollection<TransactionForUserDto> Transactions { get; set; } = default!;
    public ICollection<InvoiceForUserDto> Invoices { get; set; } = default!;
}