using FCG.API.Extensions;

var builder = WebApplication.CreateBuilder(args);
var apiConfiguration = builder.Configuration.GetApiConfiguration();

builder.Services.AddApiDependencies(apiConfiguration);

var app = builder.Build();

await app.ApplyDatabaseMigrationsAsync(apiConfiguration);
app.UseApiPipeline();
app.MapApiEndpoints();

app.Run();
