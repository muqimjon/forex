namespace Forex.Wpf;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Windows;

public partial class App : Application
{
    public static IHost? AppHost { get; private set; }

    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                var env = context.HostingEnvironment;

                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", false, true);
                config.AddJsonFile($"appsettings.Development.json", true, true);
                config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddApplicationServices(context.Configuration);
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        // 1. Hostni ishga tushiramiz
        await AppHost!.StartAsync();

        // 2. DI konteynerdan MainWindow ni olamiz
        // Eslatma: MainWindow xizmatlar (Services) ichida ro'yxatdan o'tgan bo'lishi kerak
        var mainWindow = AppHost.Services.GetRequiredService<Windows.MainWindow>();

        // 3. Oynani ko'rsatamiz
        mainWindow.Show();

        base.OnStartup(e);
    }
    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost!.StopAsync();
        AppHost.Dispose();
        base.OnExit(e);
    }
}
