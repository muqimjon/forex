namespace Forex.Application.Commons.Exceptions;

using System.Net;

[Serializable]
public class AlreadyExistException : AppException
{
    public AlreadyExistException(string? value) : base($"Bunday {value} avvaldan mavjud", HttpStatusCode.Conflict) { }

    public AlreadyExistException(string? model, string prop, object value) : base($"Bunday {model} avvaldan mavjud {prop}={value}", HttpStatusCode.Conflict) { }
}