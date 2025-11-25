namespace Forex.Domain.Entities;

using Forex.Domain.Commons;
using Forex.Domain.Entities.Sales;
using Forex.Domain.Enums;

public class OperationRecord : Auditable
{
    public OperationType Type { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;

    public Sale? Sale { get; set; }
    public Transaction? Transaction { get; set; }
}