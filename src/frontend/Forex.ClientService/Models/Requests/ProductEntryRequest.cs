namespace Forex.ClientService.Models.Requests;

public sealed record CreateProductEntryCommandRequest
{
    public List<ProductEntryRequest> Command { get; set; } = [];
}

public sealed record ProductEntryRequest
{
    public int BundleCount { get; set; }
    public decimal PreparationCostPerUnit { get; set; }
    public decimal TotalAmount { get; set; }
    public long ProductTypeId { get; set; }
    public long EmployeeId { get; set; }
    public decimal Quantity { get; set; }
}