namespace Forex.Application.Features.Sales.DTOs;

public record SaleItemDto
{
    public long Id { get; set; }
    public long ProductTypeId { get; set; }
    public decimal TypeCount { get; set; }
    public decimal Price { get; set; }
    public decimal Total => TypeCount * Price;
}