namespace Forex.Application.Common.Extensions;

using Forex.Application.Common.Interfaces;
using Forex.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

public static class PagingExtensions
{
    private const int MaxPageSize = 500;
    private const int MaxUnpagedItems = 10_000;

    public static async Task<IReadOnlyCollection<T>> ToPagedListAsync<T>(
        this IQueryable<T> query,
        FilteringRequest request,
        IPagingMetadataWriter? writer = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var filtered = query.AsFilterable(request);
        var total = await filtered.CountAsync(cancellationToken);

        if (request.Page <= 0 || request.PageSize <= 0)
            return await filtered.Take(MaxUnpagedItems).ToListAsync(cancellationToken);

        var page = request.Page;
        var pageSize = Math.Min(request.PageSize, MaxPageSize);

        var items = await filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        writer?.Write(new PagedListMetadata(total, page, pageSize,
            (int)Math.Ceiling((double)total / pageSize)));

        return items;
    }
}
