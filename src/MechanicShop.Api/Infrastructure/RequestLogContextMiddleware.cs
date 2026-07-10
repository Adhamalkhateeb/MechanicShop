using Serilog.Context;

namespace MechanicShop.Api.Infrastructure;

public class RequestLogContextMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        using (LogContext.PushProperty("CorrelationId", context.TraceIdentifier))
        {
            await _next(context);
        }
    }
}

public static class RequestLogContextMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLogContextMiddleware>();
    }
}
