namespace Weather.WebApi.Middlewares;

public class GracefulShutdownStartupFilter : IStartupFilter
{
    private readonly IHostApplicationLifetime _hostLifetime;

    private readonly int _gracefulShutdownSeconds;

    public GracefulShutdownStartupFilter(
        IHostApplicationLifetime hostLifetime,
        IConfiguration configuration)
    {
        _hostLifetime = hostLifetime;
        _gracefulShutdownSeconds = configuration.GetValue<int>(WebHostDefaults.ShutdownTimeoutKey);
    }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return builder =>
        {
            if (_gracefulShutdownSeconds > 0)
            {
                _hostLifetime.ApplicationStopping.Register(() =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(_gracefulShutdownSeconds));
                });
            }

            next(builder);
        };
    }
}