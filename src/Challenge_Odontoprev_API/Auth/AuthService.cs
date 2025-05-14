using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Challenge_Odontoprev_API.Auth;

public class AuthService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _authApiUrl;

    public AuthService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _authApiUrl = _configuration["AuthSettings:ApiUrl"] ?? "http//auth-api";
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
            return false;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Key"]);

            // Validar token localmente
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

            // Verificar se o usuário está ativo
            var jwtToken = (JwtSecurityToken)validatedToken;
            var isActive = jwtToken.Claims.FirstOrDefault(x => x.Type == "active")?.Value;

            return isActive == "true";
        }
        catch
        {
            return false;
        }
    }

    public async Task<UserInfo> GetUserInfoFromTokenAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var userId = jwtToken.Claims.FirstOrDefault(x => x.Type == "nameid")?.Value;
            var username = jwtToken.Claims.FirstOrDefault(x => x.Type == "unique_name")?.Value;
            var role = jwtToken.Claims.FirstOrDefault(x => x.Type == "role")?.Value;
            var email = jwtToken.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
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

    public async Task<string> LoginAsync(string username, string password)
    {
        var httClient = _httpClientFactory.CreateClient();
        var response = await httClient.PostAsJsonAsync($"{_authApiUrl}/api/auth/login", new
        {
            Username = username,
            Password = password
        });

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return tokenResponse?.Token;
        }

        return null;
    }

    public async Task<bool> RegisterAsync(string username, string email, string cpf, string password, string phone, string role = "User")
    {
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.PostAsJsonAsync($"{_authApiUrl}/api/auth/register", new
        {
            Username = username,
            Email = email,
            CPF = cpf,
            Phone = phone,
            Role = role,
            Active = true
        });

        return response.IsSuccessStatusCode;
    }

    public async Task<UserInfo[]> GetAllUsersAsync(string token)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.GetAsync($"{_authApiUrl}/api/auth/users");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<UserInfo[]>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return users;
        }

        return Array.Empty<UserInfo>();
    }
}

// Classes para deserialização de eventos
public class UserInfo
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string CPF { get; set; }
    public string Phone { get; set; }
    public string Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
    public bool Active { get; set; }
}

public class TokenResponse
{
    public string Token { get; set; }
    public DateTime Expiration { get; set; }
    public string Username { get; set; }
    public string CPF { get; set; }
    public string Phone { get; set; }
    public string Role { get; set; }
}