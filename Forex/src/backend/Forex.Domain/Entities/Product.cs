namespace Forex.Domain.Entities;

using Forex.Domain.Commons;

public class Product : Auditable
{
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public int Code { get; set; }
    public string Measure { get; set; } = string.Empty;
    public string PhotoPath { get; set; } = string.Empty;

    public ICollection<ProductItem> Items { get; set; } = [];
}