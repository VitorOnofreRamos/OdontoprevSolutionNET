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
        _authApiUrl = _configuration["AuthSettings:ApiUrl"] ?? "http://auth-api";
    }

    public async Task<string> RegisterUserAsync(string username, string email, string cpf, string password, string phone, string role = "User")
    {
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.PostAsJsonAsync($"{_authApiUrl}/api/auth/create-user", new
        {
            Username = username,
            Email = email,
            CPF = cpf,
            Password = password,
            Phone = phone,
            Role = role,
            Active = true
        });

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var createUserResponse = JsonSerializer.Deserialize<CreateUserResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return createUserResponse?.UserId;
        }

        return null;
    }

    public async Task<string> AuthenticateAsync(string username, string password)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.PostAsJsonAsync($"{_authApiUrl}/api/auth/authenticate", new
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

    public async Task<bool> ValidateTokenAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
            return false;

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.PostAsJsonAsync($"{_authApiUrl}/api/auth/validate", new
            {
                Token = token
            });

            return response.IsSuccessStatusCode;
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
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.PostAsJsonAsync($"{_authApiUrl}/api/auth/validate", new
            {
                Token = token
            });

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<UserInfo>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return userInfo;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<string> RefreshTokenAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.PostAsJsonAsync($"{_authApiUrl}/api/auth/refresh", new
            {
                Token = token
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
        catch
        {
            return null;
        }
    }
}

// Classes para deserialização de respostas
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

public class TokenResponse
{
    public string Token { get; set; }
    public DateTime Expiration { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string CPF { get; set; }
    public string Phone { get; set; }
    public string Role { get; set; }
}

public class CreateUserResponse
{
    public string UserId { get; set; }
}
