namespace Forex.Application.Features.OperationRecords.DTOs;

using Forex.Application.Features.Transactions.DTOs;
using Forex.Domain.Enums;

public sealed record OperationRecordForSaleDto
{
    public long Id { get; set; }
    public OperationType Type { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;

    public TransactionForOperationDto? Transaction { get; set; }
}