namespace Forex.Application.Features.SemiProducts.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities.Manufactories;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record SemiProductCommand(
    long ManufactoryId,
    string? Name,
    int Code,
    string Measure,
    Stream? Photo,
    string? Extension,
    string? ContentType
) : IRequest<long>;

public class CreateSemiProductCommandHandler(
    IAppDbContext context,
    IMapper mapper,
    IFileStorageService fileStorage
) : IRequestHandler<SemiProductCommand, long>
{
    public async Task<long> Handle(SemiProductCommand request, CancellationToken cancellationToken)
    {
        await EnsureCodeIsUnique(request.Code, cancellationToken);

        var semiProduct = mapper.Map<SemiProduct>(request);

        semiProduct.PhotoPath = await UploadPhotoIfExists(request, cancellationToken);

        await SaveSemiProduct(semiProduct, cancellationToken);

        return semiProduct.Id;
    }

    private async Task EnsureCodeIsUnique(int code, CancellationToken ct)
    {
        var isExist = await context.SemiProducts.AnyAsync(sp => sp.Code == code, ct);

        if (isExist)
            throw new AlreadyExistException(nameof(SemiProduct), nameof(code), code);
    }

    private async Task<string> UploadPhotoIfExists(SemiProductCommand request, CancellationToken ct)
    {
        if (request.Photo is null) return string.Empty;

        var fileName = $"{Guid.NewGuid():N}{request.Extension}";
        return await fileStorage.UploadAsync(
            request.Photo,
            fileName,
            request.ContentType ?? "application/octet-stream",
            ct);
    }

    private async Task SaveSemiProduct(SemiProduct semiProduct, CancellationToken ct)
    {
        context.SemiProducts.Add(semiProduct);
        await context.SaveAsync(ct);
    }
}
