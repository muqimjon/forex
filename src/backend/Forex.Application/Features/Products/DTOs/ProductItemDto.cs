namespace Forex.Application.Features.Products.DTOs;

using Forex.Application.Features.SemiProducts.DTOs;

public class ProductItemDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public long SemiProductId { get; set; }
    public decimal Quantity { get; set; }

    public SemiProductDto SemiProduct { get; set; } = default!;
}