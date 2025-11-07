namespace Forex.ClientService.Models.Requests;

public sealed record InProcessRequest
{
    public long Id { get; set; }
    public int Count { get; set; }

    public long ProductTypeId { get; set; }
}
