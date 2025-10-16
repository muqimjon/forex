namespace Forex.Application.Features.Products.ProductTypeItems.DTOs;

using Forex.Application.Features.Products.ProductTypes.DTOs;
using Forex.Application.Features.SemiProducts.SemiProducts.DTOs;

public class ProductTypeItemDto
{
    public long Id { get; set; }
    public decimal Quantity { get; set; }   // miqdori

    public long TypeId { get; set; }
    public ProductTypeDto Type { get; set; } = default!;

    public long SemiProductId { get; set; }
    public SemiProductDto SemiProduct { get; set; } = default!;
}