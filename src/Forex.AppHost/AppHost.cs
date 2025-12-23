var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres-server")
    .WithDataVolume()
    .WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(8080));

var db = postgres.AddDatabase("forex");

var apiService = builder.AddProject<Projects.Forex_WebApi>("forex-webapi")
    .WaitFor(db)
    .WithReference(db);

builder.Build().Run();