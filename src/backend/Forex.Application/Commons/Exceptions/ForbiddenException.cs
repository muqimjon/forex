namespace Forex.Application.Commons.Exceptions;

using System.Net;

public class ForbiddenException(string? message) : AppException(message, HttpStatusCode.Forbidden);