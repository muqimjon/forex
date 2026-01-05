namespace Forex.Application.Common.Exceptions;

using System.Net;

public class ConflictException(string? message) : AppException(message, HttpStatusCode.Conflict);