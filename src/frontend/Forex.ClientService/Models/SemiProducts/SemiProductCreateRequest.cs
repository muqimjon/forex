namespace Forex.ClientService.Models.SemiProducts;

public class SemiProductCreateRequest
{
    public long ManufactoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Code { get; set; }
    public string Measure { get; set; } = string.Empty;

    public Stream? Photo { get; set; }
    public string? ContentType { get; set; }
    public string? Extension { get; set; }
}
