namespace Forex.Application.Commons.Models;

public record SortingRequest
{
    /// <summary>Sort field nomi (masalan: "CreatedAt")</summary>
    public string? SortBy { get; set; }

    /// <summary>True bo‘lsa — DESC, aks holda ASC</summary>
    public bool Descending { get; set; }
}