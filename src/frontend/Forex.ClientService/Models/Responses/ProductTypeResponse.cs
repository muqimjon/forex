namespace Forex.ClientService.Models.Responses;
public record ProductTypeResponse
{
    public long ProductId { get; set; }
    public string Type { get; set; } = string.Empty;    //24-29, 30-35 , 36-41 razmeri
    public int Count { get; set; }     // 24-29 razmerda nechtadan borligi
}
