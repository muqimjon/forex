namespace Forex.ClientService.Models.Responses;

public sealed record ManufactoryResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<SemiProductResidueResponse>? SemiProducts { get; set; }
}
