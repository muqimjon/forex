namespace Forex.ClientService.Models.Requests;
public sealed record InvoiceRequest
{
    public long SenderId { get; set; }
    public long ManufactoryId { get; set; }
    public DateTime EntryDate { get; set; }
    public decimal TransferFeePerContainer { get; set; }

    public List<ContainerRequest> Containers { get; set; } = [];
    public List<SemiProductRequest> Items { get; set; } = [];
}
