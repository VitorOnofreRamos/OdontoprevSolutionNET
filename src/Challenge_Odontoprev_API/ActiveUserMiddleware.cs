using Challenge_Odontoprev_API.Services;
using System.Security.Claims;

namespace Challenge_Odontoprev_API;

public class ActiveUserMiddleware
{
    private readonly RequestDelegate _next;

    public ActiveUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserValidationService userValidationService)
    {
        // Skip for anonymous endpoints
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        // Get use ID and token
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(token))
        {
            // Check if user is active
            var isActive = await userValidationService.IsUserActiveAsync(userId, token);
            if (isActive)
            {
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsJsonAsync(new { message = "User account is not active" });
                return;
            }
        }
        await _next(context);
    }
}

// Extension method to register the middleware
public static class ActiveUserMiddlewareExtensions
{
    public static IApplicationBuilder UseActiveUserMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ActiveUserMiddleware>();
    }
}
