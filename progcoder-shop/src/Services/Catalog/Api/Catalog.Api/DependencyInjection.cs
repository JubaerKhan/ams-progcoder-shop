#region using

using BuildingBlocks.Authentication.Extensions;
using BuildingBlocks.DistributedTracing;
using BuildingBlocks.Logging;
using BuildingBlocks.Swagger.Extensions;
using Common.Configurations;
using Common.Constants;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Reflection;

#endregion

namespace Catalog.Api;

public static class DependencyInjection
{
    #region Methods

    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration cfg)
    {
        services.AddDistributedTracing(cfg);
        services.AddSerilogLogging(cfg);
        services.AddCarter();

        // Admin's browser calls this API directly (no gateway in this slice, unlike
        // the original architecture where CORS was handled once at the gateway).
        {
            var policyName = cfg["CorsConfig:PolicyName"] ?? "AllowSpecificOrigins";
            var domains = cfg.GetSection("CorsConfig:Domains").Get<string[]>() ?? [];
            services.AddCors(options => options.AddPolicy(policyName, policy => policy
                .WithOrigins(domains)
                .AllowAnyHeader()
                .AllowAnyMethod()));
        }

        // HealthChecks
        {
            var dbype = cfg[$"{ConnectionStringsCfg.Section}:{ConnectionStringsCfg.DbType}"];
            var conn = cfg[$"{ConnectionStringsCfg.Section}:{ConnectionStringsCfg.Database}"];

            switch (dbype)
            {
                case DatabaseType.SqlServer:
                    services.AddHealthChecks()
                        .AddSqlServer(connectionString: conn!);
                    break;
                case DatabaseType.MySql:
                    services.AddHealthChecks()
                        .AddMySql(connectionString: conn!);
                    break;
                case DatabaseType.PostgreSql:
                    services.AddHealthChecks()
                        .AddNpgSql(connectionString: conn!);
                    break;
                default:
                    throw new Exception("Unsupported database type");
            }
        }

        services.AddHttpContextAccessor();
        services.AddAuthenticationAndAuthorization(cfg);

        // AUTH BYPASS (demo mode): override the default authorization policy so it
        // allows anonymous callers. The endpoints still call RequireAuthorization(),
        // but with no-token requests now passing, the admin SPA (which no longer
        // logs in — see App.Admin KeycloakContext.jsx) can use the API over plain
        // HTTP on localhost and the host IP. Remove this block to re-enable
        // Keycloak-backed auth.
        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAssertion(_ => true)
                .Build();
            options.FallbackPolicy = null;
        });
        services.AddSwaggerServices(cfg);

        // Register all AutoMapper profiles from the current assembly
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        return services;
    }

    public static WebApplication UseApi(this WebApplication app)
    {
        var policyName = app.Configuration["CorsConfig:PolicyName"] ?? "AllowSpecificOrigins";
        app.UseCors(policyName);

        app.UseSerilogReqLogging();
        app.UsePrometheusEndpoint();
        app.MapCarter();
        app.UseExceptionHandler(options => { });
        app.UseHealthChecks("/health",
            new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseSwaggerApi();

        app.MapGet("/", (IWebHostEnvironment env) => new ApiDefaultPathResponse
        {
            Service = "Catalog.Api",
            Status = "Running",
            Timestamp = DateTimeOffset.UtcNow,
            Environment = env.EnvironmentName,
            Endpoints = new Dictionary<string, string>
            {
                { "health", "/health" }
            },
            Message = "API is running..."
        });

        return app;
    }

    #endregion

}
