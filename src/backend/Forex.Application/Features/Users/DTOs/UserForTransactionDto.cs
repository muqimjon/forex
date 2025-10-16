namespace Forex.Application.Features.Users.DTOs;

using Forex.Application.Features.Accounts.DTOs;
using Forex.Application.Features.Products.ProductEntries.DTOs;
using Forex.Application.Features.Sales.DTOs;
using Forex.Domain.Enums;

public sealed record UserForTransactionDto
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

    public ICollection<UserAccountDto> Accounts { get; set; } = default!;
    public ICollection<SaleDto> Sales { get; set; } = default!;
    public ICollection<ProductEntryForUserDto> ProductEntries { get; set; } = default!;
}