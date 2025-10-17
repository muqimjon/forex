using Forex.Application.Features.SemiProducts.SemiProducts.Commands;

namespace Forex.Application.Features.Products.ProductTypeItems.Commands;

public sealed record ProductTypeItemCommand
{
    public decimal Quantity { get; set; }
    public SemiProductCommand SemiProduct { get; set; } = default!;
}