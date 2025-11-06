namespace Forex.ClientService.Models.Responses;

public sealed record EntryToProcessResponse
{
    public long Id { get; set; }
    public decimal Quantity { get; set; }

    public long ProductTypeId { get; set; }
    public ProductTypeResponse ProductType { get; set; } = default!;
}