namespace Forex.Application.Commons.Extensions;

using Forex.Application.Commons.Models;

public static class PagingExtensions
{
    public static IQueryable<T> AsPagable<T>(this IQueryable<T> query, PagingRequest request)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        return query.Skip((page - 1) * pageSize).Take(pageSize);
    }
}
