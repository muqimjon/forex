namespace Forex.Application.Features.OperationRecords.DTOs;

public record OperationRecordTurnoverDto
{
    public decimal BeginBalance { get; set; }
    public decimal EndBalance { get; set; }
    public ICollection<OperationRecordDto> OperationRecords { get; set; } = default!;
}
