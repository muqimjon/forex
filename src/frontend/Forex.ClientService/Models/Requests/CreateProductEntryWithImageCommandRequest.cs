namespace Forex.ClientService.Models.Requests;

public class CreateProductEntryWithImageCommandRequest
{
    public ProductEntryCommandRequest Command { get; set; } = new();
}
