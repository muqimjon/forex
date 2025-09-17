namespace Forex.Application.Commons.Extensions;

using Forex.Application.Commons.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public static class FilteringExtensions
{
    public static IQueryable<T> AsFilterable<T>(this IQueryable<T> query, FilteringRequest request)
    {
        var param = Expression.Parameter(typeof(T), "x");

        foreach (var entry in request.Filters ?? [])
        {
            var prop = typeof(T).GetProperty(entry.Key);
            if (prop == null) continue;

            var member = Expression.Property(param, prop.Name);
            var constant = Expression.Constant(Convert.ChangeType(entry.Value, prop.PropertyType));
            var body = Expression.Equal(member, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(body, param);
            query = query.Where(lambda);
        }

        // Global search
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var stringProps = typeof(T).GetProperties().Where(p => p.PropertyType == typeof(string));
            Expression? searchExpr = null;

            foreach (var p in stringProps)
            {
                var member = Expression.Property(param, p.Name);
                var search = Expression.Constant(request.Search);
                var contains = Expression.Call(member, nameof(string.Contains), Type.EmptyTypes, search);
                searchExpr = searchExpr is null ? contains : Expression.OrElse(searchExpr, contains);
            }

            if (searchExpr is not null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(searchExpr, param);
                query = query.Where(lambda);
            }
        }

        return query.AsSortable(request).AsPagable(request);
    }

    public static async Task<PagedList<T>> ToPagedListAsync<T>(
        this IQueryable<T> query,
        FilteringRequest request,
        CancellationToken cancellationToken = default)
    {
        var filtered = query.AsFilterable(request);
        var total = await filtered.CountAsync(cancellationToken);
        var items = await filtered.ToListAsync(cancellationToken);

        return new PagedList<T>(items, total, request.Page, request.PageSize);
    }
}
