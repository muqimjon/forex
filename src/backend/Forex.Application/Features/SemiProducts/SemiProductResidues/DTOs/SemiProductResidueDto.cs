namespace Forex.Application.Features.SemiProducts.SemiProductResidues.DTOs;

using Forex.Application.Features.Manufactories.DTOs;
using Forex.Application.Features.SemiProducts.SemiProducts.DTOs;

public sealed record SemiProductResidueDto
{
    public long Id { get; set; }
    public decimal Quantity { get; set; }

    public long ManufactoryId { get; set; }
    public ManufactoryForSemiProductResidueDto Manufactory { get; set; } = default!;

    public long SemiProductId { get; set; }
    public SemiProductForSemiProductResidueDto SemiProduct { get; set; } = default!;
}