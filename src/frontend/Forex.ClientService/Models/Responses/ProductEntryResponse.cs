namespace Forex.ClientService.Models.Responses;

public class ProductEntryResponse
{
    public long Id { get; set; }
    public int Count { get; set; }
    public int BundleItemCount { get; set; }
    public decimal CostPrice { get; set; }     // tannarxi
    public decimal PreparationCostPerUnit { get; set; }  // tayyorlashga ketgan xarajat summasi
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }

    public long ProductTypeId { get; set; }  // 
    public ProductTypeResponse ProductType { get; set; } = default!;  // razmeri 24-29, 30-35, 36-41

    public long ShopId { get; set; }
    public ShopResponse Shop { get; set; } = default!;
}