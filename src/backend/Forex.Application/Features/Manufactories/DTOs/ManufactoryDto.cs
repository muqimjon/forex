namespace Forex.Application.Features.Manufactories.DTOs;

public sealed class ManufactoryDto
{
    public string Name { get; init; } = string.Empty;
    public ICollection<SemiProductResidueDto> SemiProducts { get; set; } = default!;
}
