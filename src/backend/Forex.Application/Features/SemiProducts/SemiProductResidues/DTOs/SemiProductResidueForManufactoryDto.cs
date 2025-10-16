namespace Forex.Application.Features.SemiProducts.SemiProductResidues.DTOs;

using Forex.Application.Features.SemiProducts.SemiProducts.DTOs;

public sealed class SemiProductResidueForManufactoryDto
{
    public long Id { get; set; }
    public decimal Quantity { get; set; }

    public long ManufactoryId { get; set; }

    public long SemiProductId { get; set; }
    public SemiProductDto SemiProduct { get; set; } = default!;
}