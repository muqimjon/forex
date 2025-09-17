namespace Forex.Application.Features.Manufactories.DTOs;

using Forex.Application.Features.SemiProducts.DTOs;

public sealed record SemiProductResidueDto
(
    long Id,
    long SemiProductId,
    long ManufactoryId,
    decimal Quantity,
    SemiProductDto SemiProduct
);