namespace Forex.ClientService.Models.Commons;

public record FilteringRequest
{
    public Dictionary<string, List<string>>? Filters { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? SortBy { get; set; }
    public bool Descending { get; set; }
}
