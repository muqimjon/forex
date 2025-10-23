namespace Forex.Application.Features.Users.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Accounts.Commands;
using Forex.Domain.Entities;
using Forex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record UpdateUserCommand(
    long Id,
    string Name,
    string? Phone,
    string? Email,
    UserRole Role,
    string? Address,
    string? Description,
    string? Password,
    List<UpdateUserAccountCommand> Accounts)
    : IRequest<bool>;

public class UpdateUserCommandHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<UpdateUserCommand, bool>
{
    public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), nameof(request.Id), request.Id);

        mapper.Map(request, user);

        return await context.SaveAsync(cancellationToken);
    }
}
