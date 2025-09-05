namespace Forex.Application;

using FluentValidation;
using Forex.Application.Commons.Validators;
using Forex.Application.Features.Users.Validators;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(assembly);
        services.AddAutoMapper(assembly);

        services.AddValidatorsFromAssemblyContaining<CreateUserCommandValidator>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));


        return services;
    }
}
