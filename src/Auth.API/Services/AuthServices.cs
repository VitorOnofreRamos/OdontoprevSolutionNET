//Services/AuthService.cs
using Auth.API.Models;
using Auth.API.DTOs;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

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
            var passwordHash = CreatePasswordHash(registerDto.Password);

            // Criar novo usuário
            var user = new User
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                Phone = registerDto.Phone,
                Role = "DONOR", // Padrão é DONOR
                CreatedAt = DateTime.UtcNow,
                LastLogin = null,
                IsActive = "Y",
                OrganizationId = registerDto.OrganizationId
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
            if (user == null || user.IsActive != "Y")
            {
                return null;
            }

            if (!VerifyPasswordHash(loginDto.Password, user.PasswordHash))
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
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            // Claims usando o padrão JwtRegisteredClaimNames para melhor compatibilidade
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role) // Mantemos ClaimTypes.Role para autorização
            };

            // Adicionar organization_id se existir
            if (user.OrganizationId.HasValue)
            {
                claims.Add(new Claim("organization_id", user.OrganizationId.Value.ToString()));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes),
                Issuer = "Auth.API",
                Audience = "OdontoprevClients",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string CreatePasswordHash(string password)
        {
            // Usando BCrypt para hash de senha (mais seguro que HMAC)
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            // Verificar senha usando BCrypt
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

        private UserDTO MapToUserDto(User user)
        {
            return new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin,
                IsActive = user.IsActive == "Y",
                OrganizationId = user.OrganizationId
            };
        }
    }
}