namespace Forex.ClientService.Models.Requests;

using Forex.ClientService.Enums;
using System.Text.Json.Serialization;

public sealed record ProductRequest
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public long UnitMeasureId { get; set; }
    public string? ImagePath { get; set; }
    public ProductionOrigin ProductionOrigin { get; set; }

    public ICollection<ProductTypeRequest> ProductTypes { get; set; } = default!;
}
