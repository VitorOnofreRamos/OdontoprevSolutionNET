using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Builder;

namespace Challenge_Odontoprev_API.Auth;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AuthService _authService;

    public JwtMiddleware(RequestDelegate next, AuthService authService)
    {
        _next = next;
        _authService = authService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].ToString()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            //Validar o token
            var isValid = await _authService.ValidateTokenAsync(token);
            if (isValid)
            {
                //Obter informações do usuário
                var userInfo = await _authService.GetUserInfoFromTokenAsync(token);
                if (userInfo != null)
                {
                    // Anexar informações do usuário ao contexto
                    context.Items["UserId"] = userInfo.Id;
                    context.Items["Username"] = userInfo.Username;
                    context.Items["UserRole"] = userInfo.Role;
                    context.Items["UserEmail"] = userInfo.Email;
                }
            }
        }

        await _next(context);
    }
}

// Classe de extensão para usar o middleware
public static class JwtMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtMiddleware>();
    }
};