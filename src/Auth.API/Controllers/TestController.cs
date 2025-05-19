using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auth.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TesteController : ControllerBase
    {
        [HttpGet("publico")]
        [AllowAnonymous]
        public IActionResult EndpointPublico()
        {
            return Ok(new { mensagem = "Este endpoint é público e não requer autenticação" });
        }
        
        [HttpGet("autenticado")]
        [Authorize]
        public IActionResult EndpointAutenticado()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            return Ok(new { 
                mensagem = "Você está autenticado!",
                userId,
                userName,
                userEmail,
                userRole,
                claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
        }
        
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult EndpointAdmin()
        {
            return Ok(new { mensagem = "Você tem acesso de administrador!" });
        }
        
        [HttpGet("verificar-token")]
        [AllowAnonymous]
        public IActionResult VerificarToken()
        {
            // Extrair o token do cabeçalho Authorization
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            
            if (string.IsNullOrEmpty(authHeader))
            {
                return Ok(new { 
                    status = "erro", 
                    mensagem = "Cabeçalho Authorization não encontrado" 
                });
            }
            
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Ok(new { 
                    status = "erro", 
                    mensagem = "Token não está no formato Bearer" 
                });
            }
            
            var token = authHeader.Substring("Bearer ".Length).Trim();
            var partes = token.Split('.');
            
            return Ok(new {
                status = "info",
                token_resumido = token.Length > 20 ? token.Substring(0, 20) + "..." : token,
                token_tamanho = token.Length,
                token_partes = partes.Length,
                token_formato_valido = partes.Length == 3
            });
        }
    }
}