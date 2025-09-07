namespace Forex.Application.Features.Products.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities.Shops;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record UpdateProductCommand(
    long Id,
    string Name,
    int Code,
    string Measure,
    Stream? Photo,
    string? Extension,
    string? ContentType)
    : IRequest<bool>;

public class UpdateProductCommandHandler(
    IAppDbContext context,
    IMapper mapper,
    IFileStorageService fileStorage)
    : IRequestHandler<UpdateProductCommand, bool>
{
    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await GetProduct(request.Id, cancellationToken);

        if (request.Photo is not null)
            product.PhotoPath = await UploadNewPhoto(product, request, cancellationToken);

        UpdateProductFields(product, request);

        return await context.SaveAsync(cancellationToken);
    }

    private async Task<Product> GetProduct(long id, CancellationToken ct)
        => await context.Products
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException(nameof(Product), nameof(id), id);

    private async Task<string> UploadNewPhoto(Product product, UpdateProductCommand request, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(product.PhotoPath))
            await fileStorage.DeleteAsync(product.PhotoPath, ct);

        var fileName = $"{Guid.NewGuid():N}{request.Extension}";
        return await fileStorage.UploadAsync(request.Photo!, fileName, request.ContentType ?? "application/octet-stream", ct);
    }

    private void UpdateProductFields(Product product, UpdateProductCommand request)
    {
        mapper.Map(request, product);
    }
}
