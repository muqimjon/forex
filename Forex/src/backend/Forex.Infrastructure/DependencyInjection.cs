namespace Forex.Infrastructure;

using Forex.Application.Commons.Interfaces;
using Forex.Infrastructure.FileStorage.Minio;
using Forex.Infrastructure.Persistence;
using Forex.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration conf)
    {
        services.AddDbContext(conf);
        services.AddFileStorage(conf);

        return services;
    }

    private static void AddDbContext(this IServiceCollection services, IConfiguration conf)
    {
        services.AddScoped<AuditInterceptor>();
        services.AddDbContext<IAppDbContext, AppDbContext>((sp, options) =>
            options.UseNpgsql(conf.GetConnectionString("DefaultConnection"))
                   .AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));
    }

    public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection("Minio").Get<MinioOptions>()!;

        var minioClient = new MinioClient()
            .WithEndpoint(options.Endpoint)
            .WithCredentials(options.AccessKey, options.SecretKey)
            .Build();

        services.AddSingleton(options);
        services.AddSingleton(minioClient);
        services.AddScoped<IFileStorageService, MinioFileStorageService>();

        return services;
    }
}
