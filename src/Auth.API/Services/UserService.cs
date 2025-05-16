using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Auth.API.Configuration;
using Auth.API.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Auth.API.Services;

public interface IUserService
{
    Task<List<User>> GetAllUsersAsync();
    Task<User> GetUserByIdAsync(string id);
    Task<User> GetUserByEmailAsync(string email);
    Task<User> RegisterUserAsync(RegisterModel model);
    Task<User> AuthenticateAsync(string email, string password);
    Task<User> UpdateUserAsync(string id, UpdateUserModel model);
    Task<bool> DeleteUserAsync(string id);
}

public class UserService : IUserService
{
    private readonly IMongoCollection<User> _users;

    public UserService(IOptions<MongoDbSettings> mongoDbSettings)
    {
        var client = new MongoClient(mongoDbSettings.Value.ConnectionString);
        var database = client.GetDatabase(mongoDbSettings.Value.DatabaseName);
        _users = database.GetCollection<User>(mongoDbSettings.Value.UsersCollectionName);
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _users.Find(user => true).ToListAsync();
    }

    public async Task<User> GetUserByIdAsync(string id)
    {
        return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
        return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<User> RegisterUserAsync(RegisterModel model)
    {
        // Check if user with same email exists
        var existingUser = await _users.Find(u => u.Email == model.Email).FirstOrDefaultAsync();
        if (existingUser != null)
        {
            return null;
        }

        // Create password hash and salt
        CreatePasswordHash(model.Password, out byte[] passwordHash, out byte[] passwordSalt);

        var user = new User
        {
            Username = model.Username,
            Email = model.Email,
            CPF = model.CPF,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            Phone = model.Phone,
            Role = model.Role,
            CreatedAt = DateTime.UtcNow,
            LastLogin = null,
            Active = true
        };

        await _users.InsertOneAsync(user);
        return user;
    }

    public async Task<User> AuthenticateAsync(string email, string password)
    {
        var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();

        // Check if user exists and is active
        if (user == null || !user.Active)
        {
            return null;
        }

        // Verify password
        if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
        {
            return null;
        }

        // Update last login
        var update = Builders<User>.Update.Set(u => u.LastLogin, DateTime.UtcNow);
        await _users.UpdateOneAsync(u => u.Id == user.Id, update);

        return user;
    }

    public async Task<User> UpdateUserAsync(string id, UpdateUserModel model)
    {
        var user = await GetUserByIdAsync(id);
        if (user == null)
        {
            return null;
        }

        var update = Builders<User>.Update
            .Set(u => u.Username, model.Username)
            .Set(u => u.Email, model.Email)
            .Set(u => u.CPF, model.CPF)
            .Set(u => u.Phone, model.Phone)
            .Set(u => u.Role, model.Role)
            .Set(u => u.Active, model.Active);

        // Update password if provided
        if (!string.IsNullOrEmpty(model.Password))
        {
            CreatePasswordHash(model.Password, out byte[] passwordHash, out byte[] passwordSalt);
            update = update
                .Set(u => u.PasswordHash, passwordHash)
                .Set(u => u.PasswordSalt, passwordSalt);
        }

        await _users.UpdateOneAsync(u => u.Id == id, update);
        return await GetUserByIdAsync(id);
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var result = await _users.DeleteOneAsync(u => u.Id == id);
        return result.DeletedCount > 0;
    }

    // Helper methods for password hashing
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
                if (computedHash[i] != storedHash[i]) return false;
            }
        }
        return true;
    }
}
