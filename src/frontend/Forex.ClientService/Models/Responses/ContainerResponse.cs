namespace Forex.ClientService.Models.Responses;

public record ContainerResponse
{
    public long Count { get; set; }
    public decimal Price { get; set; }
}