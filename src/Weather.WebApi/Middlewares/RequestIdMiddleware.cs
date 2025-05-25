using System.Diagnostics;

namespace Weather.WebApi.Middlewares;

public sealed class RequestIdMiddleware
{
    private readonly RequestDelegate _next;

    public RequestIdMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context)
    {
        if (Activity.Current != null)
            context.Response.Headers.Add("x-trace-id", Activity.Current.TraceId.ToString());
        
        return _next(context);
    }
}
