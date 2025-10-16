namespace Forex.Wpf;

using AutoMapper;
using Forex.ClientService;
using Forex.ClientService.Services.FileStorage.Minio;
using Forex.Wpf.Common;
using Forex.Wpf.Windows;
using Minio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Windows;
using Forex.ClientService.Services.Models;

public partial class App : Application
{
    public static ForexClient Client { get; private set; } = default!;
    public static IMapper Mapper { get; private set; } = default!;
    public static MinioFileStorageService FileStorage { get; private set; } = default!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // --- appsettings.json ---
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // --- API client ---
        Client = new ForexClient(config.GetValue<string>("ApiBaseUrl")!);

        // --- Mapper ---
        ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Warning));
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), loggerFactory);
        Mapper = mapperConfig.CreateMapper();

        // --- Minio sozlamasi ---
        var minioOptions = config.GetSection("Minio").Get<MinioOptions>()!;
        var minioClient = new MinioClient()
            .WithEndpoint(minioOptions.Endpoint)
            .WithCredentials(minioOptions.AccessKey, minioOptions.SecretKey)
            .Build();

        FileStorage = new MinioFileStorageService(minioClient, minioOptions);

        // --- UI ---
        var mainWindow = new MainWindow();
        mainWindow.Show();
    }
}
