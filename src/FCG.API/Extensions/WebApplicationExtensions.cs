using FCG.API.Endpoints;
using FCG.API.Middlewares;
using FCG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FCG.API.Extensions;

public static class WebApplicationExtensions
{
    public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app, ApiConfiguration configuration)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            context.Database.Migrate();
            await DatabaseSeeder.SeedAsync(context, configuration.SecretKey, configuration.AdminPassword);
            Console.WriteLine("Banco de dados atualizado com sucesso!");
        }
        catch (Exception ex)
        {
            app.Logger.LogCritical(ex, "Ocorreu um erro ao aplicar as migracoes ou executar o seed.");
            throw;
        }
    }

    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseMiddleware<UserLoggingScopeMiddleware>();
        app.UseAuthorization();

        return app;
    }

    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapGet("/", () => Results.Redirect("/swagger"))
                .ExcludeFromDescription();
        }

        app.MapAuthEndpoints();
        app.MapGameEndpoints();
        app.MapPromotionEndpoints();
        app.MapOrderEndpoints();
        app.MapAdminUserEndpoints();

        return app;
    }
}
