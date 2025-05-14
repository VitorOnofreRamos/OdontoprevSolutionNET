using Challenge_Odontoprev_API.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Challenge_Odontoprev_API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _authService.LoginAsync(request.Username, request.Password);

        if (string.IsNullOrEmpty(token))
            return Unauthorized(new { message = "Nome de usuário ou senha inálidos" });

        return Ok(new { token });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var success = await _authService.RegisterAsync(
                request.Username,
                request.Email,
                request.CPF,
                request.Password,
                request.Phone,
                request.Role);

        if (!success)
            return BadRequest(new { message = "Erro ao registrar usuário" });

        return Ok(new { message = "Usuário registrado com sucessos" });
    }

    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult GetCurrentUser()
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
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
            Fone = userPhone,
            Role = userRole
        });
    }

    [HttpGet("users")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer", "");
        var user = await _authService.GetAllUsersAsync(token);

        return Ok(user);
    }
}

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class RegisterRequest
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string CPF { get; set; }
    public string Password { get; set; }
    public string Phone { get; set; }
    public string Role { get; set; } = "User";
}
