namespace Auth.API.DTOs;

public class UserRegistrationDTO
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string CPF { get; set; }
    public string Password { get; set; }
    public string Phone { get; set; }
    public string Role { get; set; } = "User"; //Default role
    public bool Active { get; set; } = true;
}

public class UserUpdateDTO
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string CPF { get; set; }
    public string Phone { get; set; }
    public string Role { get; set; }
    public bool Active { get; set; }
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
    public string Email { get; set; }
    public string CPF { get; set; }
    public string Phone { get; set; }
    public string Role { get; set; }
}

public class UserProfileDTO
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string CPF { get; set; }
    public string Phone { get; set; }
    public string Role { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
    public bool Active { get; set; }
}