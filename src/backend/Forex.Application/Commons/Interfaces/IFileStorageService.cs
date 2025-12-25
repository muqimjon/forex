namespace Forex.Application.Commons.Interfaces;

using Microsoft.AspNetCore.Http;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<string> UploadAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string fileName, CancellationToken cancellationToken = default);
    Task DeleteAsync(string fileName, CancellationToken cancellationToken = default);
    Task<string> GetPresignedUrlAsync(string fileName, int expiryInSeconds = 3600);
    Task<bool> FileExistsAsync(string fileName, CancellationToken cancellationToken = default);
}

