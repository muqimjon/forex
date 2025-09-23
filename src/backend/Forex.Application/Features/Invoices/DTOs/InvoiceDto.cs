namespace Forex.Application.Features.Invoices.DTOs;

using Forex.Application.Features.SemiProductEntries.DTOs;
using Forex.Domain.Entities.Manufactories;

public sealed record InvoiceDto
{
    public long Id { get; set; }
    public DateTime EntryDate { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CostDelivery { get; set; }
    public decimal TransferFee { get; set; }

    public ICollection<SemiProduct> SemiProducts { get; set; } = default!;
}