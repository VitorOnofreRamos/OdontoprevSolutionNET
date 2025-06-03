//Controllers/AuthController.cs
using Auth.API.DTOs;
using Auth.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Auth.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDTO>> Register(RegisterDTO registerDto)
        {
            var response = await _authService.RegisterAsync(registerDto);

            if (response == null)
            {
                return BadRequest("Email já está em uso.");
            }

            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>> Login(LoginDTO loginDto)
        {
            var response = await _authService.LoginAsync(loginDto);

            if (response == null)
            {
                return Unauthorized("Email ou senha inválidos.");
            }

            return Ok(response);
        }
    }
}