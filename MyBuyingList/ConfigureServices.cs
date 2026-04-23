using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using MyBuyingList.Application;
using MyBuyingList.Application.Common.Interfaces;
using MyBuyingList.Application.Common.Options;
using MyBuyingList.Infrastructure;
using MyBuyingList.Web.Configuration;
using MyBuyingList.Web.Middlewares.Authorization;
using MyBuyingList.Web.Middlewares.CorrelationId;
using MyBuyingList.Web.Middlewares.Filters;
using MyBuyingList.Web.Middlewares.RateLimiting;
using MyBuyingList.Web.Services;
using System.Net;

namespace MyBuyingList.Web;

internal static class ConfigureServices
{
    internal static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddExternalServices(configuration);
        services.AddRateLimitService(configuration);
        services.AddAuthenticationServices(configuration);
        services.AddAuthorizationServices();
        services.AddDataProtectionService(configuration);
        services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<CorrelationIdProvider>();
        services.AddScoped<ICorrelationIdProvider>(sp => sp.GetRequiredService<CorrelationIdProvider>());
        services.AddRazorPages()
            .AddMvcOptions(options => options.Filters.Add<BasePageFilter>());
        services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
        services.Configure<LockoutOptions>(configuration.GetSection(LockoutOptions.SectionName));
    }

    private static void AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<CookieAuthOptions>()
            .BindConfiguration(CookieAuthOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        CookieAuthOptions cookieOptions = configuration
            .GetSection(CookieAuthOptions.SectionName)
            .Get<CookieAuthOptions>() ?? new CookieAuthOptions();

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/login";
                options.AccessDeniedPath = "/access-denied";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.SlidingExpiration = cookieOptions.SlidingExpiration;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(cookieOptions.ExpirationMinutes);
            });
    }

    private static void AddAuthorizationServices(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
    }

    private static void AddDataProtectionService(this IServiceCollection services, IConfiguration configuration)
    {
        string keysPath = configuration["DataProtection:KeysPath"]
            ?? throw new InvalidOperationException("DataProtection:KeysPath not found in configuration.");

        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(keysPath));
    }

    private static void AddRateLimitService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CustomRateLimiterOptions>(configuration.GetSection("CustomRateLimiterOptions"));
        services.AddRateLimiter(options =>
        {
            options.AddPolicy<IPAddress, AuthenticationRateLimiterPolicy>(AuthenticationRateLimiterPolicy.PolicyName);
        });
    }

    private static void AddExternalServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructureServices(configuration);
        services.AddApplicationServices();
    }
}
