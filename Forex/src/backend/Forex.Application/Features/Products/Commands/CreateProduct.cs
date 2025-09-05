namespace Forex.Application.Features.Products.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record CreateProductCommand(
    string Name,
    int Code,
    string Measure,
    string PhotoPath)
    : IRequest<long>;

public class CreateProductCommandHandler(
    IAppDbContext context,
    IMapper mapper) : IRequestHandler<CreateProductCommand, long>
{
    public async Task<long> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var isExist = await context.Products
            .AnyAsync(user => user.Code == request.Code, cancellationToken);

        if (isExist)
            throw new AlreadyExistException(nameof(Product), nameof(request.Code), request.Code);

        var product = mapper.Map<Product>(request);
        context.Products.Add(product);
        await context.SaveAsync(cancellationToken);
        return product.Id;
    }
}
