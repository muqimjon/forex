namespace Forex.ClientService.Models.Responses;

public record ProductResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Code { get; set; }
    public string PhotoPath { get; set; } = string.Empty;

    public long MeasureId { get; set; }
    public UnitMeasureResponse Measure { get; set; } = default!;
    public ICollection<ProductTypeResponse> ProductTypes { get; set; } = default!;

}
