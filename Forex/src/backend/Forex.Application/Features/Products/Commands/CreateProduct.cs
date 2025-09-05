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
    Stream? Photo,
    string? ContentType,
    string? Extension
) : IRequest<long>;


public class CreateProductCommandHandler(
    IAppDbContext context,
    IMapper mapper,
    IFileStorageService fileStorage)
    : IRequestHandler<CreateProductCommand, long>
{
    public async Task<long> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        await EnsureCodeIsUnique(request.Code, cancellationToken);

        var product = mapper.Map<Product>(request);

        product.PhotoPath = await UploadPhotoIfExists(request, cancellationToken);

        await SaveProduct(product, cancellationToken);

        return product.Id;
    }

    private async Task EnsureCodeIsUnique(int code, CancellationToken ct)
    {
        var isExist = await context.Products.AnyAsync(p => p.Code == code, ct);

        if (isExist)
            throw new AlreadyExistException(nameof(Product), nameof(code), code);
    }

    private async Task<string> UploadPhotoIfExists(CreateProductCommand request, CancellationToken ct)
    {
        if (request.Photo is null) return string.Empty;

        var fileName = $"{Guid.NewGuid():N}{request.Extension}";
        return await fileStorage.UploadAsync(
            request.Photo,
            fileName,
            request.ContentType ?? "application/octet-stream",
            ct);
    }

    private async Task SaveProduct(Product product, CancellationToken ct)
    {
        context.Products.Add(product);
        await context.SaveAsync(ct);
    }
}

