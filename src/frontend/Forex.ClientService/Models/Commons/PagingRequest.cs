namespace Forex.ClientService.Models.Commons;

using System.ComponentModel.DataAnnotations;

public record PagingRequest : SortingRequest
{
    [Required]
    /// <summary>Nechanchi sahifa (1-based)</summary>
    public int Page { get; set; }

    [Required]
    /// <summary>Har sahifada nechta element</summary>
    public int PageSize { get; set; }
}
