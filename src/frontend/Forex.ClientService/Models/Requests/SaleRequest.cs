namespace Forex.ClientService.Models.Requests;

public sealed record SaleRequest
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public long CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Note { get; set; }
    public List<SaleItemRequest> SaleItems { get; set; } = default!;
}

public sealed record SaleItemRequest
{
    public int BundleCount { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }

    public long ProductTypeId { get; set; }
}