using Microsoft.Extensions.DependencyInjection;
using MyBuyingList.Application.Features.Login.Services;
using MyBuyingList.Application.Features.Users.Services;
using MyBuyingList.Application.Common.Interfaces;
using MyBuyingList.Application.Common.Services;

namespace MyBuyingList.Application;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddTransient<IPasswordEncryptionService, PasswordEncryptionService>();

        return services;
    }
}
