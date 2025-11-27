namespace Forex.ClientService.Models.Responses;

using Forex.ClientService.Enums;
using Forex.ClientService.Models.Requests;

public record OperationRecordTurnoverDto
{
    public decimal BeginBalance { get; set; }
    public decimal EndBalance { get; set; }
    public List<OperationRecordDto> OperationRecords { get; set; } = default!;
}

public record OperationRecordDto
{
    public long Id { get; set; }
    public OperationType Type { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;

    public SaleRequest? Sale { get; set; }
    public TransactionRequest? Transaction { get; set; }
}
