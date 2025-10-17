namespace Forex.ClientService.Models.Requests;

using System.Text.Json.Serialization;

public sealed record ProductRequest
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Code { get; set; }
    public long UnitMeasureId { get; set; }
    public string? ImagePath { get; set; }

    public ICollection<ProductTypeRequest> ProductTypes { get; set; } = default!;

    [JsonIgnore]
    public byte[]? ImageBytes { get; set; }
}
