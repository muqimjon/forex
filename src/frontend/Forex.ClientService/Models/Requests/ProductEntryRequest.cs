namespace Forex.ClientService.Models.Requests;

using Forex.ClientService.Enums;

public sealed record ProductEntryRequest
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public int BundleItemCount { get; set; }
    public int PackItemCount { get; set; }
    public decimal PreparationCostPerUnit { get; set; }
    public decimal UnitPrice { get; set; }
    public ProductionOrigin ProductionOrigin { get; set; }

    public long? ProductTypeId { get; set; }
    public ProductRequest Product { get; set; } = default!;
}