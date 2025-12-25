namespace Forex.ClientService.Models.Requests;

using Forex.ClientService.Enums;

public class ProductCommandRequest
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long UnitMeasureId { get; set; }
    public string? ImagePath { get; set; }
    public ProductionOrigin ProductionOrigin { get; set; }
    public List<ProductTypeCommandRequest> ProductTypes { get; set; } = [];
}
