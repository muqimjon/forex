namespace Forex.Application.Features.SemiProducts.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record CreateSemiProductCommand(
    long ManufactoryId,
    string? Name,
    int Code,
    string Measure,
    string? PhotoPath)
    : IRequest<long>;

public class CreateSemiProductCommandHandler(
    IAppDbContext context,
    IMapper mapper) : IRequestHandler<CreateSemiProductCommand, long>
{
    public async Task<long> Handle(CreateSemiProductCommand request, CancellationToken cancellationToken)
    {
        var isExist = await context.SemiProducts
            .AnyAsync(user => user.Code == request.Code, cancellationToken);

        if (isExist)
            throw new AlreadyExistException(nameof(SemiProduct), nameof(request.Code), request.Code);

        var semiProduct = mapper.Map<SemiProduct>(request);
        context.SemiProducts.Add(semiProduct);
        await context.SaveAsync(cancellationToken);
        return semiProduct.Id;
    }
}
