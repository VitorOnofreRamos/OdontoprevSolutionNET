using Auth.API.Data;
using Auth.API.DTOs;
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

namespace Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MongoDBContext _context;
        private readonly TokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            MongoDBContext context,
            TokenService tokenService,
            ILogger<AuthController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Cria um novo usuário no banco de dados
        /// </summary>
        /// <remarks>
        /// Este endpoint apenas cria o usuário no banco de dados, não realiza autenticação.
        /// </remarks>
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromBody] UserRegistrationDTO userDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Tentativa de criação de usuário com dados inválidos");
                    return BadRequest(ModelState);
                }

                // Verifica se o usuário já existe
                var existingUser = await _context.Users
                    .Find(u => u.Username == userDto.Username || u.Email == userDto.Email || u.CPF == userDto.CPF)
                    .FirstOrDefaultAsync();

                if (existingUser != null)
                {
                    if (existingUser.Username == userDto.Username)
                    {
                        _logger.LogInformation("Tentativa de criar usuário com nome já existente: {Username}", userDto.Username);
                        return BadRequest(new { message = "Nome de usuário já está em uso" });
                    }

                    if (existingUser.Email == userDto.Email)
                    {
                        _logger.LogInformation("Tentativa de criar usuário com email já existente: {Email}", userDto.Email);
                        return BadRequest(new { message = "E-mail já está em uso" });
                    }

                    if (existingUser.CPF == userDto.CPF)
                    {
                        _logger.LogInformation("Tentativa de criar usuário com CPF já existente: {CPF}", userDto.CPF);
                        return BadRequest(new { message = "CPF já está cadastrado" });
                    }
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
                    Role = string.IsNullOrEmpty(userDto.Role) ? "User" : userDto.Role,
                    CreatedAt = DateTime.UtcNow,
                    Active = userDto.Active
                };

                // Salvar no MongoDB
                await _context.Users.InsertOneAsync(user);
                _logger.LogInformation("Usuário criado com sucesso: {Username}", user.Username);

                return Ok(new
                {
                    userId = user.Id,
                    message = "Usuário criado com sucesso"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar usuário: {ErrorMessage}", ex.Message);
                return StatusCode(500, new { message = "Erro interno ao registrar usuário" });
            }
        }

        /// <summary>
        /// Autentica um usuário e gera um token JWT
        /// </summary>
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] UserLoginDTO userDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Encontrar usuário pelo username ou email
                var user = await _context.Users
                    .Find(u => (u.Username == userDto.Username || u.Email == userDto.Username) && u.Active)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    _logger.LogWarning("Tentativa de login com usuário inexistente ou inativo: {Username}", userDto.Username);
                    return Unauthorized(new { message = "Usuário não encontrado ou inativo" });
                }

                // Verificar senha
                if (!VerifyPasswordHash(userDto.Password, user.PasswordHash, user.PasswordSalt))
                {
                    _logger.LogWarning("Tentativa de login com senha inválida para o usuário: {Username}", user.Username);
                    return Unauthorized(new { message = "Credenciais inválidas" });
                }

                // Atualizar último login
                user.LastLogin = DateTime.UtcNow;
                await _context.Users.ReplaceOneAsync(u => u.Id == user.Id, user);

                // Gerar token
                var tokenResponse = _tokenService.GenerateToken(user);
                _logger.LogInformation("Usuário autenticado com sucesso: {Username}", user.Username);

                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao autenticar usuário: {ErrorMessage}", ex.Message);
                return StatusCode(500, new { message = "Erro interno ao autenticar usuário" });
            }
        }

        /// <summary>
        /// Valida um token JWT e retorna as informações do usuário
        /// </summary>
        [HttpPost("validate")]
        public IActionResult ValidateToken([FromBody] TokenValidationRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Token))
                {
                    return BadRequest(new { message = "Token não informado" });
                }

                var isValid = _tokenService.ValidateToken(request.Token);
                if (!isValid)
                {
                    _logger.LogWarning("Validação de token falhou");
                    return Unauthorized(new { message = "Token inválido ou expirado" });
                }

                var userInfo = _tokenService.GetUserInfoFromToken(request.Token);
                if (userInfo == null)
                {
                    _logger.LogWarning("Não foi possível extrair informações do usuário do token");
                    return Unauthorized(new { message = "Token inválido" });
                }

                _logger.LogInformation("Token validado com sucesso para usuário: {Username}", userInfo.Username);
                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar token: {ErrorMessage}", ex.Message);
                return StatusCode(500, new { message = "Erro interno ao validar token" });
            }
        }

        /// <summary>
        /// Renova um token JWT para um usuário
        /// </summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Token))
                {
                    return BadRequest(new { message = "Token não informado" });
                }

                // Validar o token atual
                var isValid = _tokenService.ValidateToken(request.Token);
                if (!isValid)
                {
                    _logger.LogWarning("Tentativa de renovação de token inválido");
                    return Unauthorized(new { message = "Token inválido ou expirado" });
                }

                // Extrair informações do usuário do token
                var userInfo = _tokenService.GetUserInfoFromToken(request.Token);
                if (userInfo == null)
                {
                    return Unauthorized(new { message = "Não foi possível extrair informações do usuário do token" });
                }

                // Buscar o usuário no banco de dados
                var user = await _context.Users
                    .Find(u => u.Id == userInfo.Id && u.Active)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    _logger.LogWarning("Tentativa de renovação de token para usuário inexistente ou inativo: {UserId}", userInfo.Id);
                    return Unauthorized(new { message = "Usuário não encontrado ou inativo" });
                }

                // Gerar novo token
                var newToken = _tokenService.GenerateToken(user);
                _logger.LogInformation("Token renovado com sucesso para usuário: {Username}", user.Username);

                return Ok(newToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao renovar token: {ErrorMessage}", ex.Message);
                return StatusCode(500, new { message = "Erro interno ao renovar token" });
            }
        }

        /// <summary>
        /// Obtém informações de um usuário pelo ID
        /// </summary>
        [HttpGet("user/{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new { message = "ID de usuário não informado" });
                }

                var user = await _context.Users
                    .Find(u => u.Id == id)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    _logger.LogWarning("Tentativa de obter usuário inexistente: {UserId}", id);
                    return NotFound(new { message = "Usuário não encontrado" });
                }

                // Verificar se é o próprio usuário ou um administrador
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userId != user.Id && userRole != "Admin")
                {
                    _logger.LogWarning("Tentativa de acesso não autorizado às informações do usuário: {UserId}", id);
                    return Forbid();
                }

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

                _logger.LogInformation("Informações do usuário recuperadas com sucesso: {UserId}", id);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter usuário: {ErrorMessage}", ex.Message);
                return StatusCode(500, new { message = "Erro interno ao obter usuário" });
            }
        }

        #region Métodos Privados

        /// <summary>
        /// Cria um hash seguro para uma senha
        /// </summary>
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("A senha não pode ser nula ou vazia", nameof(password));
            }

            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        /// <summary>
        /// Verifica se uma senha corresponde ao hash armazenado
        /// </summary>
        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }

            if (storedHash.Length != 64)
            {
                return false;
            }

            if (storedSalt.Length != 128)
            {
                return false;
            }

            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(storedHash);
            }
        }

        #endregion
    }

    public class TokenValidationRequest
    {
        public string Token { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string Token { get; set; }
    }
}