namespace Forex.Application.Features.Invoices.DTOs;

using Forex.Application.Features.SemiProductEntries.DTOs;

public sealed record InvoiceDto
{
    public long Id { get; set; }
    public DateTime EntryDate { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CostDelivery { get; set; }
    public decimal TransferFee { get; set; }

    public ICollection<ContainerDto> ContainerEntries { get; set; } = default!;
    public ICollection<SemiProductEntryDto> SemiProductEntries { get; set; } = default!;
}