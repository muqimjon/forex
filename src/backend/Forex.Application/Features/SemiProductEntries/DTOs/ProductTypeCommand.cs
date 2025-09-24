namespace Forex.Application.Features.SemiProductEntries.DTOs;

public class ProductTypeCommand
{
    public string Type { get; set; } = string.Empty!;
    public ICollection<ProductTypeItemCommand> Items { get; set; } = default!;
}
