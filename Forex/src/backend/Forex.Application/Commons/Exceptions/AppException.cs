namespace Forex.Application.Commons.Exceptions;

using System.Net;

[Serializable]
public class AppException(string? message, HttpStatusCode statusCode) : Exception(message)
{
    public HttpStatusCode StatusCode { get; set; } = statusCode;
}