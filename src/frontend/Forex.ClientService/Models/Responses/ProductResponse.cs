namespace Forex.ClientService.Models.Responses;

public record ProductResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Code { get; set; }
    public string Measure { get; set; } = string.Empty;
    public string PhotoPath { get; set; } = string.Empty;
    public ICollection<ProductTypeResponse> ProductTypes { get; set; } = default!;

}
