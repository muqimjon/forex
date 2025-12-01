// Forex.Wpf/DependencyInjection.cs
using Forex.ClientService;
using Forex.Wpf.Common;
using Forex.Wpf.Common.Interfaces;
using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Reports.ViewModels;
using Forex.Wpf.Pages.Sales.ViewModels;
using Forex.Wpf.Windows;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using NavigationService = Forex.Wpf.Common.Services.NavigationService;

namespace Forex.Wpf;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        AddClientLayer(services, config);
        AddUiLayer(services);           // ← faqat bitta chaqiruv
        AddCommonServices(services);
        return services;
    }

    private static void AddClientLayer(IServiceCollection services, IConfiguration config)
    {
        services.AddClientServices(config);
    }

    // TO‘G‘RI VA YAGONA AddUiLayer
    private static void AddUiLayer(IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // 1. Windows → Singleton
        foreach (var window in assembly.GetTypes()
                     .Where(t => t.IsClass && !t.IsAbstract && typeof(Window).IsAssignableFrom(t)))
        {
            services.AddSingleton(window);
        }

        // 2. Pages → Transient
        foreach (var page in assembly.GetTypes()
                     .Where(t => t.IsClass && !t.IsAbstract && typeof(Page).IsAssignableFrom(t)))
        {
            services.AddTransient(page);
        }

        // 3. Barcha ViewModellar → Transient (avto-topish)
        foreach (var vm in assembly.GetTypes()
                     .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("ViewModel")))
        {
            services.AddTransient(vm);
        }

        // MUHIM: ReportsPageViewModel uchun maxsus factory (constructor injection)
        services.AddTransient(provider => new ReportsPageViewModel(
            provider.GetRequiredService<INavigationService>(),
            provider.GetRequiredService<SalesHistoryReportViewModel>(),
            provider.GetRequiredService<FinishedStockReportViewModel>(),
            provider.GetRequiredService<SemiFinishedStockReportViewModel>(),
            provider.GetRequiredService<DebtorCreditorReportViewModel>(),
            provider.GetRequiredService<EmployeeBalanceReportViewModel>(),
            provider.GetRequiredService<CustomerSalesReportViewModel>(),
            provider.GetRequiredService<CustomerTurnoverReportViewModel>()
        ));
    }

    private static void AddCommonServices(IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        MappingProfile.Register(config);
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
        services.AddSingleton<CommonReportDataService>();
        services.AddSingleton<INavigationService>(sp =>
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow!;
            return new NavigationService(mainWindow.MainFrame);
        });
    }
}