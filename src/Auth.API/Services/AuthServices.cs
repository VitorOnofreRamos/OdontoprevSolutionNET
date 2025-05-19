using Auth.API.Models;
using Auth.API.DTOs;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Auth.API.Services
{
    public class AuthService
    {
        private readonly UserService _userService;
        private readonly string _jwtSecret;
        private readonly int _jwtExpirationMinutes;

        public AuthService(UserService userService, string jwtSecret, int jwtExpirationMinutes)
        {
            _userService = userService;
            _jwtSecret = jwtSecret;
            _jwtExpirationMinutes = jwtExpirationMinutes;
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO registerDto)
        {
            // Verificar se o email já está em uso
            var existingUser = await _userService.GetByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return null;
            }

            // Criar hash da senha
            CreatePasswordHash(registerDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            // Criar novo usuário
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                CPF = registerDto.CPF,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Phone = registerDto.Phone,
                Role = "User", // Padrão é usuário comum
                CreatedAt = DateTime.UtcNow,
                LastLogin = null,
                Active = true
            };

            await _userService.CreateAsync(user);

            // Gerar token JWT
            var token = GenerateJwtToken(user);

            return new AuthResponseDTO
            {
                Token = token,
                User = MapToUserDto(user)
            };
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO loginDto)
        {
            var user = await _userService.GetByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return null;
            }

            if (!VerifyPasswordHash(loginDto.Password, user.PasswordHash, user.PasswordSalt))
            {
                return null;
            }

            // Atualizar LastLogin
            user.LastLogin = DateTime.UtcNow;
            await _userService.UpdateAsync(user.Id, user);

            // Gerar token JWT
            var token = GenerateJwtToken(user);

            return new AuthResponseDTO
            {
                Token = token,
                User = MapToUserDto(user)
            };
        }

        private string GenerateJwtToken(User user)
        {
            Console.WriteLine($"JWT Secret used: '{_jwtSecret}'");
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            // Criando claims personalizadas com os nomes exatos que você deseja
            var claims = new List<Claim>
            {
                new Claim("_id", user.Id),
                new Claim("Email", user.Email),
                new Claim("Username", user.Username),
                new Claim("Role", user.Role),
                // Mantemos as claims de sistema para o funcionamento da autorização
                new Claim(ClaimTypes.Role, user.Role) // Necessário para [Authorize(Roles = "Admin")] funcionar
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
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
            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }
            return true;
        }

        private UserDTO MapToUserDto(User user)
        {
            return new UserDTO
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
        }
    }
}