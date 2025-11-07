namespace Forex.Application.Features.Products.ProductEntries.Commands;

public record ProductEntryCommand
{
    public int Count { get; set; }
    public int BundleItemCount { get; set; }
    public decimal PreparationCostPerUnit { get; set; }
    public decimal TotalAmount { get; set; }
    public long ProductTypeId { get; set; }
}