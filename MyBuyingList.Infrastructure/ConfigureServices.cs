using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyBuyingList.Application.Features.Users;
using MyBuyingList.Infrastructure.Persistence.Seeders;
using MyBuyingList.Infrastructure.Repositories;

namespace MyBuyingList.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabaseContext(configuration);
        services.AddRepositories(configuration);
        services.AddAdminSeeder();

        return services;
    }

    private static void AddAdminSeeder(this IServiceCollection services)
    {
        services.ConfigureOptions<AdminSettingsSetup>();
        services.AddScoped<AdminUserSeeder>();
    }

    private static void AddRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RepositorySettings>(configuration.GetSection("RepositorySettings"));
        services.AddScoped<IUserRepository, UserRepository>();
    }

    private static void AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = GetConnectionString(configuration);

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention();
        });
        services.AddDatabaseDeveloperPageExceptionFilter();
        services.AddScoped<ApplicationDbContext>();
    }

    private static string GetConnectionString(IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        if (connectionString is null)
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
        }

        return connectionString;
    }
}
