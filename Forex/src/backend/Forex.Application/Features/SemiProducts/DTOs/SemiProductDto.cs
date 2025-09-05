namespace Forex.Application.Features.SemiProducts.DTOs;

using Forex.Application.Features.Manufactories.DTOs;

public record SemiProductDto
{
    public long Id { get; set; }
    public long ManufactoryId { get; set; }
    public string? Name { get; set; }
    public string NormalizedName { get; set; } = string.Empty;
    public int Code { get; set; }
    public string Measure { get; set; } = string.Empty;
    public string? PhotoPath { get; set; }

    public ManufactoryDto Manufactory { get; set; } = default!;
}