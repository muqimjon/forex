namespace Forex.Application.Features.Processes.InProcesses.DTOs;

using Forex.Application.Features.Products.ProductTypes.DTOs;

public class InProcessDto
{
    public long Id { get; set; }
    public decimal Quantity { get; set; }

    public long ProductTypeId { get; set; }
    public ProductTypeDto ProductType { get; set; } = default!;
}
