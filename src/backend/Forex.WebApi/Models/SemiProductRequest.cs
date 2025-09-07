namespace Forex.WebApi.Models;

public class SemiProductRequest
{
    public long ManufactoryId { get; set; }
    public string? Name { get; set; }
    public int Code { get; set; }
    public string Measure { get; set; } = default!;
    public IFormFile? Photo { get; set; }
}