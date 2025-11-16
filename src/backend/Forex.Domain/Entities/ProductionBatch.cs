namespace Forex.Domain.Entities;

using Forex.Domain.Commons;
using Forex.Domain.Entities.Products;
using System.Collections.Generic;

public class ProductionBatch : Auditable
{
    public long ProductTypeId { get; set; }
    public ProductType ProductType { get; set; } = default!;

    public int Count { get; set; }

    public DateTime ProductionDate { get; set; } = DateTime.UtcNow;

    public decimal UnitCost { get; set; }

    public ICollection<WorkerPayment> WorkerPayments { get; set; } = default!;
}
