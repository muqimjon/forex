namespace Forex.ClientService;

using Forex.ClientService.Configuration;
using Forex.ClientService.Interfaces;
using Forex.ClientService.Services;
using Forex.ClientService.Services.FileStorage.Minio;
using Forex.ClientService.Services.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using Refit;

public static class DependencyInjection
{
    public static IServiceCollection AddClientServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<AuthStore>();

        var minioOptions = config.GetSection("Minio").Get<MinioOptions>()!;
        services.AddSingleton(minioOptions);

        var minioClient = new MinioClient()
            .WithEndpoint(minioOptions.Endpoint)
            .WithCredentials(minioOptions.AccessKey, minioOptions.SecretKey)
            .Build();

        services.AddSingleton(minioClient);
        services.AddSingleton<MinioFileStorageService>();

        services.AddTransient<AuthHeaderHandler>();

        string apiBaseUrl = config.GetValue<string>("ApiBaseUrl")!;
        services.AddAllRefitClients(apiBaseUrl);

        services.AddSingleton<ForexClient>();

        return services;
    }

    private static IServiceCollection AddAllRefitClients(this IServiceCollection services, string baseUrl)
    {
        var assembly = typeof(IApiAuth).Assembly;
        var refitInterfaces = assembly.GetTypes()
            .Where(t => t.IsInterface && t.Name.StartsWith("IApi"))
            .ToList();

        foreach (var apiInterface in refitInterfaces)
        {
            services.AddRefitClient(apiInterface)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl))
                .AddHttpMessageHandler<AuthHeaderHandler>();
        }

        return services;
    }
}
