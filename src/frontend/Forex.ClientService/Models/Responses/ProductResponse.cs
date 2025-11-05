namespace Forex.ClientService.Models.Responses;

public record ProductResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string PhotoPath { get; set; } = string.Empty;

    public long UnitMeasureId { get; set; }
    public UnitMeasureResponse UnitMeasure { get; set; } = default!;
    public ICollection<ProductTypeResponse> ProductTypes { get; set; } = default!;

}
