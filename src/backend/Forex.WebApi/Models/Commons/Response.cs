namespace Forex.WebApi.Models.Commons;

using System.Net;

public class Response
{
    public int StatusCode { get; set; } = (int)HttpStatusCode.OK;
    public string Message { get; set; } = HttpStatusCode.OK.ToString();
    public object? Data { get; set; }
}
