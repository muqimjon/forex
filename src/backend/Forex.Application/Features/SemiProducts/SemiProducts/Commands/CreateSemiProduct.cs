namespace Forex.Application.Features.SemiProducts.SemiProducts.Commands;

using AutoMapper;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities.SemiProducts;
using MediatR;

public record CreateSemiProductCommand(
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
) : IRequestHandler<CreateSemiProductCommand, long>
{
    public async Task<long> Handle(CreateSemiProductCommand request, CancellationToken cancellationToken)
    {
        var semiProduct = mapper.Map<SemiProduct>(request);

        semiProduct.ImagePath = await UploadPhotoIfExists(request, cancellationToken);

        await SaveSemiProduct(semiProduct, cancellationToken);

        return semiProduct.Id;
    }

    private async Task<string> UploadPhotoIfExists(CreateSemiProductCommand request, CancellationToken ct)
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
