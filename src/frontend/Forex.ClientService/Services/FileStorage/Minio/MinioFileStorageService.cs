namespace Forex.ClientService.Services.FileStorage.Minio;

using Forex.ClientService.Services.Models;
using global::Minio;
using global::Minio.DataModel.Args;
using Microsoft.AspNetCore.Http;

public class MinioFileStorageService(IMinioClient client, MinioOptions options)
{
    private readonly string bucketName = options.BucketName;
    private readonly string publicPolicy = BucketPolicyBuilder.BuildPublicReadPolicy(options.BucketName);

    public async Task<string> UploadAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        using var stream = file.OpenReadStream();
        return await UploadAsync(stream, file.FileName, file.ContentType, cancellationToken);
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        await EnsurePublicBucketAsync(cancellationToken);

        await client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(fileName)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(contentType), cancellationToken);

        var fileUrl = $"http://{options.Endpoint}/{bucketName}/{fileName}";
        return fileUrl;
    }

    private async Task EnsurePublicBucketAsync(CancellationToken cancellationToken)
    {
        var exists = await client.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName), cancellationToken);
        if (!exists)
        {
            await client.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName), cancellationToken);

            await client.SetPolicyAsync(new SetPolicyArgs()
                .WithBucket(bucketName)
                .WithPolicy(publicPolicy), cancellationToken);
        }
    }

    public async Task<Stream> DownloadAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var ms = new MemoryStream();

        await client.GetObjectAsync(new GetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(fileName)
            .WithCallbackStream(stream => stream.CopyTo(ms)), cancellationToken);

        ms.Position = 0;
        return ms;
    }

    public async Task DeleteAsync(string fileName, CancellationToken cancellationToken = default)
    {
        await client.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(bucketName)
            .WithObject(fileName), cancellationToken);
    }
}
