namespace Forex.Application.Features.Invoices.Invoices.DTOs;

using Forex.Application.Features.Invoices.InvoicePayments.DTOs;
using Forex.Application.Features.Manufactories.DTOs;

public sealed record InvoiceForSemiProductEntryDto
{
    public long Id { get; set; }
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
    public ManufactoryForInvoiceDto Manufactory { get; set; } = default!;

    public ICollection<InvoicePaymentForInvoiceDto> Payments { get; set; } = default!;
}