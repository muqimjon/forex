namespace Forex.Domain.Entities.Shops;

using Forex.Domain.Commons;

public class ProductType : Auditable
{
    public long ProductId { get; set; }
    public string Type { get; set; } = string.Empty;    //24-29, 30-35 , 36-41 razmeri
    public int Count { get; set; }     // 24-29 razmerda nechtadan borligi

    public ProductResidue ProductResidue { get; set; } = default!;
    public ICollection<ProductTypeItem> Items { get; set; } = default!;
}