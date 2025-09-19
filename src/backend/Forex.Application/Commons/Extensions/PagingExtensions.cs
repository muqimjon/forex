namespace Forex.Application.Commons.Extensions;

using Forex.Application.Commons.Interfaces;
using Forex.Application.Commons.Models;
using Microsoft.EntityFrameworkCore;

public static class PagingExtensions
{
    private static IPagingMetadataWriter? _writer;

    public static void ConfigureWriter(IPagingMetadataWriter writer)
    {
        _writer = writer;
    }

    public static async Task<IReadOnlyCollection<T>> ToPagedListAsync<T>(
        this IQueryable<T> query,
        FilteringRequest request,
        CancellationToken cancellationToken = default)
    {
        var filtered = query.AsFilterable(request);
        var total = await filtered.CountAsync(cancellationToken);

        if (request.Page <= 0 || request.PageSize <= 0)
            return await filtered.ToListAsync(cancellationToken);

        var page = request.Page;
        var pageSize = request.PageSize;

        var items = await filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        _writer?.Write(new PagedListMetadata(total, page, pageSize,
            (int)Math.Ceiling((double)total / pageSize)));

        return items;
    }
}
