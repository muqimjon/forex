namespace Forex.Application.Features.Sales.Validators;

using FluentValidation;
using Forex.Application.Features.Sales.Commands;

public class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0);
        RuleFor(x => x.TotalAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SaleItems).NotEmpty();

        RuleForEach(x => x.SaleItems).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductTypeId).GreaterThan(0);
            item.RuleFor(i => i.BundleCount).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
            item.RuleFor(i => i.Amount).GreaterThanOrEqualTo(0);
        });
    }
}
