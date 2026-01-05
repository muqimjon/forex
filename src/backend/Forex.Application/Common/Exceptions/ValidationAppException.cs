namespace Forex.Application.Common.Exceptions;

using FluentValidation.Results;
using System.Net;

public class ValidationAppException(IEnumerable<ValidationFailure> failures) :
    AppException(BuildErrorMessage(failures), HttpStatusCode.BadRequest)
{
    private static string BuildErrorMessage(IEnumerable<ValidationFailure> failures)
        => string.Join("; ", failures
            .GroupBy(f => f.PropertyName)
            .Select(g => $"{g.Key}: {string.Join(", ", g.Select(f => f.ErrorMessage).Distinct())}"));
}
