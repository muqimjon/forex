namespace Forex.ClientService.Models.Commons;

using System.Text.Json.Serialization;

public class Response<T>
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }
}