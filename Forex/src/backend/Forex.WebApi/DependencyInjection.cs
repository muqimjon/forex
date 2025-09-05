namespace Forex.WebApi;

using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using Forex.Application;
using Forex.Infrastructure;
using Forex.WebApi.Conventions;

public static class DependencyInjection
{
    public static void AddDependencies(this IServiceCollection services, IConfiguration conf)
    {
        services.AddApplicationServices();
        services.AddInfrastructureServices(conf);
        services.AddControllers(options
            => options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer())))
                .AddJsonOptions(opt => opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddOpenApi();
    }

    public static void UseOpenApiDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(opt =>
            {
                opt.WithTheme(ScalarTheme.BluePlanet);
                opt.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
                opt.WithTitle("Forex API Documentation");
                //opt.WithLayout(ScalarLayout.Classic);
                //opt.WithFavicon("favicon.ico");
            });
        }
    }

    public static void UseInfrastructure(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseCors(s => s
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

        app.UseAuthorization();
    }
}
