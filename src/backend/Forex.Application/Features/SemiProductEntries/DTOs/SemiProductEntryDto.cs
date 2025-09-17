namespace Forex.Application.Features.SemiProductEntries.DTOs;

using Forex.Application.Features.Manufactories.DTOs;
using Forex.Application.Features.SemiProducts.DTOs;

public sealed record SemiProductEntryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Code { get; set; }
    public string Measure { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }

    public SemiProductDto SemiProduct { get; set; } = default!;
    public ManufactoryDto Manufactory { get; set; } = default!;
}