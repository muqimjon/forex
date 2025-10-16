namespace Forex.Application.Features.SemiProducts.SemiProductEntries.DTOs;

public sealed record ProductTypeItemCommand
{
    public decimal Quantity { get; set; }
    public SemiProductCommand SemiProduct { get; set; } = default!;
}