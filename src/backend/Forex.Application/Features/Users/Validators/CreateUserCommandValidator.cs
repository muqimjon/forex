namespace Forex.Application.Features.Users.Validators;

using FluentValidation;
using Forex.Application.Features.Users.Commands;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Foydalanuvchi nomi kiritilishi shart");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Noto'g'ri rol tanlandi");

        RuleFor(x => x.AccessMask)
            .GreaterThanOrEqualTo(0);
    }
}


