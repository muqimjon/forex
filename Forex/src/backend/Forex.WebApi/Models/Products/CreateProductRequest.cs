namespace Forex.WebApi.Models.Products;

public class CreateProductRequest
{
    public string Name { get; set; } = default!;
    public int Code { get; set; }
    public string Measure { get; set; } = default!;
    public IFormFile? Photo { get; set; }
}
