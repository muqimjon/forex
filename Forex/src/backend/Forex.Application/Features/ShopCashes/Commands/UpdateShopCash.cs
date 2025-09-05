namespace Forex.Application.Features.ShopCashes.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record UpdateShopCashCommand(
    long Id,
    int ShopId,
    int CurrencyId,
    decimal Balance)
    : IRequest<bool>;
public class UpdateShopCashCommandHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<UpdateShopCashCommand, bool>
{
    public async Task<bool> Handle(UpdateShopCashCommand request, CancellationToken cancellationToken)
    {
        var cash = await context.ShopCashes
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(ShopCash), nameof(request.Id), request.Id);

        mapper.Map(request, cash);
        return await context.SaveAsync(cancellationToken);
    }
}
