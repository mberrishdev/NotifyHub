using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NotifyHub.Api.Filters;
using NotifyHub.Api.Infrastructure.Options;
using NotifyHub.Application;
using NotifyHub.Application.Hubs;
using NotifyHub.Application.Interfaces;
using NotifyHub.Application.Services;
using NotifyHub.Infrastructure;
using NotifyHub.Persistence;
using NotifyHub.Persistence.Database;
using System.Text;

namespace NotifyHub.Api.Infrastructure.StartupConfiguration;

public static class ServiceConfiguration
{
    public static WebApplicationBuilder ConfigureService(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        IConfiguration configuration = builder.Configuration;
        var environment = builder.Environment;

        builder.Services.AddMemoryCache();

        services.AddControllers()
            .AddFluentValidation(config => config.RegisterValidatorsFromAssemblyContaining<INotificationService>());

        services.AddPersistence(configuration);
        services.AddApplication();
        services.AddInfrastructure(configuration);

        #region Health checks
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>();
        #endregion

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddVersioning();
        services.AddSwagger();
        services.AddHealthChecks();
        services.AddCors(configuration, environment);
        services.AddLogging();

        // Add JWT Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"] ?? "NotifyHub",
                    ValidAudience = configuration["Jwt:Audience"] ?? "NotifyHub",
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? "your-256-bit-secret"))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        // Add SignalR with authentication
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.MaximumReceiveMessageSize = 102400; // 100 KB
        });

        // Register application services
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IGroupSubscriptionService, GroupSubscriptionService>();
        services.AddScoped<IEventHistoryService, EventHistoryService>();
        services.AddScoped<IUserContextService, UserContextService>();
        services.AddScoped<INotificationHubService, NotificationHubService>();

        return builder;
    }

    #region Versioning
    private static IServiceCollection AddVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(setup =>
        {
            setup.DefaultApiVersion = new ApiVersion(1, 0);
            setup.AssumeDefaultVersionWhenUnspecified = true;
            setup.ReportApiVersions = true;
        });

        services.AddVersionedApiExplorer(setup =>
        {
            setup.GroupNameFormat = "'v'VVV";
            setup.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
    #endregion

    #region Swagger
    private static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.ConfigureOptions<ConfigureSwaggerOptions>()
            .AddSwaggerGen(swagger =>
            {
                swagger.DocumentFilter<HealthChecksFilter>();
            });

        return services;
    }
    #endregion

    #region Cors
    private static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.WithOrigins("http://localhost:5500")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .SetIsOriginAllowed(_ => true); // For development only
            });
        });

        return services;
    }
    #endregion
}