using Challenge_Odontoprev_API.Auth;
using Challenge_Odontoprev_API.Models;
using Challenge_Odontoprev_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Threading.Tasks;

namespace Challenge_Odontoprev_API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly AuthService _authService;

    public UsersController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] UserRegisterDTO request)
    {
        try
        {
            // 1. Validar os dados do usuário
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 2. Criar o usuário no Auth.API
            var userId = await _authService.RegisterUserAsync(
                request.Username,
                request.Email,
                request.CPF,
                request.Password,
                request.Phone,
                request.Role);

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "Erro ao registrar usuário" });
            }

            // 3. Autenticar o usuário recém-criado
            var token = await _authService.AuthenticateAsync(request.Username, request.Password);

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Erro ao autenticar usuário após registro" });
            }

            // 4. Retornar token e dados do usuário
            return Ok(new
            {
                userId,
                token,
                username = request.Username,
                email = request.Email,
                role = request.Role
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Erro ao registrar usuário: {ex.Message}" });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] UserLoginDTO request)
    {
        try
        {
            var token = await _authService.AuthenticateAsync(request.Username, request.Password);

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "Nome de usuário ou senha inválidos" });
            }

            // Obter informações do usuário a partir do token
            var userInfo = await _authService.GetUserInfoFromTokenAsync(token);

            return Ok(new
            {
                token,
                userInfo.Username,
                userInfo.Email,
                userInfo.Role,
                userInfo.CPF,
                userInfo.Phone
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Erro ao fazer login: {ex.Message}" });
        }
    }

    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Usuário não autenticado" });
            }

            var username = HttpContext.Items["Username"]?.ToString();
            var userRole = HttpContext.Items["UserRole"]?.ToString();
            var userEmail = HttpContext.Items["UserEmail"]?.ToString();
            var userCPF = HttpContext.Items["UserCPF"]?.ToString();
            var userPhone = HttpContext.Items["UserPhone"]?.ToString();

            return Ok(new
            {
                UserId = userId,
                Username = username,
                Email = userEmail,
                CPF = userCPF,
                Phone = userPhone,
                Role = userRole
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Erro ao obter usuário atual: {ex.Message}" });
        }
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var newToken = await _authService.RefreshTokenAsync(request.Token);

            if (string.IsNullOrEmpty(newToken))
            {
                return Unauthorized(new { message = "Token inválido ou expirado" });
            }

            return Ok(new { token = newToken });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Erro ao renovar token: {ex.Message}" });
        }
    }
}

public class UserRegisterDTO
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string CPF { get; set; }
    public string Password { get; set; }
    public string Phone { get; set; }
    public string Role { get; set; } = "User";
}

public class UserLoginDTO
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class RefreshTokenRequest
{
    public string Token { get; set; }
}