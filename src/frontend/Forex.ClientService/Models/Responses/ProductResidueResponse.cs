namespace Forex.ClientService.Models.Responses;

public class ProductResidueResponse
{
    public long Id { get; set; }
    public int Count { get; set; }  // 24-29,30-35 yoki 36-41 razmerlarda nechtadan borligi

    public long ProductTypeId { get; set; }
    public ProductTypeResponse ProductType { get; set; } = default!;

    public long ShopId { get; set; }
    public ShopResponse Shop { get; set; } = default!;

    public ICollection<ProductEntryResponse> ProductEntries { get; set; } = default!;
}