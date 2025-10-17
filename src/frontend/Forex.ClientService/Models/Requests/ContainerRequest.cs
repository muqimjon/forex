namespace Forex.ClientService.Models.Requests;

public sealed record ContainerRequest
{
    public long Count { get; set; }
    public decimal Price { get; set; }
}