using System.Security.Claims;

namespace FCG.API.Middlewares;

public sealed class UserLoggingScopeMiddleware(RequestDelegate next, ILogger<UserLoggingScopeMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("user_id");
        var userEmail = context.User.FindFirstValue(ClaimTypes.Email)
            ?? context.User.FindFirstValue("email");

        using (logger.BeginScope(new Dictionary<string, object?>
        {
            ["UserId"] = userId,
            ["UserEmail"] = userEmail
        }))
        {
            await next(context);
        }
    }
}
