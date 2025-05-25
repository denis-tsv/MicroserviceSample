using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using Weather.Application.Commands;
using Weather.Infrastructure.DataAccess;
using Weather.WebApi.Extensions;
using Weather.WebApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebApi();
builder.Services.AddHealthChecks();
builder.Services.AddSwagger();
builder.Services.AddTelemetry(builder.Environment);
builder.Services.AddEntityFramework(builder.Configuration);
builder.Services.AddHttpClient(Options.DefaultName).AddStandardResilienceHandler();
builder.Services.AddWeatherServiceClient(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining(typeof(GetCurrentWeatherRequestDtoValidator));
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddMediatR(configuration => configuration.RegisterServicesFromAssemblyContaining<GetCurrentWeatherCommand>());
builder.Services.AddApplicationServices();
builder.Services.AddSingleton<IStartupFilter, GracefulShutdownStartupFilter>();
builder.WebHost.ConfigureWebHost();

var app = builder.Build();

app.MapOnInternalPort(internalPortApp =>
{
    internalPortApp.UseRouting();
    internalPortApp.UseEndpoints(endpoints =>
    {
        endpoints.MapHealthChecks("/healthz");
    });
    internalPortApp.UseOpenTelemetryPrometheusScrapingEndpoint();
});

app.MapOnPublicPort(publicPortApp =>
{
    if (!publicPortApp.ApplicationServices.GetRequiredService<IWebHostEnvironment>().IsProduction())
    {
        publicPortApp.UseSwagger();
        publicPortApp.UseSwaggerUI();
    }

    publicPortApp.UseMiddleware<RequestIdMiddleware>();
    publicPortApp.UsePathBase("/api/v1");
    publicPortApp.UseRouting();
    publicPortApp.UseCors();
    publicPortApp.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
});

if (app.Environment.IsDevelopment())
{
    app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

app.Run();

public partial class Program { }