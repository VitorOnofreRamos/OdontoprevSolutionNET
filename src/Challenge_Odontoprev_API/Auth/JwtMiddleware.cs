using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Builder;

namespace Challenge_Odontoprev_API.Auth;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public JwtMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].ToString()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                // Use the configuration directly rather than AuthService
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Key"]);

                // Validate token
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["JwtSettings:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["JwtSettings:Audience"],
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                // Extract user information from token
                var userId = jwtToken.Claims.FirstOrDefault(x => x.Type == "nameid")?.Value;
                var username = jwtToken.Claims.FirstOrDefault(x => x.Type == "unique_name")?.Value;
                var role = jwtToken.Claims.FirstOrDefault(x => x.Type == "role")?.Value;
                var email = jwtToken.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
                var cpf = jwtToken.Claims.FirstOrDefault(x => x.Type == "cpf")?.Value;
                var fone = jwtToken.Claims.FirstOrDefault(x => x.Type == "fone")?.Value;
                var isActive = jwtToken.Claims.FirstOrDefault(x => x.Type == "active")?.Value == "true";

                // Only attach user info if the account is active
                if (isActive)
                {
                    // Attach user info to context
                    context.Items["UserId"] = userId;
                    context.Items["Username"] = username;
                    context.Items["UserRole"] = role;
                    context.Items["UserEmail"] = email;
                    context.Items["UserCPF"] = cpf;
                    context.Items["UserFone"] = fone;
                }
            }
            catch
            {
                // Do nothing if token validation fails
                // User is not attached to context so the request won't have access to secured endpoints
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