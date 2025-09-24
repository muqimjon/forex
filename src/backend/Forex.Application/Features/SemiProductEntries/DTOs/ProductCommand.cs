namespace Forex.Application.Features.SemiProductEntries.DTOs;

using Microsoft.AspNetCore.Http;

public class ProductCommand
{
    public string Name { get; set; } = string.Empty;
    public int Code { get; set; }
    public long MeasureId { get; set; }
    public IFormFile File { get; set; } = default!;

    public ICollection<ProductTypeCommand> Types { get; set; } = default!;
}
