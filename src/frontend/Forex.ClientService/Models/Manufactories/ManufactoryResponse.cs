namespace Forex.ClientService.Models.Manufactories;

using Forex.ClientService.Models.SemiProducts;

public sealed record ManufactoryResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<SemiProductResidueResponse>? SemiProducts { get; set; }
}
