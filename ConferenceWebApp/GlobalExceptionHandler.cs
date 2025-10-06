using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Text.Json;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken token)
    {
        if (WantsJson(ctx.Request))
        {
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            ctx.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = 500,
                Title = "Internal Server Error",
                Detail = _env.IsDevelopment() ? ex.ToString() : "Непредвиденная ошибка",
                Instance = ctx.Request.Path
            };
            problem.Extensions["traceId"] = ctx.TraceIdentifier;

            await ctx.Response.WriteAsync(JsonSerializer.Serialize(
                problem,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }), token);

            return true;
        }

        return false;
    }


    private static bool WantsJson(HttpRequest req)
    {
        if (req.Headers.TryGetValue("X-Requested-With", out var xrw) &&
            StringValues.Equals(xrw, "XMLHttpRequest"))
            return true;

        var accept = req.GetTypedHeaders().Accept;
        if (accept is null || accept.Count == 0) return false;

        foreach (var a in accept)
        {
            if (a is null) continue;

            if (a.MediaType.HasValue &&
                a.MediaType.Value.Equals("application/json", StringComparison.OrdinalIgnoreCase))
                return true;

            if (a.Suffix.HasValue &&
                a.Suffix.Value.Equals("json", StringComparison.OrdinalIgnoreCase))
                return true;

            if (a.MediaType.HasValue &&
                a.MediaType.Value.IndexOf("json", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }

        return false;
    }
}
