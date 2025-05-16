namespace Auth.API.Models;

public class UpdateUserModel
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string CPF { get; set; }
    public string Phone { get; set; }
    public string Role { get; set; }
    public string Password { get; set; } // Optional for update
    public bool Active { get; set; }
}
