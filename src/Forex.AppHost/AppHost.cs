var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres-server")
    .WithIconName("Database")
    .WithDataVolume(isReadOnly: false)
    .WithPgAdmin(pgAdmin =>
    {
        pgAdmin.WithHostPort(8080);
    })
    .AddDatabase("forex");

var apiService = builder.AddProject<Projects.Forex_WebApi>("forex-webapi")
    .WithIconName("API")
    .WaitFor(postgres)
    .WithReference(postgres);

builder.Build().Run();