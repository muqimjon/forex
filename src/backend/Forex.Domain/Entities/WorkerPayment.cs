namespace Forex.Domain.Entities;

using Forex.Domain.Commons;

public class WorkerPayment : Auditable
{
    public long ProductionBatchId { get; set; }
    public ProductionBatch Batch { get; set; } = default!;

    public long UserId { get; set; }
    public User User { get; set; } = default!;

    public long? ProductionStageId { get; set; }
    public ProductionStage? Stage { get; set; } = default!;

    public decimal? UnitRate { get; set; }
    public decimal? PaymentAmount { get; set; }
}
