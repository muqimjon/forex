namespace Forex.ClientService.Models.Requests;

using Forex.ClientService.Enums;

public class ProductEntryCommandRequest
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public int BundleItemCount { get; set; }
    public decimal PreparationCostPerUnit { get; set; }
    public decimal UnitPrice { get; set; }
    public ProductionOrigin ProductionOrigin { get; set; }
    public ProductCommandRequest Product { get; set; } = new();
}
