using Auth.API.Data;
using Auth.API.DTOs;
using Auth.API.Models;
using MongoDB.Driver;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Auth.API.Services;

public class AuthService
{
    private readonly MongoDBContext _context;
    private readonly TokenService _tokenService;
    private readonly RabbitMQService _rabbitMQService;

    public AuthService(MongoDBContext context, TokenService tokenService, RabbitMQService rabbitMQService)
    {
        _context = context;
        _tokenService = tokenService;
        _rabbitMQService = rabbitMQService;
    }

    public async Task<UserTokenDTO> Register(UserRegistrationDTO userDTO)
    {
        // Check if user already exists
        var existingUser = await _context.Users
            .Find(u => u.Username == userDTO.Username || u.Email == userDTO.Email)
            .FirstOrDefaultAsync();

        if (existingUser != null) 
        {
            throw new Exception("Username or email already in use");
        }

        // Create password hash
        CreatePasswordHash(userDTO.Password, out byte[] passwordHash, out byte[] passwordSalt);

        var user = new User
        {
            Username = userDTO.Username,
            Email = userDTO.Email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            Role = userDTO.Role,
            CreatedAt = DateTime.UtcNow,
        };

        // Save to MongoDB
        await _context.Users.InsertOneAsync(user);

        // Publish event to RabbitMQ
        await _rabbitMQService.PublishUserCreatedEventAsync(user);

        // Generate token
        var token = _tokenService.GenerateToken(user);
        return token;
    }

    public async Task<UserTokenDTO> Login(UserLoginDTO userDto)
    {
        // Find user
        var user = await _context.Users
            .Find(u => u.Username == userDto.Username)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            throw new Exception("User not found");
        }

        // Verify password
        if (!VerifyPasswordHash(userDto.Password, user.PasswordHash, user.PasswordSalt))
        {
            throw new Exception("Invalid password");
        }

        // Update last login
        user.LastLogin = DateTime.UtcNow;
        await _context.Users.ReplaceOneAsync(u => u.Id == user.Id, user);

        // Publish login event
        await _rabbitMQService.PublishUserLoggedInEventAsync(user);

        // Generate token
        var token = _tokenService.GenerateToken(user);
        return token;
    }

    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }

    private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
    {
        using (var hmac = new HMACSHA512(storedSalt))
        {
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != storedHash[i])
                    return false;
            }
        }
        return true;
    }
}
