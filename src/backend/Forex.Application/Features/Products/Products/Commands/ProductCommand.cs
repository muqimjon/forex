namespace Forex.Application.Features.Products.Products.Commands;

using Forex.Application.Features.Products.ProductTypes.Commands;

public sealed record ProductCommand
{
    public string Name { get; set; } = string.Empty;
    public int Code { get; set; }
    public long MeasureId { get; set; }
    public string? ImagePath { get; set; } = string.Empty;

    public ICollection<ProductTypeCommand> ProductTypes { get; set; } = default!;
}