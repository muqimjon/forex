namespace Forex.WebApi.Models;

using Forex.Application.Features.SemiProductEntries.DTOs;

public class SemiProductIntakeRequest
{
    public long SenderId { get; set; }
    public long ManufactoryId { get; set; }
    public DateTime EntryDate { get; set; }
    public decimal TransferFeePerContainer { get; set; }

    public List<ContainerDto> Containers { get; set; } = default!;
    public List<ItemFormDto> Items { get; set; } = default!;
}

public class ItemFormDto
{
    public long? SemiProductId { get; set; }
    public string? Name { get; set; }
    public int? Code { get; set; }
    public string? Measure { get; set; }

    public IFormFile? Photo { get; set; }

    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CostDelivery { get; set; }
    public decimal TransferFee { get; set; }
}
