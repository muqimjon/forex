namespace Forex.Application.Features.SemiProducts.SemiProductResidues.DTOs;

using Forex.Application.Features.Manufactories.DTOs;

public sealed record SemiProductResidueForSemiProdutDto
{
    public long Id { get; set; }
    public decimal Quantity { get; set; }

    public long ManufactoryId { get; set; }
    public ManufactoryForSemiProductResidueDto Manufactory { get; set; } = default!;

    public long SemiProductId { get; set; }
}