using Microsoft.Extensions.DependencyInjection;
using NotifyHub.Application.Services;

namespace NotifyHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register services

        services.AddScoped<ITokenService, TokenService>();
        return services;
    }
}