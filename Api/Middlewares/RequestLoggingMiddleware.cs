using System.Diagnostics;

namespace Api.Middlewares;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        await next(context);

        sw.Stop();

        logger.LogInformation("HTTP {Method} {Path} responded {StatusCode} in {Elapsed} ms Correlation={CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            sw.ElapsedMilliseconds,
            context.TraceIdentifier);
    }
}