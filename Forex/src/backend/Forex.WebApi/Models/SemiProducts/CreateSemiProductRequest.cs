namespace Forex.WebApi.Models.SemiProducts;

public class CreateSemiProductRequest
{
    public long ManufactoryId { get; set; }
    public string? Name { get; set; }
    public int Code { get; set; }
    public string Measure { get; set; } = default!;
    public IFormFile? Photo { get; set; }
}
