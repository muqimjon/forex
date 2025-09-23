namespace Forex.Application.Features.SemiProductEntries.Validators;

using FluentValidation;
using Forex.Application.Features.SemiProductEntries.Commands;

public class CreateSemiProductIntakeCommandValidator : AbstractValidator<CreateSemiProductIntakeCommand>
{
    public CreateSemiProductIntakeCommandValidator()
    {
        RuleFor(x => x.Supplier.Id).GreaterThan(0);
        RuleFor(x => x.ManufactoryId).GreaterThan(0);
        RuleFor(x => x.EntryDate).LessThanOrEqualTo(DateTime.UtcNow);
        RuleFor(x => x.Containers).NotEmpty().WithMessage("At least one container is required.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one semi-product line is required.");

        RuleForEach(x => x.Containers).ChildRules(cont =>
        {
            cont.RuleFor(c => c.Count).GreaterThan(0);
            cont.RuleFor(c => c.Price).GreaterThanOrEqualTo(0);
        });

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.CostPrice).GreaterThanOrEqualTo(0);
            item.RuleFor(i => i.CostDelivery).GreaterThanOrEqualTo(0);
            item.RuleFor(i => i.TransferFee).GreaterThanOrEqualTo(0);

            item.When(i => i.SemiProductId is null, () =>
            {
                item.RuleFor(i => i.Name).NotEmpty();
                item.RuleFor(i => i.Code).NotNull();
                item.RuleFor(i => i.Measure).NotEmpty();
            });
        });
    }
}

