namespace Forex.WebApi.Models.Products;

public class UpdateProductRequest
{
    public long Id { get; set; }
    public string Name { get; set; } = default!;
    public int Code { get; set; }
    public string Measure { get; set; } = default!;
    public IFormFile? Photo { get; set; }
}
