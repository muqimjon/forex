namespace Forex.ClientService.Models.Responses;

public record ProductTypeResponse
{
    public long Id { get; set; }
    public string Type { get; set; } = string.Empty;    //24-29, 30-35 , 36-41 razmeri
    public int BundleItemCount { get; set; }     // 24-29 razmerda nechtadan borligi
    public int PackItemCount { get; set; }
    public decimal UnitPrice { get; set; }

    public string? QopBarcode { get; set; }
    public string? PackBarcode { get; set; }

    public long ProductId { get; set; }
    public ProductResponse Product { get; set; } = default!;

    public ProductResidueResponse ProductResidue { get; set; } = default!;
    public ICollection<ProductEntryResponse> ProductEntries { get; set; } = default!;
}