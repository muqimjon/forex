namespace Forex.ClientService.Models.Responses;

using Forex.ClientService.Enums;

public class ProductEntryResponse
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public int BundleItemCount { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? PreparationCostPerUnit { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? TotalAmount { get; set; }
    public ProductionOrigin? ProductionOrigin { get; set; }

    public long? ProductTypeId { get; set; }
    public ProductTypeResponse? ProductType { get; set; }

    public long? ShopId { get; set; }
    public ShopResponse? Shop { get; set; }

    public long? ProductResidueId { get; set; }
    public ProductResidueResponse? ProductResidue { get; set; }
}