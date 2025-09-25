namespace Forex.Application.Features.SemiProductEntries.DTOs;

public class InvoiceCommand
{
    public DateTime EntryDate { get; set; }
    public string? Number { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CostDelivery { get; set; }
    public UserCommand Supplier { get; set; } = default!;
    public bool ViaMiddleman { get; set; }
    public UserCommand? Sender { get; set; } = default!;
    public int ContainerCount { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal? TransferFee { get; set; }
    public long CurrencyId { get; set; }
    public decimal TotalSum { get; set; }
    public long ManufactoryId { get; set; }
}
