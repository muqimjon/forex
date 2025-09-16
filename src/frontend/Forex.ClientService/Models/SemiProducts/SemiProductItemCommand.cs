namespace Forex.ClientService.Models.SemiProducts;

public class SemiProductItemCommand
{
    public long? SemiProductId { get; set; }
    public string? Name { get; set; }
    public int? Code { get; set; }
    public string? Measure { get; set; }

    public string? PhotoPath { get; set; }   // UI’dan fayl yo‘lini olamiz
    public string? PhotoContentType { get; set; }
    public string? PhotoFileName { get; set; }

    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CostDelivery { get; set; }
    public decimal TransferFee { get; set; }
}
