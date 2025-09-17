namespace Forex.ClientService.Models.Commons;
public record FilteringRequest : PagingRequest
{
    /// <summary>Key-value filterlar: { "Role": "Supplier", "IsActive": true }</summary>
    public Dictionary<string, object>? Filters { get; set; }

    /// <summary>Global search (string fieldlar bo‘yicha)</summary>
    public string? Search { get; set; }
}
