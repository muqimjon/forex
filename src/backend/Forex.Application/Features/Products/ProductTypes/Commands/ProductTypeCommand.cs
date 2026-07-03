namespace Forex.Application.Features.Products.ProductTypes.Commands;

public record ProductTypeCommand
{
    public long Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public int BundleItemCount { get; set; }
    public int PackItemCount { get; set; }
    public decimal UnitPrice { get; set; }
    public int Count { get; set; }
    public int BundleCount { get; set; }
    public string? QopBarcode { get; set; }
    public string? PackBarcode { get; set; }
}