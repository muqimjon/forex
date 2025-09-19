namespace Forex.Application.Commons.Extensions;

using Forex.Application.Commons.Models;
using System.Linq.Expressions;

public static class FilteringExtensions
{
    public static IQueryable<T> AsFilterable<T>(this IQueryable<T> query, FilteringRequest request)
    {
        var param = Expression.Parameter(typeof(T), "x");
        var props = typeof(T).GetProperties();

        foreach (var entry in request.Filters ?? [])
        {
            var prop = props.FirstOrDefault(p =>
                string.Equals(p.Name, entry.Key, StringComparison.OrdinalIgnoreCase));
            if (prop is null) continue;

            var member = Expression.Property(param, prop.Name);
            Expression? filterExpr = null;

            foreach (var raw in entry.Value)
            {
                var condition = BuildCondition(member, raw, prop.PropertyType);
                if (condition is not null)
                    filterExpr = filterExpr is null ? condition : Expression.OrElse(filterExpr, condition);
            }

            if (filterExpr is not null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(filterExpr, param);
                query = query.Where(lambda);
            }
        }

        // 🔍 Global search
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var stringProps = typeof(T).GetProperties()
                .Where(p => p.PropertyType == typeof(string));

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

        return query.AsSortable(request);
    }

    private static Expression? BuildCondition(Expression member, string raw, Type targetType)
    {
        string value = raw;
        string op = "=";

        if (raw.StartsWith(">=")) { op = ">="; value = raw[2..]; }
        else if (raw.StartsWith("<=")) { op = "<="; value = raw[2..]; }
        else if (raw.StartsWith(">")) { op = ">"; value = raw[1..]; }
        else if (raw.StartsWith("<")) { op = "<"; value = raw[1..]; }
        else if (raw.StartsWith("contains:", StringComparison.OrdinalIgnoreCase))
        {
            op = "contains"; value = raw["contains:".Length..];
        }

        object? converted;
        try
        {
            converted = ConversionHelper.TryConvert(value, targetType);
        }
        catch
        {
            return null;
        }

        var constant = Expression.Constant(converted, targetType);

        return op switch
        {
            "=" => Expression.Equal(member, constant),
            ">" => Expression.GreaterThan(member, constant),
            ">=" => Expression.GreaterThanOrEqual(member, constant),
            "<" => Expression.LessThan(member, constant),
            "<=" => Expression.LessThanOrEqual(member, constant),
            "contains" when targetType == typeof(string) =>
                Expression.Call(member, nameof(string.Contains), Type.EmptyTypes, Expression.Constant(value)),
            _ => null
        };
    }
}
