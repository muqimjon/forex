namespace Forex.Application.Features.Users.Validators;

using FluentValidation;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Users.Commands;
using Microsoft.EntityFrameworkCore;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(IAppDbContext context)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .MustAsync(async (phone, cancellation) =>
            {
                var exists = await context.Users
                    .AnyAsync(u => u.Phone == phone, cancellation);
                return !exists;
            }).WithMessage("User with this phone already exists");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required");
    }
}

