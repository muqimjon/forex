namespace Forex.Application.Features.Cashes.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record UpdateCashCommand(
    long Id,
    int ShopId,
    int CurrencyId,
    decimal Balance)
    : IRequest<bool>;
public class UpdateCashCommandHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<UpdateCashCommand, bool>
{
    public async Task<bool> Handle(UpdateCashCommand request, CancellationToken cancellationToken)
    {
        var cash = await context.Cashes
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Cash), nameof(request.Id), request.Id);

        mapper.Map(request, cash);
        return await context.SaveAsync(cancellationToken);
    }
}
