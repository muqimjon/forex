namespace Forex.Application.Commons.Extensions;

using Forex.Application.Commons.Exceptions;
using System.Globalization;
using System.Text.Json;

public static class ConversionHelper
{
    private static readonly string[] DateFormats =
    [
        // 1. ISO 8601 & Round-trip (ToString("O") uchun - Eng birinchi turishi shart)
        "yyyy-MM-ddTHH:mm:ss.FFFFFFFK",
        "yyyy-MM-ddTHH:mm:ss.fffffffzzz",
        "yyyy-MM-ddTHH:mm:ssZ",
        "yyyy-MM-ddTHH:mm:sszzz",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-dd",

        // 2. Nuqtali (O‘zbekiston/Yevropa) - Nuqta bo'lgani uchun boshqalar bilan chalkashmaydi
        "d.M.yyyy HH:mm:ss",
        "d.M.yyyy HH:mm",
        "d.M.yyyy",
        "dd.MM.yyyy.HH:mm:ss",

        // 3. Yil birinchi keladigan (Custom) - Yil birinchi kelsa adashish ehtimoli 0%
        "yyyy/M/d HH:mm:ss",
        "yyyy/M/d HH:mm",
        "yyyy/M/d",
        "yyyy-M-d HH:mm:ss",
        "yyyy-M-d HH:mm",
        "yyyy-M-d",

        // 4. Chiziqchali (Kun birinchi) - O'zbekistonda ko'p qo'llaniladi
        "d-M-yyyy HH:mm:ss",
        "d-M-yyyy HH:mm",
        "d-M-yyyy",

        // 5. Sleshli (Xalqaro - Kun birinchi) 
        "d/M/yyyy HH:mm:ss",
        "d/M/yyyy HH:mm",
        "d/M/yyyy",

        // 6. Sleshli (AQSH - Oy birinchi) - ENG OXIRIDA
        // Chunki 10/11/2025 kelsa, yuqoridagi "Kun birinchi" qoidasi ustunlik qilishi kerak
        "M/d/yyyy HH:mm:ss",
        "M/d/yyyy HH:mm",
        "M/d/yyyy",

        // 7. Matnli formatlar
        "d MMM yyyy",
        "MMM d, yyyy",
        "MMMM d yyyy"
    ];

    public static object? TryConvert(object value, Type targetType)
    {
        targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (value is JsonElement json)
            value = json.ExtractJsonValue()
                ?? throw new AppException($"'{targetType.Name}' turiga null qiymatni o‘tkazib bo‘lmaydi.");

        var strValue = value?.ToString();
        if (string.IsNullOrWhiteSpace(strValue))
        {
            if (Nullable.GetUnderlyingType(targetType) != null)
                return null;

            throw new AppException($"Bo‘sh yoki null qiymat '{targetType.Name}' turiga mos emas.");
        }

        if (targetType == typeof(Guid))
            return Guid.TryParse(strValue, out var guid)
                ? guid
                : throw new AppException($"'{strValue}' — to‘g‘ri Guid emas.");

        if (targetType == typeof(DateTime))
            return ParseFlexibleDate(strValue);

        if (targetType == typeof(DateTimeOffset))
            return ParseFlexibleDateTimeOffset(strValue);

        if (targetType.IsEnum)
            return Enum.TryParse(targetType, strValue, ignoreCase: true, out var enumVal)
                ? enumVal
                : throw new AppException($"'{strValue}' — '{targetType.Name}' enum qiymatiga mos emas.");

        if (value is IConvertible)
            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);

        throw new AppException($"Qiymat '{value}' turga o‘zgartirib bo‘lmaydi: {targetType.Name}");
    }

    public static object? ExtractJsonValue(this JsonElement json) => json.ValueKind switch
    {
        JsonValueKind.String => json.GetString(),
        JsonValueKind.Number => json.TryGetInt64(out var l) ? l : json.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null => null,
        _ => json.ToString()
    };

    public static DateTime ParseFlexibleDate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new AppException("Sana qiymati bo‘sh yoki null bo‘lishi mumkin emas.");

        foreach (var format in DateFormats)
            if (DateTime.TryParseExact(input, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed))
                return parsed;

        if (DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var fallback))
            return fallback;

        throw new AppException($"'{input}' — tanilgan sana formatlariga mos emas.");
    }

    public static DateTimeOffset ParseFlexibleDateTimeOffset(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new AppException("Sana qiymati bo‘sh yoki null bo‘lishi mumkin emas.");

        foreach (var format in DateFormats)
            if (DateTimeOffset.TryParseExact(input, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed))
                return parsed;

        if (DateTimeOffset.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var fallback))
            return fallback;

        return new DateTimeOffset(ParseFlexibleDate(input));
    }
}
