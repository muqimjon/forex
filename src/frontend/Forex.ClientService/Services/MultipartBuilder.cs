namespace Forex.ClientService.Services;

using Microsoft.AspNetCore.Http;
using System.Collections;
using System.Net.Http.Headers;

public static class MultipartFormDataBuilder
{
    public static MultipartFormDataContent Build(object obj)
    {
        var form = new MultipartFormDataContent();
        AddObject(form, obj, string.Empty);
        return form;
    }

    private static void AddObject(MultipartFormDataContent form, object? obj, string prefix)
    {
        if (obj == null) return;

        var type = obj.GetType();

        // Agar oddiy tip bo‘lsa
        if (IsSimple(type))
        {
            form.Add(new StringContent(obj.ToString() ?? ""), prefix);
            return;
        }

        // Agar fayl bo‘lsa
        if (obj is IFormFile file)
        {
            var streamContent = new StreamContent(file.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            form.Add(streamContent, prefix, file.FileName);
            return;
        }

        // Agar ro‘yxat (list, array) bo‘lsa
        if (obj is IEnumerable enumerable && obj is not string)
        {
            int index = 0;
            foreach (var item in enumerable)
            {
                var newPrefix = $"{prefix}[{index}]";
                AddObject(form, item, newPrefix);
                index++;
            }
            return;
        }

        // Agar complex obyekt bo‘lsa
        foreach (var prop in type.GetProperties())
        {
            var value = prop.GetValue(obj);
            var newPrefix = string.IsNullOrEmpty(prefix)
                ? prop.Name
                : $"{prefix}.{prop.Name}";

            AddObject(form, value, newPrefix);
        }
    }

    private static bool IsSimple(Type type)
    {
        return type.IsPrimitive
               || type.IsEnum
               || type == typeof(string)
               || type == typeof(decimal)
               || type == typeof(DateTime)
               || type == typeof(Guid);
    }
}
