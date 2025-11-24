namespace Forex.ClientService.Models.Responses;

using Forex.ClientService.Enums;

public record ProductResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string PhotoPath { get; set; } = string.Empty;
    public ProductionOrigin ProductionOrigin { get; set; }

    public long UnitMeasureId { get; set; }
    public UnitMeasureResponse UnitMeasure { get; set; } = default!;
    public ICollection<ProductTypeResponse> ProductTypes { get; set; } = default!;

}
