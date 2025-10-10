namespace Forex.Application.Features.Sales.DTOs;
public class SaleItemCreateDto
{
    public int TypeCount { get; set; }
    public int Count { get; set; }
    public decimal Price { get; set; }
    public decimal Amount { get; set; }

    public long ProductTypeId { get; set; }
}