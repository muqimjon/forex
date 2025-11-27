namespace Forex.Application.Features.OperationRecords.DTOs;

using Forex.Application.Features.Sales.DTOs;
using Forex.Application.Features.Transactions.DTOs;
using Forex.Domain.Enums;

public record OperationRecordDto
{
    public long Id { get; set; }
    public OperationType Type { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;

    public SaleForOperationDto? Sale { get; set; }
    public TransactionForOperationDto? Transaction { get; set; }
}