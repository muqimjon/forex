namespace Forex.Application.Features.SemiProductEntries.DTOs;

public sealed record SemiProductEntryDto(
    string Name,
    int Code,
    string Measure,
    decimal Quantity,
    decimal CostPrice
);