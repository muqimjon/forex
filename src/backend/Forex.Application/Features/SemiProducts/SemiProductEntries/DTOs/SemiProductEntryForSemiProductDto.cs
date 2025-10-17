namespace Forex.Application.Features.SemiProducts.SemiProductEntries.DTOs;

using Forex.Application.Features.Invoices.DTOs;
using Forex.Application.Features.Manufactories.DTOs;

public sealed record SemiProductEntryForSemiProductDto
{
    public long Id { get; set; }
    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CostDelivery { get; set; }
    public decimal TransferFee { get; set; }

    public long InvoiceId { get; set; }
    public InvoiceForSemiProductEntryDto Invoice { get; set; } = default!;

    public long SemiProductId { get; set; }

    public long ManufactoryId { get; set; }
    public ManufactoryForSemiProductEntryDto Manufactory { get; set; } = default!;
}