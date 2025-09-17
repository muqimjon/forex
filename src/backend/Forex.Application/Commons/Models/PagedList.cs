namespace Forex.Application.Commons.Models;

public record PagedList<T>
{
    /// <summary>Joriy sahifadagi elementlar</summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>Jami elementlar soni (filterni hisobga olgan holda)</summary>
    public int TotalCount { get; }

    /// <summary>Joriy sahifa raqami (1-based)</summary>
    public int Page { get; }

    /// <summary>Har sahifadagi elementlar soni</summary>
    public int PageSize { get; }

    /// <summary>Jami sahifalar soni</summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>Sahifalash mavjudmi (ya’ni TotalCount > PageSize)</summary>
    public bool HasPagination => TotalPages > 1;

    public PagedList(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }
}
