namespace Forex.Application.Features.Products.ProductEntries.Queries;

using Forex.Application.Commons.Interfaces;
using MediatR;

public record GetPresignedUrlQuery(string FileName, string Extension) : IRequest<PresignedUrlResponse>;

public record PresignedUrlResponse(string Url, string Key);

public class GetPresignedUrlHandler(IFileStorageService fileStorage) : IRequestHandler<GetPresignedUrlQuery, PresignedUrlResponse>
{
    public async Task<PresignedUrlResponse> Handle(GetPresignedUrlQuery request, CancellationToken cancellationToken)
    {
        var key = $"{Guid.NewGuid()}{request.Extension}";
        var url = await fileStorage.GetPresignedUrlAsync(key);

        return new PresignedUrlResponse(url, key);
    }
}
