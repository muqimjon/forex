namespace Forex.Wpf;

using Forex.ClientService;
using Forex.Wpf.Windows;
using System.Windows;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static ForexClient Client { get; private set; } = default!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var client = new ForexClient("https://localhost:7041/api");

        var mainWindow = new MainWindow();
        mainWindow.Initialize(client);
        mainWindow.Show();
    }
}

