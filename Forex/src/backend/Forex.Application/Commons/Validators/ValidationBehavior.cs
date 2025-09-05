namespace Forex.Application.Commons.Validators;

using FluentValidation;
using Forex.Application.Commons.Exceptions;
using MediatR;

public class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        var context = new ValidationContext<TRequest>(request);
        var failures = validators
            .SelectMany(v => v.Validate(context).Errors)
            .Where(f => f is not null);

        if (failures.Any())
            throw new ValidationAppException(failures);

        return await next();
    }
}
