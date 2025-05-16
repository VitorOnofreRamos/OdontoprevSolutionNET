using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Challenge_Odontoprev_API.Services;

public interface IUserValidationService
{
    Task<bool> IsUserActiveAsync(string userId, string token);
}

public class UserValidationService : IUserValidationService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public UserValidationService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["OdontoprevAuthApi:BaseUrl"];
    }

    public async Task<bool> IsUserActiveAsync(string userId, string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.GetAsync($"{_baseUrl}/users/{userId}");
        if (!response.IsSuccessStatusCode)
            return false;

        var content = await response.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<UserDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return user?.Active ?? false;
    }
}

public class UserDto
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public bool Active { get; set; }
}
