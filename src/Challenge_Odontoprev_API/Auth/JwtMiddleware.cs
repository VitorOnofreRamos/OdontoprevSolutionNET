using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Challenge_Odontoprev_API.Auth;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].ToString()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                // Obter o serviço AuthService do escopo atual
                var authService = context.RequestServices.GetRequiredService<AuthService>();

                // Validar token com o Auth.API
                var isValid = await authService.ValidateTokenAsync(token);

                if (isValid)
                {
                    // Obter informações do usuário do token
                    var userInfo = await authService.GetUserInfoFromTokenAsync(token);

                    if (userInfo != null && userInfo.Active)
                    {
                        // Adicionar informações do usuário ao contexto
                        context.Items["UserId"] = userInfo.Id;
                        context.Items["Username"] = userInfo.Username;
                        context.Items["UserRole"] = userInfo.Role;
                        context.Items["UserEmail"] = userInfo.Email;
                        context.Items["UserCPF"] = userInfo.CPF;
                        context.Items["UserPhone"] = userInfo.Phone;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log da exceção, mas continua o fluxo
                Console.WriteLine($"Erro ao processar token JWT: {ex.Message}");
                // As informações do usuário não serão adicionadas ao contexto
                // e as solicitações para endpoints protegidos serão rejeitadas
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
}