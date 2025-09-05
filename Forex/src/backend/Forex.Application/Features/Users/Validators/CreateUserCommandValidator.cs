namespace Forex.Application.Features.Users.Validators;

using FluentValidation;
using Forex.Application.Features.Users.Commands;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .Matches(@"^\+?[0-9]{9,15}$")
            .WithMessage("Invalid phone number format");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required");
    }
}

