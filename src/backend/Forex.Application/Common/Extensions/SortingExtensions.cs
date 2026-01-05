namespace Forex.Application.Common.Extensions;

using Forex.Application.Common.Models;

public static class SortingExtensions
{
    public static IQueryable<T> AsSortable<T>(this IQueryable<T> query, SortingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SortBy))
            return query;

        return request.Descending
            ? query.OrderByDescendingDynamic(request.SortBy)
            : query.OrderByDynamic(request.SortBy);
    }
}
