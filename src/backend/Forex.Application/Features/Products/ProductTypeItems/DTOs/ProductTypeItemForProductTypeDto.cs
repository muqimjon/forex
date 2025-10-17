namespace Forex.Application.Features.Products.ProductTypeItems.DTOs;

using Forex.Application.Features.SemiProducts.SemiProducts.DTOs;

public sealed record ProductTypeItemForProductTypeDto
{
    public long Id { get; set; }
    public decimal Quantity { get; set; }   // miqdori

    public long ProductTypeId { get; set; }

    public long SemiProductId { get; set; }
    public SemiProductDto SemiProduct { get; set; } = default!;
}