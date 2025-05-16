namespace Auth.API.Models;

public class AuthResult
{
    public bool Success { get; set; }
    public string Token { get; set; }
    public string Message { get; set; }
    public UserDto User { get; set; }
}

public class UserDto
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
}