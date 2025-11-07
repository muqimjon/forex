namespace Forex.ClientService.Models.Requests;

public sealed record EntryToProcessRequest
{
    public long Id { get; set; }
    public int Count { get; set; }

    public long ProductTypeId { get; set; }
}