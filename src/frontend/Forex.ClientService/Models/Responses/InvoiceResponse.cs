namespace Forex.ClientService.Models.Responses;

public sealed record InvoiceResponse
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public string? Number { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CostDelivery { get; set; }
    public bool ViaConsolidator { get; set; }
    public int? ContainerCount { get; set; }
    public decimal? PricePerUnitContainer { get; set; }
    public decimal? ConsolidatorFee { get; set; }
    public decimal TotalAmount { get; set; }

    public long ManufactoryId { get; set; }
    public ManufactoryResponse Manufactory { get; set; } = default!;

    public ICollection<InvoicePaymentResponse> Payments { get; set; } = default!;
    public ICollection<SemiProductEntryResponse> SemiProductEntries { get; set; } = default!;
}
