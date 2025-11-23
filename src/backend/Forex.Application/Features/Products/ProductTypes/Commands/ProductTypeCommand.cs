namespace Forex.Application.Features.Products.ProductTypes.Commands;

using Forex.Application.Features.Products.ProductTypeItems.Commands;

public record ProductTypeCommand
{
    public long Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public int BundleItemCount { get; set; }
    public decimal UnitPrice { get; set; }
    public ICollection<ProductTypeItemCommand> ProductTypeItems { get; set; } = default!;
}