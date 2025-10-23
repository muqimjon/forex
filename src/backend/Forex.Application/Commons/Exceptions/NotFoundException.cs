namespace Forex.Application.Commons.Exceptions;

using System.Net;

public class NotFoundException : AppException
{
    public NotFoundException(string? value) : base($"Bunday {value} topilmadi", HttpStatusCode.NotFound) { }

    public NotFoundException(string? model, string prop, object value) : base($"Bunday {prop} li {model} topilmadi. {prop}={value}", HttpStatusCode.NotFound) { }
}