namespace Forex.Infrastructure.Storage;

public sealed class MinioStorageOptions
{
    public string Endpoint { get; init; } = default!;
    public string AccessKey { get; init; } = default!;
    public string SecretKey { get; init; } = default!;
    public string BucketName { get; init; } = default!;
    public bool UseSsl { get; init; }
    public bool EnablePublicRead { get; init; }
    public string Prefix { get; init; } = "uploads";
}
