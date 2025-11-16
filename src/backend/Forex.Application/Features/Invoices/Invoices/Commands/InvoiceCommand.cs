namespace Forex.Application.Features.Invoices.Invoices.Commands;

using Forex.Application.Features.Invoices.InvoicePayments.Commands;

public sealed record InvoiceCommand
{
    public DateTime Date { get; set; }
    public string? Number { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CostDelivery { get; set; }
    public bool ViaConsolidator { get; set; }
    public int? ContainerCount { get; set; }
    public decimal? PricePerUnitContainer { get; set; }
    public decimal? ConsolidatorFee { get; set; }
    public decimal TotalAmount { get; set; }
    public long ManufactoryId { get; set; }
    public ICollection<InvoicePaymentCommand> Payments { get; set; } = default!;
}
