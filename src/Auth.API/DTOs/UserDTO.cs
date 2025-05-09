namespace Auth.API.DTOs;

public class UserRegistrationDTO
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; } = "User"; //Default role
}

public class UserLoginDTO
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class UserTokenDTO
{
    public string Token { get; set; }
    public DateTime Expiration { get; set; }
    public string Username { get; set; }
    public string Role { get; set; }
}
