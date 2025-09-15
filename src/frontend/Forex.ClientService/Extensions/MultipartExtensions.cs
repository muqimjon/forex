namespace Forex.ClientService.Extensions;

using System.Net.Http;
using System.Net.Http.Headers;

public static class MultipartExtensions
{
    public static void AddFormField(this MultipartFormDataContent content, string name, object? value)
    {
        var stringValue = value?.ToString() ?? string.Empty;
        content.Add(new StringContent(stringValue), name);
    }

    public static void AddFileField(this MultipartFormDataContent content, string name, string filePath, string? contentType = null, string? fileName = null)
    {
        if (!File.Exists(filePath))
            return;

        var stream = File.OpenRead(filePath);
        var streamContent = new StreamContent(stream)
        {
            Headers = { ContentType = new MediaTypeHeaderValue(contentType ?? "application/octet-stream") }
        };

        content.Add(streamContent, name, fileName ?? Path.GetFileName(filePath));
    }

    public static void AddIndexedFields<T>(this MultipartFormDataContent content, List<T> list, Action<MultipartFormDataContent, T, int> addItemFields)
    {
        for (int i = 0; i < list.Count; i++)
        {
            addItemFields(content, list[i], i);
        }
    }
}
