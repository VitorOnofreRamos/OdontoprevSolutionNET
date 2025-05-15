using Auth.API.DTOs;
using Auth.API.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth.API.Services;

public class TokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public UserTokenDTO GenerateToken(User user)
    {
        // Get JWT settings from configuration
        var jwtKey = _configuration["JwtSettings:Key"];
        var jwtIssuer = _configuration["JwtSettings:Issuer"];
        var jwtAudience = _configuration["JwtSettings:Audience"];
        var jwtExpireMinutes = int.Parse(_configuration["JwtSettings:ExpireMinutes"]);

        // Setup JWT token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(jwtKey);

        var expiration = DateTime.UtcNow.AddMinutes(jwtExpireMinutes);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
        };

        // Adicionar CPF como claim opcional
        if (!string.IsNullOrEmpty(user.CPF))
        {
            claims.Add(new Claim("cpf", user.CPF));
        }

        // Adicionar telefone como claim opcional
        if (!string.IsNullOrEmpty(user.Phone))
        {
            claims.Add(new Claim("phone", user.Phone));
        }

        // Adicionar status ativo
        claims.Add(new Claim("active", user.Active.ToString().ToLower()));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            Issuer = jwtIssuer,
            Audience = jwtAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return new UserTokenDTO
        {
            Token = tokenHandler.WriteToken(token),
            Expiration = expiration,
            Username = user.Username,
            Email = user.Email,
            CPF = user.CPF,
            Phone = user.Phone,
            Role = user.Role
        };
    }

    public bool ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return false;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Key"]);

            // Validar token
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["JwtSettings:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["JwtSettings:Audience"],
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var isActive = jwtToken.Claims.FirstOrDefault(x => x.Type == "active")?.Value;

            return isActive == "true";
        }
        catch
        {
            return false;
        }
    }

    public UserInfo GetUserInfoFromToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var userId = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            var username = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            var role = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;
            var email = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var cpf = jwtToken.Claims.FirstOrDefault(x => x.Type == "cpf")?.Value;
            var phone = jwtToken.Claims.FirstOrDefault(x => x.Type == "phone")?.Value;
            var isActive = jwtToken.Claims.FirstOrDefault(x => x.Type == "active")?.Value == "true";

            if (!isActive)
                return null;

            return new UserInfo
            {
                Id = userId,
                Username = username,
                Email = email,
                CPF = cpf,
                Phone = phone,
                Role = role,
                Active = isActive
            };
        }
        catch
        {
            return null;
        }
    }
}

// Classe para informações do usuário
public class UserInfo
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string CPF { get; set; }
    public string Phone { get; set; }
    public string Role { get; set; }
    public bool Active { get; set; }
}