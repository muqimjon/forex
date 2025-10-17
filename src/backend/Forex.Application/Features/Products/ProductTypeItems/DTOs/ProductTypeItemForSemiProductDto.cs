namespace Forex.Application.Features.Products.ProductTypeItems.DTOs;

using Forex.Application.Features.Products.ProductTypes.DTOs;

public sealed record ProductTypeItemForSemiProductDto
{
    public long Id { get; set; }
    public decimal Quantity { get; set; }   // miqdori

    public long ProductTypeId { get; set; }
    public ProductTypeDto ProductType { get; set; } = default!;

    public long SemiProductId { get; set; }
}