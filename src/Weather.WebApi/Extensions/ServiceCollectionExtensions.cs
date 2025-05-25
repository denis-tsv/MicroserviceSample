using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Weather.Application.Services;
using Weather.Domain.Services;
using Weather.Infrastructure;
using Weather.Infrastructure.Abstractions.DataAccess;
using Weather.Infrastructure.Abstractions.Metrics;
using Weather.Infrastructure.Abstractions.WeatherServiceClient;
using Weather.Infrastructure.DataAccess;
using Weather.WebApi.Mertics;

namespace Weather.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebApi(this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

        return services;
    }
    
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SupportNonNullableReferenceTypes();
            options.MapType<DateOnly>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "date",
            });
        });
        
        return services;
    }
    
    public static IServiceCollection AddTelemetry(this IServiceCollection services, IWebHostEnvironment environment)
    {
        services.AddSingleton<IMetricCollector, MetricCollector>();
        
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(environment.ApplicationName))
            .WithTracing(provider =>
            {
                provider.AddHttpClientInstrumentation();
                provider.AddNpgsql();
                provider.AddAspNetCoreInstrumentation();

                provider.AddOtlpExporter();
            })
            .WithMetrics(provider =>
            {
                provider.AddMeter(MetricCollector.MeterName);
                provider.AddAspNetCoreInstrumentation();
            
                provider.AddPrometheusExporter();
            });
        
        return services;
    }
    
    public static IServiceCollection AddEntityFramework(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextPool<IAppDbContext, AppDbContext>(optionsBuilder =>
        {
            var dataSource = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("Weather"))
                .BuildMultiHost()
                .WithTargetSession(TargetSessionAttributes.Primary);
        
            optionsBuilder
                .UseNpgsql(dataSource, 
                    opt => opt.MigrationsHistoryTable("_migrations", "weather"))
                .UseSnakeCaseNamingConvention();
        });
        
        return services;
    }
    
    public static IServiceCollection AddWeatherServiceClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<WeatherStackConfiguration>(configuration.GetSection("WeatherStack"));
        services.AddScoped<IWeatherServiceClient, WeatherStackServiceClient>();
        
        return services;
    }
    
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IWeatherApplicationService, WeatherApplicationService>();
        services.AddScoped<IWeatherForecastService, WeatherForecastService>();

        
        return services;
    }
    
    public static ConfigureWebHostBuilder ConfigureWebHost(this ConfigureWebHostBuilder webHost)
    {
        webHost.ConfigureKestrel(serverOptions =>
        {
            // Internal port. Used for k8s probes (readiness, health check) and exposing metrics
            serverOptions.Listen(IPAddress.Any, WebApplicationExtensions.DefaultInternalPort);

            // Public port. REST API
            serverOptions.Listen(IPAddress.Any, WebApplicationExtensions.DefaultPublicPort);
        });
        webHost.UseShutdownTimeout(TimeSpan.FromSeconds(10));
        
        
        return webHost;
    }
}