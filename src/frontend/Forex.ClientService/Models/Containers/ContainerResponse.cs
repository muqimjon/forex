namespace Forex.ClientService.Models.Containers;

public record ContainerResponse
{
    public long Count { get; set; }
    public decimal Price { get; set; }
}