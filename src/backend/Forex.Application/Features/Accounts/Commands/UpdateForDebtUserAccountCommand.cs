namespace Forex.Application.Features.Accounts.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Accounts.DTOs;
using Forex.Application.Features.Currencies.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record UpdateForDebtUserAccountCommand(
    long Id,
  //  decimal OpeningBalance,
    //decimal Discount,
   // decimal Balance,
    string? Description,
    DateTime? DueDate
   // long UserId,
    //long CurrencyId
    )
    : IRequest<bool>;
public class UpdateForDebtUserAccountCommandHandler(
    IAppDbContext context)
    : IRequestHandler<UpdateForDebtUserAccountCommand, bool>
{
    public async Task<bool> Handle(UpdateForDebtUserAccountCommand request, CancellationToken cancellationToken)
    {
        var userAccountEntity = await context.UserAccounts
        .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
        ?? throw new NotFoundException(nameof(UpdateForDebtUserAccountCommand), "Id", request.Id);

        userAccountEntity.DueDate = DateTime.SpecifyKind(request.DueDate!.Value, DateTimeKind.Utc);
        userAccountEntity.Description = request.Description;
        // Save qilish
        return await context.SaveAsync(cancellationToken);
    }

}
