namespace Forex.Application.Features.Products.ProductTypes.Commands;

using Forex.Application.Features.Products.ProductTypeItems.Commands;

public sealed record ProductTypeCommand
{
    public string Type { get; set; } = string.Empty!;
    public int Count { get; set; }
    public ICollection<ProductTypeItemCommand> ProductTypeItems { get; set; } = default!;
}