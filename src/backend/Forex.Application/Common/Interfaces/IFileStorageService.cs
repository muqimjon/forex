namespace Forex.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<PresignedUploadResult> GeneratePresignedUploadUrlAsync(
        string fileName,
        string contentType,
        string? folder = null,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default);

    Task<bool> FileExistsAsync(
        string objectKey,
        CancellationToken cancellationToken = default);

    Task<string?> MoveFileAsync(
        string sourceKey,
        string destinationFolder,
        CancellationToken cancellationToken = default);

    Task CleanupExpiredFilesAsync(
        TimeSpan maxAge,
        string prefix,
        CancellationToken cancellationToken = default);

    Task DeleteFileAsync(
        string objectKey,
        CancellationToken cancellationToken = default);
}

public sealed record PresignedUploadResult
{
    public required string UploadUrl { get; init; }
    public required string ObjectKey { get; init; }
    public required DateTime ExpiresAt { get; init; }
}
