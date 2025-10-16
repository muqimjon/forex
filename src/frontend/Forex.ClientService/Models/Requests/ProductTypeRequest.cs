namespace Forex.ClientService.Models.Requests;

public sealed record ProductTypeRequest
{
    public long Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
    public ICollection<ProductTypeItemRequest> Items { get; set; } = default!;
}