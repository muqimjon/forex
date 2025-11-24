namespace Forex.Application.Features.Products.ProductEntries.Commands;

using Forex.Application.Features.Products.Products.Commands;
using Forex.Domain.Enums;

public record ProductEntryCommand
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public int BundleItemCount { get; set; }
    public decimal PreparationCostPerUnit { get; set; }
    public decimal UnitPrice { get; set; }
    public ProductionOrigin ProductionOrigin { get; set; }
    public ProductCommand Product { get; set; } = default!;
}