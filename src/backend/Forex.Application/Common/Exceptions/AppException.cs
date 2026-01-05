namespace Forex.Application.Common.Exceptions;

using System.Net;

[Serializable]
public class AppException(string? message,
    HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    : Exception(message)
{
    public HttpStatusCode StatusCode { get; set; } = statusCode;
}