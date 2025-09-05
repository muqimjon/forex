namespace Forex.Application.Commons.Exceptions;

using System.Net;

internal class ForbiddenException(string? message) : AppException(message, HttpStatusCode.Forbidden);