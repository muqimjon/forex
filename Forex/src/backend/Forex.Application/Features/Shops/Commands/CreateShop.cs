namespace Forex.Application.Features.Shops.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record CreateShopCommand(
    string Name)
    : IRequest<long>;

public class CreateShopCommandHandler(
    IAppDbContext context,
    IMapper mapper) : IRequestHandler<CreateShopCommand, long>
{
    public async Task<long> Handle(CreateShopCommand request, CancellationToken cancellationToken)
    {
        var isExist = await context.Shops
            .AnyAsync(user => user.Name == request.Name, cancellationToken);

        if (isExist)
            throw new AlreadyExistException(nameof(Shop), nameof(request.Name), request.Name);

        var product = mapper.Map<Shop>(request);
        context.Shops.Add(product);
        await context.SaveAsync(cancellationToken);
        return product.Id;
    }
}
