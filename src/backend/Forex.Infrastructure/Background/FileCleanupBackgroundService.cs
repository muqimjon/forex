namespace Forex.Infrastructure.Background;

using Forex.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public sealed class FileCleanupBackgroundService(
    IServiceProvider serviceProvider,
    ILogger<FileCleanupBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(24);
    private static readonly TimeSpan FileMaxAge = TimeSpan.FromDays(7);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(CleanupInterval, stoppingToken);

                using var scope = serviceProvider.CreateScope();
                var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();

                await fileStorage.CleanupExpiredFilesAsync(FileMaxAge, "temp", stoppingToken);

                logger.LogInformation("File cleanup completed successfully");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error occurred during file cleanup");
            }
        }
    }
}
