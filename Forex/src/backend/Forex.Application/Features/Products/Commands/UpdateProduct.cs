namespace Forex.Application.Features.Products.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record UpdateProductCommand(
    long Id,
    string Name,
    int Code,
    string Measure,
    string PhotoPath)
    : IRequest<bool>;

public class UpdateProductCommandHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<UpdateProductCommand, bool>
{
    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Products
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), nameof(request.Id), request.Id);

        mapper.Map(request, product);
        return await context.SaveAsync(cancellationToken);
    }
}
