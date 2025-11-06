namespace Forex.ClientService.Models.Requests;

public sealed record EntryToProcessRequest
{
    public long Id { get; set; }
    public decimal Quantity { get; set; }

    public long ProductTypeId { get; set; }
}