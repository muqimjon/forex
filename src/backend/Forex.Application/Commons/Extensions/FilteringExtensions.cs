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

            Expression body;

            if (prop.PropertyType == typeof(string))
            {
                var notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));

                var memberToLower = Expression.Call(member, nameof(string.ToLower), Type.EmptyTypes);
                var valueToLower = Expression.Constant(entry.Value.ToString()!.ToLower());

                var equals = Expression.Equal(memberToLower, valueToLower);
                body = Expression.AndAlso(notNull, equals);
            }
            else if (prop.PropertyType.IsEnum)
            {
                var enumValue = Enum.Parse(prop.PropertyType, entry.Value.ToString()!, ignoreCase: true);
                var constant = Expression.Constant(enumValue);
                body = Expression.Equal(member, constant);
            }
            else
            {
                var constant = Expression.Constant(Convert.ChangeType(entry.Value, prop.PropertyType));
                body = Expression.Equal(member, constant);
            }

            var lambda = Expression.Lambda<Func<T, bool>>(body, param);
            query = query.Where(lambda);
        }

        // 🔎 Global search (case-insensitive)
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var stringProps = typeof(T).GetProperties().Where(p => p.PropertyType == typeof(string));
            Expression? searchExpr = null;

            foreach (var p in stringProps)
            {
                var member = Expression.Property(param, p.Name);

                var notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
                var memberToLower = Expression.Call(member, nameof(string.ToLower), Type.EmptyTypes);
                var searchValue = Expression.Constant(request.Search.ToLower());
                var contains = Expression.Call(memberToLower, nameof(string.Contains), Type.EmptyTypes, searchValue);

                var condition = Expression.AndAlso(notNull, contains);
                searchExpr = searchExpr is null ? condition : Expression.OrElse(searchExpr, condition);
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