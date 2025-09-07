namespace Forex.Infrastructure.FileStorage.Minio;

using System.Text.Json;

public static class BucketPolicyBuilder
{
    public static string BuildPublicReadPolicy(string bucketName)
    {
        var policy = new
        {
            Version = "2012-10-17",
            Statement = new[]
            {
                new
                {
                    Effect = "Allow",
                    Principal = new { AWS = new[] { "*" } },
                    Action = new[] { "s3:GetObject" },
                    Resource = new[] { $"arn:aws:s3:::{bucketName}/*" }
                }
            }
        };

        return JsonSerializer.Serialize(policy);
    }
}

