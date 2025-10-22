namespace Forex.ClientService.Models.Requests;

public sealed record ProductEntryRequest
{
    public long Id { get; set; }
    public int Count { get; set; }
    public decimal CostPrice { get; set; }     // tannarxi
    public decimal CostPreparation { get; set; }  // tayyorlashga ketgan xarajat summasi

    public long ProductTypeId { get; set; }
    public long ShopId { get; set; }
    public long EmployeeId { get; set; }
}