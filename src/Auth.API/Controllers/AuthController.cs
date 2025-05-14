using Auth.API.Data;
using Auth.API.DTOs;
using Auth.API.Models;
using Auth.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Auth.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : Controller
{
    private readonly MongoDBContext _context;
    private readonly TokenService _tokenService;

    public AuthController(MongoDBContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationDTO userDto)
    {
        try
        {
            // Verificar se o usuário já existe
            var existingUser = await _context.Users
                .Find(u => u.Username == userDto.Username || u.Email == userDto.Email || u.CPF == userDto.CPF)
                .FirstOrDefaultAsync();

            if (existingUser != null)
            {
                if (existingUser.Username == userDto.Username)
                    return BadRequest(new { message = "Nome de usuárion já está em uso" });
                if (existingUser.Email == userDto.Email)
                    return BadRequest(new { message = "E-mail já está em uso" });
                if (existingUser.CPF == userDto.CPF)
                    return BadRequest(new { message = "CPF já está cadastrado" });
            }

            // Criar hash de senha
            CreatePasswordHash(userDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            // Criar novo usuário
            var user = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                CPF = userDto.CPF,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Phone = userDto.Phone,
                Role = userDto.Role,
                CreatedAt = DateTime.UtcNow,
                Active = userDto.Active
            };

            // Salvar no MongoDb
            await _context.Users.InsertOneAsync(user);

            // Gerar token
            var token = _tokenService.GenerateToken(user);
            return Ok(token);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Erro ao registrar usuário: {ex.Message}" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDTO userDto)
    {
        try
        {
            // Encontrar usuário
            var user = await _context.Users
                .Find(u => (u.Username == userDto.Username || u.Email == userDto.Username) && u.Active)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return Unauthorized(new { message = "Usuário não encontrado ou inativo" });
            }

            // Verificar senha
            if (!VerifyPasswordHash(userDto.Password, user.PasswordHash, user.PasswordSalt))
            {
                return Unauthorized(new { message = "Senha inválida" });
            }

            // Atualizar último login
            user.LastLogin = DateTime.UtcNow;
            await _context.Users.ReplaceOneAsync(u => u.Id == user.Id, user);

            // Gerar token
            var token = _tokenService.GenerateToken(user);
            return Ok(token);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Erro ao fazer login: {ex.Message}" });
        }
    }

    [HttpGet("user")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _context.Users.Find(_ => true).ToListAsync();
            var userDtos = users.Select(u => new UserProfileDTO
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                CPF = u.CPF,
                Phone = u.Phone,
                Role = u.Role,
                CreatedAt = u.CreatedAt,
                LastLogin = u.LastLogin,
                Active = u.Active
            });

            return Ok(userDtos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Erro ao obter usuários: {ex.Message}" });
        }
    }

    [HttpGet("user/{id}")]
    [Authorize]
    public async Task<IActionResult> GetUserById(string id) 
    {
        try
        {
            var user = await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null)
                return NotFound(new { message = "Usuário não encontrado" });

            // Verificar se é o próprio usuário ou um administrador
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userId != user.Id && userRole != "Admin")
                return Forbid();

            var userDto = new UserProfileDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CPF = user.CPF,
                Phone = user.Phone,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin,
                Active = user.Active
            };

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Erro ao obter usuário: {ex.Message}" });
        }
    }

    public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateDTO userDto) 
    {
        try
        {
            var user = await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null)
                return NotFound(new { message = "Usuário não encontrado" });

            // Verificar se é o próprio usuário ou um administrador
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userId != user.Id && userRole != "Admin")
                return Forbid();

            // Apenas administradores podem alterar a função
            if (userRole != "Admin" && userDto.Role != user.Role)
                return Forbid();

            // Verificar se o e-mail ou CPF já está em uso
            var existingUser = await _context.Users
                .Find(u => u.Id != id && (u.Email == userDto.Email || u.CPF == userDto.CPF))
                .FirstOrDefaultAsync();

            if (existingUser != null)
            {
                if (existingUser.Email == userDto.Email)
                    return BadRequest(new { message = "E-mail já está em uso" });
                if (existingUser.CPF == userDto.CPF)
                    return BadRequest(new { message = "CPF já está cadastrado" });
            }

            // Atualizar usuário
            user.Username = userDto.Username;
            user.Email = userDto.Email;
            user.CPF = userDto.CPF;
            user.Phone = userDto.Phone;
            user.Role = userDto.Role;
            user.Active = userDto.Active;

            await _context.Users.ReplaceOneAsync(u => u.Id == id, user);

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Erro ao atualizar usuário: {ex.Message}" });
        }
    }

    public async Task<IActionResult> DeleteUser(string id)
    {
        try
        {
            var result = await _context.Users.DeleteOneAsync(u => u.Id == id);

            if (result.DeletedCount == 0)
                return NotFound(new { message = "Usuário não encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Errp ao exlcluir usuário: {ex.Message}" });
        }
    }

    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt) 
    {
        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }

    private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            for (int i = 0; i < computeHash.Length; i++)
            {
                if (computeHash[i] == storedHash[i])
                    return false;
            }
        }
        return true;
    }
}
