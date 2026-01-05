namespace Forex.Application.Common.Extensions;

public static class StringExtensions
{
    public static string ToNormalized(this string value) =>
        value.ToUpperInvariant();
}
