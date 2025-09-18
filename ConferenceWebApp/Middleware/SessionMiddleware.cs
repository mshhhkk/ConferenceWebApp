namespace ConferenceWebApp.Middleware;

public class SessionMiddleware
{
    private readonly RequestDelegate _next;

    public SessionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Session.SetString("LastAccess", DateTime.UtcNow.ToString());

        await _next(context);
    }
}
