namespace Forex.Application.Features.Sales.DTOs;

public record SaleListDto
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal CostPrice { get; set; }
    public decimal BenifitPrice { get; set; }
    public string? Note { get; set; }
}

