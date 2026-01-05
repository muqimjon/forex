namespace Forex.Application.Common.Extensions;

public static class DateTimeExtensions
{
    public static DateTime ToUtcSafe(this DateTime dateTime)
        => dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime();

    public static DateTime? ToUtcSafe(this DateTime? dateTime)
        => dateTime.HasValue ? dateTime.Value.ToUtcSafe() : dateTime;
}
