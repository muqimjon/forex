namespace Forex.Application.Features.SemiProducts.SemiProductEntries.DTOs;

public sealed record ProductTypeCommand
{
    public string Type { get; set; } = string.Empty!;
    public int Count { get; set; }
    public ICollection<ProductTypeItemCommand> Items { get; set; } = default!;
}