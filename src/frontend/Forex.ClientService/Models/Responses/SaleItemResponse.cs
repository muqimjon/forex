namespace Forex.ClientService.Models.Responses;

public sealed record SaleItemResponse
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public decimal CostPrice { get; set; }      // 1 ta savdoda umumiy tannarxi
    public decimal BenifitPrice { get; set; }    // 1 ta savdoda umumiy foydasi
    public int TotalCount { get; set; }       // 1 ta savdoda jami necha dona sotildi
    public decimal TotalAmount { get; set; }   // 1 ta savdoda jami summa
    public string? Note { get; set; }

    public long UserId { get; set; }
    public UserResponse User { get; set; } = default!;

    public long ProductTypeId { get; set; }   // 24-29, 30-35, 36-41 razmeri idsi
    public ProductTypeResponse ProductType { get; set; } = default!;
}