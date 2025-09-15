namespace Forex.Application.Features.SemiProductEntries.DTOs;

public record ItemDto(
    long? SemiProductId,                   // if existing; otherwise provide new product data
    string? Name,
    int? Code,
    string? Measure,
    Stream? Photo,
    string? ContentType,
    string? Extension,
    decimal Quantity,
    decimal CostPrice,                     // per-line cost price (tan narx)
    decimal CostDelivery,                  // per-line delivery cost allocation
    decimal TransferFee                    // per-line transfer fee allocation (optional if you allocate per line)
);
