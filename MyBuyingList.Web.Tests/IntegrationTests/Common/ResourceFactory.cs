using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyBuyingList.Application.Common.Interfaces;
using MyBuyingList.Domain.Entities;
using MyBuyingList.Infrastructure;
using MyBuyingList.Infrastructure.Persistence.Seeders;
using MyBuyingList.Web.Tests.IntegrationTests.Common.Logging;
using Testcontainers.PostgreSql;

namespace MyBuyingList.Web.Tests.IntegrationTests.Common;

// Video I got inspired by: https://www.youtube.com/watch?v=E4TeWBFzcCw
public class ResourceFactory : WebApplicationFactory<AssemblyMarker>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    private readonly int _exposedPort = new Random().Next(1000, 10000);

    public IConfiguration Configuration { get; private set; } = null!;
    public HttpClient HttpClient { get; private set; } = null!;
    public FakeLogCollector LogCollector { get; } = new FakeLogCollector();

    public ResourceFactory()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:14")
            .WithUsername("myuser")
            .WithPassword("password")
            .WithDatabase("db")
            .WithHostname("localhost")
            .WithPortBinding(_exposedPort, 5432)
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        ConfigurationManager manager = new();
        manager
            .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.IntegrationTests.json"), optional: false)
            .Build();

        builder.UseConfiguration(manager);
        Configuration = manager;

        builder.ConfigureLogging(logging => logging.AddProvider(new FakeLoggerProvider(LogCollector)));

        builder.ConfigureTestServices(services =>
        {
            // Replace with proper DbContext
            var descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            services.Remove(descriptor);

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options
                    .UseNpgsql(_dbContainer.GetConnectionString())
                    .UseSnakeCaseNamingConvention();
            });
            services.AddScoped<ApplicationDbContext>();

            // TestServer uses http://localhost — Secure cookies won't be sent over plain HTTP.
            // Override to SameAsRequest so the CookieContainer sends auth cookies in tests.
            services.PostConfigureAll<CookieAuthenticationOptions>(options =>
            {
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });
        });
    }

    public async ValueTask InitializeAsync()
    {
        await _dbContainer.StartAsync();

        await SeedAdminAsync();
        await SeedIntegrationAdminAsync();

        HttpClient = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        await Utils.LoginAsync(HttpClient, Utils.IntegrationTestAdminUsername, Utils.IntegrationTestAdminPassword);
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        using IServiceScope scope = Services.CreateScope();
        ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        await SeedAdminAsync();
        await SeedIntegrationAdminAsync();
    }

    public async Task<int> InsertTestUserAsync()
    {
        using IServiceScope scope = Services.CreateScope();
        ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        IPasswordEncryptionService passwordService = scope.ServiceProvider.GetRequiredService<IPasswordEncryptionService>();

        User user = new()
        {
            UserName = Utils.TestUserUsername,
            Email = Utils.TestUserEmail,
            Password = passwordService.HashPassword(Utils.TestUserPassword),
            Active = true
        };

        db.Set<User>().Add(user);
        await db.SaveChangesAsync();
        return user.Id;
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync(string username, string password)
    {
        HttpClient client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        await Utils.LoginAsync(client, username, password);
        return client;
    }

    private async Task SeedAdminAsync()
    {
        using IServiceScope scope = Services.CreateScope();
        AdminUserSeeder seeder = scope.ServiceProvider.GetRequiredService<AdminUserSeeder>();
        await seeder.SeedAsync();
    }
    
    private async Task SeedIntegrationAdminAsync()
    {
        using IServiceScope scope = Services.CreateScope();
        ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        IPasswordEncryptionService passwordService = scope.ServiceProvider.GetRequiredService<IPasswordEncryptionService>();

        User user = new()
        {
            UserName = Utils.IntegrationTestAdminUsername,
            Email = "integration_admin@test.local",
            Password = passwordService.HashPassword(Utils.IntegrationTestAdminPassword),
            Active = true
        };

        db.Set<User>().Add(user);
        await db.SaveChangesAsync();

        User insertedUser = await db.Set<User>()
            .SingleAsync(u => u.UserName == Utils.IntegrationTestAdminUsername);

        db.Set<UserRole>().Add(new UserRole { UserId = insertedUser.Id, RoleId = 1 });
        await db.SaveChangesAsync();
    }
}
