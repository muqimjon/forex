namespace Forex.Application.Features.SemiProducts.SemiProductEntries.DTOs;

public sealed record InvoiceCommand
{
    public DateTime Date { get; set; }
    public string? Number { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CostDelivery { get; set; }
    public bool ViaMiddleman { get; set; }
    public int ContainerCount { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal? TransferFee { get; set; }
    public decimal TotalAmount { get; set; }

    public long CurrencyId { get; set; }
    public long ManufactoryId { get; set; }
    public long SupplierId { get; set; }
    public long? SenderId { get; set; }
}