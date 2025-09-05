namespace Forex.Application.Features.Manufactories.DTOs;

using Forex.Application.Features.SemiProducts.DTOs;

public class ManufactoryDto
{
    public string Name { get; set; } = string.Empty;
    public ICollection<SemiProductDto> SemiProducts { get; set; } = [];
}