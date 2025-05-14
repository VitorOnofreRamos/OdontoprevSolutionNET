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
}
