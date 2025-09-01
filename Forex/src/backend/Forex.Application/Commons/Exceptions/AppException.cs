namespace Forex.Application.Commons.Exceptions;

using System.Net;

public class AppException(string? message, HttpStatusCode statusCode) : Exception(message)
{
    public HttpStatusCode StatusCode { get; set; } = statusCode;
}