namespace Forex.WebApi.Models;

public class ProductRequest
{
    public string Name { get; set; } = default!;
    public int Code { get; set; }
    public string Measure { get; set; } = default!;
    public IFormFile? Photo { get; set; }
}
