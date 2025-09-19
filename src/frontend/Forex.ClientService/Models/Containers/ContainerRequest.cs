namespace Forex.ClientService.Models.Containers;

public record ContainerRequest
{
    public long Count { get; set; }
    public decimal Price { get; set; }
}