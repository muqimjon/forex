namespace Forex.WebApi.Models;

public class ProductRequest
{
    public string Name { get; set; } = default!;
    public string Code { get; set; } = string.Empty;
    public string Measure { get; set; } = default!;
    public IFormFile? Photo { get; set; }
}
