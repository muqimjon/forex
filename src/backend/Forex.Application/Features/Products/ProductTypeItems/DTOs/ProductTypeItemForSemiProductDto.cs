namespace Forex.Application.Features.Products.ProductTypeItems.DTOs;

using Forex.Application.Features.Products.ProductTypes.DTOs;

public sealed record ProductTypeItemForSemiProductDto
{
    public long Id { get; set; }
    public decimal Quantity { get; set; }   // miqdori

    public long TypeId { get; set; }
    public ProductTypeDto Type { get; set; } = default!;

    public long SemiProductId { get; set; }
}