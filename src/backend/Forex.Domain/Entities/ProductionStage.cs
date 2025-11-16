namespace Forex.Domain.Entities;

using Forex.Domain.Commons;
using Forex.Domain.Entities.Products;

public class ProductionStage : Auditable
{
    public string Name { get; set; } = default!;

    public decimal? UnitRate { get; set; }

    public long? ProductTypeId { get; set; }
    public ProductType? ProductType { get; set; }
}