namespace Forex.Application.Features.SemiProducts.SemiProductEntries.DTOs;

public sealed record ProductCommand
{
    public string Name { get; set; } = string.Empty;
    public int Code { get; set; }
    public long MeasureId { get; set; }
    public string? ImagePath { get; set; } = string.Empty;

    public ICollection<ProductTypeCommand> Types { get; set; } = default!;
}