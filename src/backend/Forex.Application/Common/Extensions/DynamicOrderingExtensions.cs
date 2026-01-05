namespace Forex.Application.Common.Extensions;

using Forex.Application.Common.Exceptions;
using System.Linq.Expressions;

public static class DynamicOrderingExtensions
{
    public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> source, string propertyName)
    {
        var param = Expression.Parameter(typeof(T), "x");

        var prop = typeof(T).GetProperties()
            .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            ?? throw new NotFoundException(typeof(T).Name, "Property", propertyName);

        var property = Expression.Property(param, prop.Name);
        var lambda = Expression.Lambda(property, param);

        var method = typeof(Queryable).GetMethods()
            .First(m => m.Name == "OrderBy" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), prop.PropertyType);

        return (IQueryable<T>)method.Invoke(null, [source, lambda])!;
    }


    public static IQueryable<T> OrderByDescendingDynamic<T>(this IQueryable<T> source, string propertyName)
    {
        var param = Expression.Parameter(typeof(T), "x");

        var prop = typeof(T).GetProperties()
            .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            ?? throw new NotFoundException(typeof(T).Name, "Property", propertyName);

        var property = Expression.Property(param, prop.Name);
        var lambda = Expression.Lambda(property, param);

        var method = typeof(Queryable).GetMethods()
            .First(m => m.Name == "OrderByDescending" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), prop.PropertyType);

        return (IQueryable<T>)method.Invoke(null, [source, lambda])!;
    }

    public static IOrderedQueryable<T> ThenByDynamic<T>(this IOrderedQueryable<T> source, string propertyName)
    {
        var param = Expression.Parameter(typeof(T), "x");

        var prop = typeof(T).GetProperties()
            .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            ?? throw new NotFoundException(typeof(T).Name, "Property", propertyName);

        var property = Expression.Property(param, prop.Name);
        var lambda = Expression.Lambda(property, param);

        var method = typeof(Queryable).GetMethods()
            .First(m => m.Name == "ThenBy" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), prop.PropertyType);

        return (IOrderedQueryable<T>)method.Invoke(null, [source, lambda])!;
    }

    public static IOrderedQueryable<T> ThenByDescendingDynamic<T>(this IOrderedQueryable<T> source, string propertyName)
    {
        var param = Expression.Parameter(typeof(T), "x");

        var prop = typeof(T).GetProperties()
            .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            ?? throw new NotFoundException(typeof(T).Name, "Property", propertyName);

        var property = Expression.Property(param, prop.Name);
        var lambda = Expression.Lambda(property, param);

        var method = typeof(Queryable).GetMethods()
            .First(m => m.Name == "ThenByDescending" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), prop.PropertyType);

        return (IOrderedQueryable<T>)method.Invoke(null, [source, lambda])!;
    }
}
