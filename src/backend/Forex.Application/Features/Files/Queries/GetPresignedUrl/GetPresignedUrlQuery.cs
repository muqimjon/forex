namespace Forex.Application.Features.Files.Queries.GetPresignedUrl;

using Forex.Application.Common.Interfaces;
using MediatR;

public record GetPresignedUrlQuery(string FileName, string Folder) : IRequest<PresignedUrlResponse>;

public record PresignedUrlResponse(string Url, string Key, DateTime ExpiresAt);

public class GetPresignedUrlHandler(IFileStorageService fileStorage) : IRequestHandler<GetPresignedUrlQuery, PresignedUrlResponse>
{
    public async Task<PresignedUrlResponse> Handle(GetPresignedUrlQuery request, CancellationToken cancellationToken)
    {
        var contentType = GetContentType(request.FileName);

        var result = await fileStorage.GeneratePresignedUploadUrlAsync(
            request.FileName,
            contentType,
            $"temp/{request.Folder}",
            cancellationToken: cancellationToken);

        return new PresignedUrlResponse(result.UploadUrl, result.ObjectKey, result.ExpiresAt);
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}
