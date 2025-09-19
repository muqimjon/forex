namespace Forex.ClientService.Models.Invoices;

using Forex.ClientService.Models.Containers;
using Forex.ClientService.Models.SemiProducts;

public sealed record InvoiceResponse
{
    public long Id { get; set; }
    public DateTime EntryDate { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CostDelivery { get; set; }
    public decimal TransferFee { get; set; }

    public ICollection<ContainerResponse> ContainerEntries { get; set; } = default!;
    public ICollection<SemiProductEntryResponse> SemiProductEntries { get; set; } = default!;
}