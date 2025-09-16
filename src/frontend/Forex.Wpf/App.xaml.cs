namespace Forex.Wpf;

using AutoMapper;
using Forex.ClientService;
using Forex.Wpf.Pages.SemiProducts.Mapping;
using Forex.Wpf.Windows;
using Microsoft.Extensions.Logging;
using System.Windows;

public partial class App : Application
{
    public static ForexClient Client { get; private set; } = default!;
    public static IMapper Mapper { get; private set; } = default!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Client = new ForexClient("https://localhost:7041/api");

        #region Mapper Configuration..

        ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Warning);
        });

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<SemiProductMappingProfile>();
        }, loggerFactory);

        #endregion

        Mapper = mapperConfig.CreateMapper();

        var mainWindow = new MainWindow();
        mainWindow.Show();
    }
}
