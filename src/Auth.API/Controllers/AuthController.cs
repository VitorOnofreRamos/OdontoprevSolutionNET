using Auth.API.Models;
using Auth.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;

    public AuthController(IUserService userService, IJwtService jwtService)
    {
        _userService = userService;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthResult
            {
                Success = false,
                Message = "Invalid registration data"
            });
        }

        var user = await _userService.RegisterUserAsync(model);
        if (user == null)
        {
            return BadRequest(new AuthResult
            {
                Success = false,
                Message = "User with the same email already exists"
            });
        }

        var token = _jwtService.GenerateToken(user);

        return Ok(new AuthResult
        {
            Success = true,
            Token = token,
            Message = "User registered successfully",
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            }
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthResult
            {
                Success = false,
                Message = "Invalid login data"
            });
        }

        var user = await _userService.AuthenticateAsync(model.Email, model.Password);
        if (user == null)
        {
            return Unauthorized(new AuthResult
            {
                Success = false,
                Message = "Invalid email or password"
            });
        }

        var token = _jwtService.GenerateToken(user);

        return Ok(new AuthResult
        {
            Success = true,
            Token = token,
            Message = "Login successful",
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            }
        });
    }
}