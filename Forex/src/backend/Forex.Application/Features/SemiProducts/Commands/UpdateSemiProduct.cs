namespace Forex.Application.Features.SemiProducts.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record UpdateSemiProductCommand(
    long Id,
    long ManufactoryId,
    string? Name,
    int Code,
    string Measure,
    Stream? Photo,
    string? Extension,
    string? ContentType
) : IRequest<bool>;

public class UpdateSemiProductCommandHandler(
    IAppDbContext context,
    IMapper mapper,
    IFileStorageService fileStorage
) : IRequestHandler<UpdateSemiProductCommand, bool>
{
    public async Task<bool> Handle(UpdateSemiProductCommand request, CancellationToken cancellationToken)
    {
        var semiProduct = await GetSemiProduct(request.Id, cancellationToken);

        if (request.Photo is not null)
            semiProduct.PhotoPath = await UploadNewPhoto(semiProduct, request, cancellationToken);

        UpdateSemiProductFields(semiProduct, request);

        return await context.SaveAsync(cancellationToken);
    }

    private async Task<SemiProduct> GetSemiProduct(long id, CancellationToken ct)
        => await context.SemiProducts
            .FirstOrDefaultAsync(sp => sp.Id == id, ct)
            ?? throw new NotFoundException(nameof(SemiProduct), nameof(id), id);

    private async Task<string> UploadNewPhoto(SemiProduct semiProduct, UpdateSemiProductCommand request, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(semiProduct.PhotoPath))
            await fileStorage.DeleteAsync(semiProduct.PhotoPath, ct);

        var fileName = $"{Guid.NewGuid():N}{request.Extension}";
        return await fileStorage.UploadAsync(request.Photo!, fileName, request.ContentType ?? "application/octet-stream", ct);
    }

    private void UpdateSemiProductFields(SemiProduct semiProduct, UpdateSemiProductCommand request)
    {
        mapper.Map(request, semiProduct);
    }
}
