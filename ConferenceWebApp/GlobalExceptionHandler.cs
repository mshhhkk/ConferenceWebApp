using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NuGet.Common;
using System.Net;
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
            string.Equals(xrw, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
            return true;

        var accepts = req.GetTypedHeaders().Accept;
        return accepts != null && accepts.Any(a => a.MediaType.Value.Contains("json", StringComparison.OrdinalIgnoreCase));
    }
}
