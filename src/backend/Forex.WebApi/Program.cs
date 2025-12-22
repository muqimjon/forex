using Forex.WebApi;

var builder = WebApplication.CreateBuilder(args);

// Service registrations
builder.Services.AddDependencies(builder.Configuration);

var app = builder.Build();

// Middleware pipeline
app.UseInfrastructure(); // HTTPS, CORS, Auth
app.UseOpenApiDocumentation(); // Scalar UI

await app.UseSeedData();
app.MapControllers();

app.Run();
