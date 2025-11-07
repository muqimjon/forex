namespace Forex.ClientService.Models.Requests;

public sealed record CreateProductEntryCommandRequest
{
    public List<ProductEntryRequest> Command { get; set; } = [];
}

public sealed record ProductEntryRequest
{
    public int Count { get; set; }
    public int BundleItemCount { get; set; }
    public decimal PreparationCostPerUnit { get; set; }
    public decimal TotalAmount { get; set; }
    public long ProductTypeId { get; set; }
}