namespace Forex.ClientService.Models.Manufactory;

public class ManufactoryDto
{
    public string Name { get; set; } = string.Empty;
    public List<SemiProductResidueDto>? SemiProducts { get; set; }
}
