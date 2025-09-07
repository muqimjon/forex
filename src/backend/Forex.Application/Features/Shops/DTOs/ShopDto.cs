namespace Forex.Application.Features.Shops.DTOs;

public record ShopDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}