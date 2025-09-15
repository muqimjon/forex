namespace Forex.ClientService.Models.SemiProducts;

public class SemiProductIntakeCommand
{
    public long SenderId { get; set; }
    public long ManufactoryId { get; set; }
    public DateTime EntryDate { get; set; }
    public decimal TransferFeePerContainer { get; set; }

    public List<ContainerDto> Containers { get; set; } = default!;
    public List<ItemFormData> Items { get; set; } = default!;
}
public class ContainerDto
{
    public long Count { get; set; }
    public decimal Price { get; set; }
}

public class ItemFormData
{
    public long? SemiProductId { get; set; }
    public string? Name { get; set; }
    public int? Code { get; set; }
    public string? Measure { get; set; }

    // Faylni modelda saqlamaymiz — StreamPart builderda yaratiladi
    public string? PhotoFileName { get; set; }      // "chuchvara.jpg"
    public string? PhotoContentType { get; set; }   // "image/jpeg"
    public string? PhotoPath { get; set; }          // "C:\\Images\\chuchvara.jpg"

    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CostDelivery { get; set; }
    public decimal TransferFee { get; set; }
}

