namespace Forex.Infrastructure;

using Forex.Application.Commons.Interfaces;
using Forex.Infrastructure.FileStorage.Minio;
using Forex.Infrastructure.Identity;
using Forex.Infrastructure.Persistence;
using Forex.Infrastructure.Persistence.Interceptors;
using Forex.Infrastructure.Security;
using Forex.Infrastructure.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration conf)
    {
        services.AddHttpContextAccessor();
        services.AddDbContext(conf);
        services.AddFileStorage(conf);
        services.AddIdentity();
        services.AddScoped<IPagingMetadataWriter, HttpPagingMetadataWriter>();
    }

    private static void AddIdentity(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
    }

    private static void AddDbContext(this IServiceCollection services, IConfiguration conf)
    {
        services.AddScoped<AuditInterceptor>();

        var connectionString = conf.GetConnectionString("forex")
                               ?? conf.GetConnectionString("DefaultConnection");

        services.AddDbContext<IAppDbContext, AppDbContext>((sp, options) =>
            options.UseNpgsql(connectionString)
                   .AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));
    }

    public static void AddFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection("Minio").Get<MinioOptions>()!;

        var minioClient = new MinioClient()
            .WithEndpoint(options.Endpoint)
            .WithCredentials(options.AccessKey, options.SecretKey)
            .Build();

        services.AddSingleton(options);
        services.AddSingleton(minioClient);
        services.AddScoped<IFileStorageService, MinioFileStorageService>();
    }
}
