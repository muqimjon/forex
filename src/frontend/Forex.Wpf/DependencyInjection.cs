namespace Forex.Wpf;

using Forex.ClientService;
using Forex.Wpf.Common;
using Forex.Wpf.Common.Interfaces;
using Forex.Wpf.Common.Services;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        // 🔸 1. Client servislari (API, Minio, Auth va h.k.)
        AddClientLayer(services, config);

        // 🔸 2. ViewModel, Windows, Pages ro‘yxatdan o‘tkazish
        AddUiLayer(services);

        // 🔸 3. Qo‘shimcha helper yoki utility servislari
        AddCommonServices(services);

        return services;
    }

    private static void AddClientLayer(IServiceCollection services, IConfiguration config)
    {
        services.AddClientServices(config);
    }

    private static void AddUiLayer(IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var windows = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(Window).IsAssignableFrom(t));
        foreach (var w in windows)
            services.AddSingleton(w);

        var pages = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(Page).IsAssignableFrom(t));
        foreach (var p in pages)
            services.AddTransient(p);

        var viewModels = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("ViewModel"));
        foreach (var vm in viewModels)
            services.AddTransient(vm);
    }

    private static void AddCommonServices(IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;

        MappingProfile.Register(config);
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
    }
}
