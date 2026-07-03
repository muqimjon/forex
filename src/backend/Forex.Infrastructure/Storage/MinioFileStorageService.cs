namespace Forex.Infrastructure.Storage;

using Forex.Application.Common.Interfaces;
using Forex.Application.Common.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using System.Text.RegularExpressions;

public sealed class MinioFileStorageService : IFileStorageService
{
    private const int DefaultExpirySeconds = 600;
    private readonly MinioStorageOptions _options;
    private readonly FileUploadOptions _uploadOptions;
    private readonly IMinioClient _internalClient;
    private readonly ForexMinioClientFactory _clientFactory;
    private readonly ILogger<MinioFileStorageService> _logger;
    private readonly Regex _tempKeyPattern;

    public MinioFileStorageService(
        IMinioClient internalClient,
        IOptions<MinioStorageOptions> options,
        IOptions<FileUploadOptions> uploadOptions,
        ForexMinioClientFactory clientFactory,
        ILogger<MinioFileStorageService> logger)
    {
        _internalClient = internalClient;
        _options = options.Value;
        _uploadOptions = uploadOptions.Value;
        _clientFactory = clientFactory;
        _logger = logger;

        // Faqat server generatsiya qilgan shakl: {prefix}/temp/<folder>/<yyyyMMdd>/<12hex><ext>
        _tempKeyPattern = new Regex(
            $@"^{Regex.Escape(_options.Prefix)}/temp/[A-Za-z0-9_-]+/\d{{8}}/[0-9a-f]{{12}}\.[A-Za-z0-9]+$",
            RegexOptions.Compiled);
    }

    public async Task<PresignedUploadResult> GeneratePresignedUploadUrlAsync(
        string fileName,
        string contentType,
        string? folder = null,
        TimeSpan? expiry = null,
        string? publicEndpointOverride = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureBucketExistsAsync(cancellationToken);

        var objectKey = GenerateObjectKey(fileName, folder);
        var expirySeconds = expiry?.TotalSeconds ?? DefaultExpirySeconds;

        var publicClient = _clientFactory.CreatePublicClient(publicEndpointOverride);

        var presignedPutArgs = new PresignedPutObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectKey)
            .WithExpiry((int)expirySeconds);

        var uploadUrl = await publicClient.PresignedPutObjectAsync(presignedPutArgs);

        return new PresignedUploadResult
        {
            UploadUrl = uploadUrl,
            ObjectKey = objectKey,
            ExpiresAt = DateTime.UtcNow.AddSeconds(expirySeconds),
            MaxFileSizeBytes = _uploadOptions.MaxFileSizeBytes
        };
    }

    public async Task<bool> FileExistsAsync(
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _internalClient.StatObjectAsync(
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
        await _internalClient.RemoveObjectAsync(
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
        if (!IsTempKey(sourceKey))
            return null;

        try
        {
            var destinationKey = sourceKey.Replace("/temp/", "/");

            if (sourceKey == destinationKey)
                return sourceKey;

            await _internalClient.CopyObjectAsync(
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

        await foreach (var item in _internalClient.ListObjectsEnumAsync(
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
        var exists = await _internalClient.BucketExistsAsync(
            new BucketExistsArgs()
                .WithBucket(_options.BucketName),
            cancellationToken);

        if (!exists)
        {
            await _internalClient.MakeBucketAsync(
                new MakeBucketArgs()
                    .WithBucket(_options.BucketName),
                cancellationToken);
        }

        if (_options.EnablePublicRead)
        {
            await SetBucketPolicyAsync(cancellationToken);
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
                    "Resource": ["arn:aws:s3:::{{_options.BucketName}}/{{_options.Prefix}}/*"]
                }
            ]
        }
        """;

        await _internalClient.SetPolicyAsync(
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

    public bool IsTempKey(string? objectKey)
        => !string.IsNullOrWhiteSpace(objectKey) && _tempKeyPattern.IsMatch(objectKey);

    public string GetFullUrl(string? objectKey)
    {
        if (string.IsNullOrWhiteSpace(objectKey)) return string.Empty;

        if (objectKey.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return objectKey;

        var endpoint = !string.IsNullOrWhiteSpace(_options.PublicEndpoint)
            ? _options.PublicEndpoint
            : _clientFactory.DeterminePublicEndpoint(null);

        if (!endpoint.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            string protocol = _options.UseSsl ? "https://" : "http://";
            endpoint = $"{protocol}{endpoint}";
        }

        return $"{endpoint.TrimEnd('/')}/{_options.BucketName}/{objectKey.TrimStart('/')}";
    }
}
