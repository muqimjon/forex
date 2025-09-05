namespace Forex.Application.Commons.Exceptions;

using FluentValidation.Results;
using System.Net;

public class ValidationAppException(IEnumerable<ValidationFailure> failures) :
    AppException(BuildErrorMessage(failures), HttpStatusCode.BadRequest)
{
    private static string BuildErrorMessage(IEnumerable<ValidationFailure> failures)
        => string.Join("; ", failures.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
}
