namespace Forex.Application.Features.Products.ProductEntries.Commands;

public record ProductEntryCommand
{
    public int BundleCount { get; set; }
    public decimal PreparationCostPerUnit { get; set; }
    public decimal TotalAmount { get; set; }
    public long EmployeeId { get; set; }
    public long ProductTypeId { get; set; }
}