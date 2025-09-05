namespace Forex.Application.Features.Users.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities.Users;
using Forex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record UpdateUserCommand(
    long Id,
    string Name,
    string Phone,
    Role Role,
    string Address,
    decimal Balance,
    string Description)
    : IRequest<bool>;

public class UpdateUserCommandHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<UpdateUserCommand, bool>
{
    public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Include(u => u.Account)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), nameof(request.Id), request.Id);

        mapper.Map(request, user);
        user.Account.BeginSumm = request.Balance;
        user.Account.Balance = request.Balance;

        return await context.SaveAsync(cancellationToken);
    }
}
