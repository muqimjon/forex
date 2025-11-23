namespace Forex.Application.Features.Products.Products.Commands;

using Forex.Application.Features.Products.ProductTypes.Commands;
using Forex.Domain.Enums;

public sealed record ProductCommand
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long UnitMeasureId { get; set; }
    public string? ImagePath { get; set; } = string.Empty;
    public ProductionOrigin ProductionOrigin { get; set; }

    public ICollection<ProductTypeCommand> ProductTypes { get; set; } = [];
}