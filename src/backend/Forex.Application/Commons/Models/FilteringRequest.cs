namespace Forex.Application.Commons.Models;

public record FilteringRequest : PagingRequest
{
    /// <summary>Key-value filterlar: { "Role": "Supplier", "IsActive": true }</summary>
    public Dictionary<string, List<string>>? Filters { get; set; }

    /// <summary>Global search (string fieldlar bo‘yicha)</summary>
    public string? Search { get; set; }

    /// <summary>
    /// TimeZone offset in hours (e.g., +5 for Uzbekistan, -5 for EST)
    /// Used when filtering DateTime/DateTimeOffset fields without explicit timezone
    /// </summary>
    public double? TimeZone { get; set; }
}
