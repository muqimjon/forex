using Forex.Application.Features.Products.ProductTypeItems.Commands;

namespace Forex.Application.Features.Products.ProductTypes.Commands;

public sealed record ProductTypeCommand
{
    public string Type { get; set; } = string.Empty!;
    public int Count { get; set; }
    public ICollection<ProductTypeItemCommand> Items { get; set; } = default!;
}