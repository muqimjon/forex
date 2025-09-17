namespace Forex.ClientService.Models.Manufactory;

using Forex.ClientService.Models.SemiProducts;

public class SemiProductResidueDto
{
    public long Id { get; set; }
    public long SemiProductId { get; set; }
    public long ManufactoryId { get; set; }
    public decimal Quantity { get; set; }
    public SemiProductDto SemiProduct { get; set; } = default!;
}