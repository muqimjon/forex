namespace Forex.Application.Commons.Extensions;

public static class StringExtensions
{
    public static string ToNormalized(this string value) =>
        value.ToUpperInvariant();
}
