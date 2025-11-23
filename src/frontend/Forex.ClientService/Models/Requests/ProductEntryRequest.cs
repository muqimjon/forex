namespace Forex.ClientService.Models.Requests;

using Forex.ClientService.Enums;

public sealed record CreateProductEntryCommandRequest
{
    public List<ProductEntryRequest> Command { get; set; } = [];
}

public sealed record ProductEntryRequest
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public uint? Count { get; set; }
    public uint BundleItemCount { get; set; }
    public decimal PreparationCostPerUnit { get; set; }
    public decimal UnitPrice { get; set; }
    public ProductionOrigin ProductionOrigin { get; set; }

    public ProductRequest Product { get; set; } = default!;
}