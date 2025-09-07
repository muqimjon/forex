namespace Forex.Application.Features.SemiProducts.DTOs;

using Forex.Application.Features.Manufactories.DTOs;

public sealed record SemiProductDto(
    long Id,
    long ManufactoryId,
    string? Name,
    string NormalizedName,
    int Code,
    string Measure,
    string? PhotoPath,
    ManufactoryDto Manufactory);