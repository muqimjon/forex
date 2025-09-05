namespace Forex.Application.Features.Shops.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record UpdateShopCommand(
    long Id,
    string Name)
    : IRequest<bool>;

public class UpdateShopCommandHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<UpdateShopCommand, bool>
{
    public async Task<bool> Handle(UpdateShopCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Shops
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Shop), nameof(request.Id), request.Id);

        mapper.Map(request, product);
        return await context.SaveAsync(cancellationToken);
    }
}
