namespace Forex.Application.Common.Models;

public record PagingRequest : SortingRequest
{
    /// <summary>Nechanchi sahifa (1-based)</summary>
    public int Page { get; set; }

    /// <summary>Har sahifada nechta element</summary>
    public int PageSize { get; set; }
}
