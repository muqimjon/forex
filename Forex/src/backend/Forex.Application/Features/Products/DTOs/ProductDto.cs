namespace Forex.Application.Features.Products.DTOs;

public record ProductDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Code { get; set; }
    public string Measure { get; set; } = string.Empty;
    public string PhotoPath { get; set; } = string.Empty;

    public ICollection<ProductItemDto> Items { get; set; } = [];
}