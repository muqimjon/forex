namespace Forex.WebApi.Models.SemiProducts;

public class UpdateSemiProductRequest
{
    public long Id { get; set; }
    public long ManufactoryId { get; set; }
    public string? Name { get; set; }
    public int Code { get; set; }
    public string Measure { get; set; } = default!;
    public IFormFile? Photo { get; set; }
}