namespace Forex.Application.Features.Processes.InProcess.DTOs;

using Forex.Application.Features.Products.ProductTypes.DTOs;

public class EntryToProcessDto
{
    public long Id { get; set; }
    public int Count { get; set; }

    public long ProductTypeId { get; set; }
    public ProductTypeDto ProductType { get; set; } = default!;

    public long InProcessId { get; set; }
    public InProcessDto InProcess { get; set; } = default!;
}