namespace Forex.Application.Features.Sales.DTOs;

using Forex.Application.Features.Sales.SaleItems.DTOs;
using Forex.Application.Features.Users.DTOs;

public sealed record SaleDto
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public decimal CostPrice { get; set; }      // 1 ta savdoda umumiy tannarxi
    public decimal BenifitPrice { get; set; }    // 1 ta savdoda umumiy foydasi
    public int TotalCount { get; set; }       // 1 ta savdoda jami necha dona sotildi
    public decimal TotalAmount { get; set; }   // 1 ta savdoda jami summa
    public string? Note { get; set; }

    public long UserId { get; set; }
    public UserDto User { get; set; } = default!;

    public ICollection<SaleItemDto> SaleItems { get; set; } = default!;
}