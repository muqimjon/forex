namespace Forex.WebApi.Extensions;

using Forex.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public static class MigrationExtensions
{
    public static IApplicationBuilder ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        dbContext.Database.Migrate();
        return app;
    }
}
