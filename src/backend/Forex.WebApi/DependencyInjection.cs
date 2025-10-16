namespace Forex.WebApi;

using Forex.Application;
using Forex.Infrastructure;
using Forex.WebApi.Conventions;
using Forex.WebApi.Extensions;
using Forex.WebApi.Middlewares;
using Forex.WebApi.OpenApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using System.Text.Json.Serialization;

public static class DependencyInjection
{
    public static void AddDependencies(this IServiceCollection services, IConfiguration conf)
    {
        services.AddApplicationServices();
        services.AddInfrastructureServices(conf);

        services.AddApiControllers();
        services.AddValidation();

        services.AddOpenApiDocumentation();
        services.AddJwtAuthentication(conf);
        services.AddAppCors();
    }

    public static void UseInfrastructure(this WebApplication app)
    {
        //app.UseMiddleware<ExceptionHandlerMiddleware>();

        app.UseHttpsRedirection();
        app.UseCors("DefaultPolicy");

        app.UseAuthentication();
        app.UseAuthorization();

        app.ApplyMigrations();
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
            });
        }
    }

    #region private helpers

    private static void AddOpenApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi("v1", options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        });
    }

    private static void AddJwtAuthentication(this IServiceCollection services, IConfiguration conf)
    {
        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(x =>
        {
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = conf["Jwt:Issuer"],
                ValidAudience = conf["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(conf["Jwt:Key"]!)
                )
            };
        });
    }

    private static void AddApiControllers(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
        })
        .AddJsonOptions(opt =>
            opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    }

    private static void AddValidation(this IServiceCollection services)
    {
        //services.Configure((Action<ApiBehaviorOptions>)(options =>
        //{
        //    options.InvalidModelStateResponseFactory = context =>
        //        throw new ValidationAppException(context.ModelState
        //            .SelectMany(kvp => kvp.Value!.Errors
        //                .Select(e => new FluentValidation.Results.ValidationFailure(kvp.Key, e.ErrorMessage))));
        //}));
    }

    private static void AddAppCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", policy =>
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader());
        });
    }

    #endregion
}
