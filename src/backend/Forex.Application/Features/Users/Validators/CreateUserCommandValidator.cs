namespace Forex.Application.Features.Users.Validators;

using FluentValidation;
using Forex.Application.Features.Users.Commands;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Foydalanuvchi nomi kiritilishi shart");

        //RuleFor(x => x.Phone)
        //    .NotEmpty().WithMessage("Telefon raqami kiritilishi shart")
        //    .Matches(@"^\+998\d{9}$")
        //    .WithMessage("Faqat O‘zbekiston telefon raqami formatida bo‘lishi kerak (masalan: +998901234567)");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Noto‘g‘ri rol tanlandi");

        //RuleFor(x => x.Address)
        //    .NotEmpty().WithMessage("Manzil kiritilishi shart");
    }
}


