namespace Forex.Application.Features.Processes.DTOs;

using Forex.Application.Features.SemiProducts.SemiProducts.DTOs;

public class InProcessDto
{
    public long Id { get; set; }
    public decimal Quantity { get; set; }

    public long SemiProductId { get; set; }
    public SemiProductDto SemiProduct { get; set; } = default!;
}
