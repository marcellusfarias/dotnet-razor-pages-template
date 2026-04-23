using Microsoft.EntityFrameworkCore;
using MyBuyingList.Infrastructure;
using MyBuyingList.Infrastructure.Persistence.Seeders;
using MyBuyingList.Web.Middlewares.CorrelationId;

namespace MyBuyingList.Web;

internal static class ConfigureApp
{
    internal static async Task StartApplication(this WebApplication app)
    {
        try
        {
            await app.RunDatabaseMigrations();
        }
        catch (Exception ex)
        {
            app.Logger.LogError("Failed running migrations. Err: {ExMessage}, Exception: {ExInnerException}", ex.Message, ex.InnerException);
            await app.StopAsync();
        }

        app.Logger.LogInformation("Migration ran successfully");

        app.AddMiddlewares();
    }

    private static async Task RunDatabaseMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        AdminUserSeeder seeder = scope.ServiceProvider.GetRequiredService<AdminUserSeeder>();
        await seeder.SeedAsync();
    }

    private static void AddMiddlewares(this WebApplication app)
    {
        app.UseSecurityHeaders();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/error");
        }

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseRouting();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapRazorPages();
        app.MapHealthChecks("/health").AllowAnonymous();
    }

    private static void UseSecurityHeaders(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            await next();
        });
    }
}
