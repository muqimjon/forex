namespace Forex.Infrastructure.Storage;

using Forex.Application.Common.Interfaces;
using Minio;
using Minio.DataModel.Args;

public sealed class MinioFileStorageService : IFileStorageService
{
    private const int DefaultExpirySeconds = 3600;
    
    private readonly IMinioClient _client;
    private readonly MinioStorageOptions _options;

    public MinioFileStorageService(IMinioClient client, MinioStorageOptions options)
    {
        _client = client;
        _options = options;
    }

    public async Task<PresignedUploadResult> GeneratePresignedUploadUrlAsync(
        string fileName,
        string contentType,
        string? folder = null,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureBucketExistsAsync(cancellationToken);

        var objectKey = GenerateObjectKey(fileName, folder);
        var expirySeconds = expiry?.TotalSeconds ?? DefaultExpirySeconds;

        var uploadUrl = await _client.PresignedPutObjectAsync(
            new PresignedPutObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(objectKey)
                .WithExpiry((int)expirySeconds));

        return new PresignedUploadResult
        {
            UploadUrl = uploadUrl,
            ObjectKey = objectKey,
            ExpiresAt = DateTime.UtcNow.AddSeconds(expirySeconds)
        };
    }

    public async Task<bool> FileExistsAsync(
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.StatObjectAsync(
                new StatObjectArgs()
                    .WithBucket(_options.BucketName)
                    .WithObject(objectKey),
                cancellationToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task DeleteFileAsync(
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        await _client.RemoveObjectAsync(
            new RemoveObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(objectKey),
            cancellationToken);
    }

    public async Task<string?> MoveFileAsync(
        string sourceKey,
        string destinationFolder,
        CancellationToken cancellationToken = default)
    {
        try
        {
             var destinationKey = sourceKey.Replace("/temp/", "/");
            
            if (sourceKey == destinationKey)
                 return sourceKey;

            await _client.CopyObjectAsync(
                new CopyObjectArgs()
                    .WithBucket(_options.BucketName)
                    .WithObject(destinationKey)
                    .WithCopyObjectSource(new CopySourceObjectArgs()
                        .WithBucket(_options.BucketName)
                        .WithObject(sourceKey)),
                cancellationToken);

            await DeleteFileAsync(sourceKey, cancellationToken);
            
            return destinationKey;
        }
        catch
        {
            return null;
        }
    }

    public async Task CleanupExpiredFilesAsync(
        TimeSpan maxAge,
        string prefix,
        CancellationToken cancellationToken = default)
    {
        var expiryDate = DateTime.UtcNow.Subtract(maxAge);

        var fullPrefix = $"{_options.Prefix}/{prefix}";

        await foreach (var item in _client.ListObjectsEnumAsync(
            new ListObjectsArgs()
                .WithBucket(_options.BucketName)
                .WithPrefix(fullPrefix)
                .WithRecursive(true),
            cancellationToken))
        {
            if (item.LastModified == null) continue;

            var lastModified = DateTime.Parse(item.LastModified);
            
            if (lastModified < expiryDate)
            {
                await DeleteFileAsync(item.Key, cancellationToken);
            }
        }
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        var exists = await _client.BucketExistsAsync(
            new BucketExistsArgs()
                .WithBucket(_options.BucketName),
            cancellationToken);

        if (!exists)
        {
            await _client.MakeBucketAsync(
                new MakeBucketArgs()
                    .WithBucket(_options.BucketName),
                cancellationToken);

            if (_options.EnablePublicRead)
            {
                await SetBucketPolicyAsync(cancellationToken);
            }
        }
    }

    private async Task SetBucketPolicyAsync(CancellationToken cancellationToken)
    {
        var policy = $$"""
        {
            "Version": "2012-10-17",
            "Statement": [
                {
                    "Effect": "Allow",
                    "Principal": {"AWS": "*"},
                    "Action": ["s3:GetObject"],
                    "Resource": ["arn:aws:s3:::{{_options.BucketName}}/*"]
                }
            ]
        }
        """;

        await _client.SetPolicyAsync(
            new SetPolicyArgs()
                .WithBucket(_options.BucketName)
                .WithPolicy(policy),
            cancellationToken);
    }

    private string GenerateObjectKey(string fileName, string? folder)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var uniqueId = Guid.NewGuid().ToString("N")[..12];
        var extension = Path.GetExtension(fileName);
        
        var prefix = _options.Prefix;
        if (!string.IsNullOrWhiteSpace(folder))
        {
            prefix = $"{prefix}/{folder}";
        }

        return $"{prefix}/{timestamp}/{uniqueId}{extension}";
    }
}
