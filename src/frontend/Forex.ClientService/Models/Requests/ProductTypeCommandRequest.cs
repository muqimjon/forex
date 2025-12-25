namespace Forex.ClientService.Models.Requests;

public class ProductTypeCommandRequest
{
    public long Id { get; set; }
    public string Type { get; set; } = string.Empty;
}
