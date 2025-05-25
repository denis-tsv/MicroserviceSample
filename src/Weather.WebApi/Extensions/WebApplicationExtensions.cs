namespace Weather.WebApi.Extensions;

public static class WebApplicationExtensions
{
    public const int DefaultPublicPort = 8080;
    public const int DefaultInternalPort = 8082;
    
    public static IApplicationBuilder MapOnInternalPort(
        this WebApplication app,
        Action<IApplicationBuilder> builder)
    {
        return app.MapWhen(ctx => ctx.Connection.LocalPort == DefaultInternalPort, builder);
    }

    public static IApplicationBuilder MapOnPublicPort(
        this WebApplication app,
        Action<IApplicationBuilder> builder)
    {
        return app.MapWhen(ctx =>
                ctx.Connection.LocalPort == DefaultPublicPort
                || (!ctx.RequestServices.GetRequiredService<IHostEnvironment>().IsProduction() && ctx.Connection.LocalPort == 0),// for integration tests
            builder);
    }
}