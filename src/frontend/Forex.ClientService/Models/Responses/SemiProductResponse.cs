namespace Forex.ClientService.Models.Responses;

public sealed record SemiProductResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Code { get; set; }
    public string? ImagePath { get; set; }
    public long UnitMeasureId { get; set; }
    public UnitMeasureResponse UnitMeasure { get; set; } = default!;

}
