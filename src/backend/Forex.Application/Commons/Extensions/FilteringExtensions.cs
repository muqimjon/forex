namespace Forex.Application.Commons.Extensions;

using Forex.Application.Commons.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


public static class FilteringExtensions
{
    public static IQueryable<T> AsFilterable<T>(this IQueryable<T> query, FilteringRequest request)
    where T : class
    {
        query = ApplyPropertyIncludes(query, request);

        var param = Expression.Parameter(typeof(T), "x");
        var props = typeof(T).GetProperties();

        foreach (var entry in request.Filters ?? [])
        {
            var key = entry.Key;

            if (entry.Value.All(v => v.StartsWith("include", StringComparison.OrdinalIgnoreCase)))
                continue;

            var prop = props.FirstOrDefault(p =>
                string.Equals(p.Name, key, StringComparison.OrdinalIgnoreCase));
            if (prop is null) continue;

            var member = Expression.Property(param, prop.Name);
            var filterExpr = BuildCombinedCondition(member, entry.Value, prop.PropertyType);

            if (filterExpr is not null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(filterExpr, param);
                query = query.Where(lambda);
            }
        }

        var searchExpr = BuildGlobalSearchExpression<T>(request.Search, param);
        if (searchExpr is not null)
        {
            var lambda = Expression.Lambda<Func<T, bool>>(searchExpr, param);
            query = query.Where(lambda);
        }

        return query.AsSortable(request);
    }

    #region Logical Operator Detection

    private static ExpressionType DetectLogicalOperator(List<string> values)
    {
        var tokens = values.Select(v => v.Trim().ToLower()).ToList();

        if (tokens.Contains("or") || tokens.Contains("||") || tokens.Contains("|"))
            return ExpressionType.OrElse;

        return ExpressionType.AndAlso;
    }

    private static bool IsLogicalToken(string token)
    {
        var t = token.Trim().ToLower();
        return t is "and" or "&&" or "&" or "or" or "||" or "|";
    }

    #endregion

    #region Condition Building

    private static Expression? BuildCondition(Expression member, string raw, Type targetType)
    {
        var opMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [">="] = ">=",
            ["<="] = "<=",
            [">"] = ">",
            ["<"] = "<",
            ["="] = "=",
            ["contains:"] = "contains",
            ["starts:"] = "starts",
            ["ends:"] = "ends",
            ["equals:"] = "equals",
            ["in:"] = "in",
            ["!"] = "not"
        };

        string op = "=";
        string value = raw;

        foreach (var kvp in opMap)
        {
            if (raw.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                op = kvp.Value;
                value = raw[kvp.Key.Length..];
                break;
            }
        }

        // NOT operator special case
        if (op == "not")
        {
            var inner = BuildCondition(member, value, targetType);
            return inner is not null ? Expression.Not(inner) : null;
        }

        // IN operator
        if (op == "in")
        {
            var values = value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(v => ConversionHelper.TryConvert(v.Trim(), targetType))
                .ToList();

            var arrayExpr = Expression.Constant(values);
            return Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), [targetType], arrayExpr, member);
        }

        // Convert single value
        object? converted;
        try { converted = ConversionHelper.TryConvert(value, targetType); }
        catch { return null; }

        var constant = Expression.Constant(converted, targetType);

        // String operations
        if (targetType == typeof(string))
        {
            var memberToLower = Expression.Call(member, nameof(string.ToLower), Type.EmptyTypes);
            var constantToLower = Expression.Call(constant, nameof(string.ToLower), Type.EmptyTypes);

            return op switch
            {
                "contains" => Expression.Call(memberToLower, nameof(string.Contains), Type.EmptyTypes, constantToLower),
                "starts" => Expression.Call(memberToLower, nameof(string.StartsWith), Type.EmptyTypes, constantToLower),
                "ends" => Expression.Call(memberToLower, nameof(string.EndsWith), Type.EmptyTypes, constantToLower),
                "equals" => Expression.Call(memberToLower, nameof(string.Equals), Type.EmptyTypes, constantToLower, Expression.Constant(StringComparison.OrdinalIgnoreCase)),
                _ => null
            };
        }

        // Standard comparisons
        return op switch
        {
            "=" => Expression.Equal(member, constant),
            ">" => Expression.GreaterThan(member, constant),
            ">=" => Expression.GreaterThanOrEqual(member, constant),
            "<" => Expression.LessThan(member, constant),
            "<=" => Expression.LessThanOrEqual(member, constant),
            _ => null
        };
    }

    private static Expression? BuildCombinedCondition(Expression member, List<string> values, Type targetType)
    {
        var logic = DetectLogicalOperator(values);
        var conditions = values
            .Where(v => !string.IsNullOrWhiteSpace(v) && !IsLogicalToken(v))
            .Select(v => BuildCondition(member, v, targetType))
            .Where(c => c is not null)
            .ToList();

        if (conditions.Count == 0)
            return null;

        Expression expr = conditions[0]!;

        for (int i = 1; i < conditions.Count; i++)
        {
            expr = logic == ExpressionType.AndAlso
                ? Expression.AndAlso(expr, conditions[i]!)
                : Expression.OrElse(expr, conditions[i]!);
        }

        return expr;
    }

    #endregion

    #region Global Search

    private static Expression? BuildGlobalSearchExpression<T>(string? search, ParameterExpression param)
    {
        if (string.IsNullOrWhiteSpace(search))
            return null;

        var stringProps = typeof(T).GetProperties()
            .Where(p => p.PropertyType == typeof(string));

        Expression? searchExpr = null;

        foreach (var p in stringProps)
        {
            var member = Expression.Property(param, p.Name);
            var notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
            var memberToLower = Expression.Call(member, nameof(string.ToLower), Type.EmptyTypes);
            var searchValue = Expression.Constant(search.ToLower());
            var contains = Expression.Call(memberToLower, nameof(string.Contains), Type.EmptyTypes, searchValue);
            var condition = Expression.AndAlso(notNull, contains);

            searchExpr = searchExpr is null ? condition : Expression.OrElse(searchExpr, condition);
        }

        return searchExpr;
    }

    #endregion

    #region Include

    private static IQueryable<T> ApplyPropertyIncludes<T>(IQueryable<T> query, FilteringRequest request)
    where T : class
    {
        var props = typeof(T).GetProperties();

        foreach (var entry in request.Filters ?? [])
        {
            var key = entry.Key;
            var prop = props.FirstOrDefault(p =>
                string.Equals(p.Name, key, StringComparison.OrdinalIgnoreCase));
            if (prop is null) continue;

            var propName = prop.Name;

            foreach (var value in entry.Value)
            {
                if (value.Equals("include", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Include(propName);
                }
                else if (value.StartsWith("include:", StringComparison.OrdinalIgnoreCase))
                {
                    var path = value["include:".Length..].Trim();

                    var fullPath = $"{propName}.{path}";
                    query = query.Include(fullPath);
                }
            }
        }

        return query;
    }

    #endregion
}
