using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Challenge_Odontoprev_API.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public AuthorizeAttribute(params string[] roles)
    {
        _roles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Pular autorização se a ação tiver o atributo [AllowAnonymous]
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata
            .Any(em => em.GetType() == typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute));

        if (allowAnonymous)
            return;

        // Verificar se o usuário está autenticado
        var userId = context.HttpContext.Items["UserId"]?.ToString();
        if (string.IsNullOrEmpty(userId))
        {
            // Não está autenticado
            context.Result = new UnauthorizedObjectResult(new { message = "Não autorizado" });
            return;
        }

        // Verificar se a ação requer uma função específica
        if (_roles != null && _roles.Length > 0)
        {
            var userRole = context.HttpContext.Items["UserRole"]?.ToString();
            if (string.IsNullOrEmpty(userRole) || !_roles.Contains(userRole))
            {
                // Função não permitida
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}

