//Services/UserService.cs
using Auth.API.Data;
using Auth.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Auth.API.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(DatabaseSettings settings)
        {
            _context = context;
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> UpdateAsync(int id, User user)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                return null;

            existingUser.Name = user.Name ?? existingUser.Name;
            existingUser.Email = user.Email ?? existingUser.Email;
            existingUser.Phone = user.Phone ?? existingUser.Phone;
            existingUser.Role = user.Role ?? existingUser.Role;
            existingUser.IsActive = user.IsActive ?? existingUser.IsActive;
            existingUser.OrganizationId = user.OrganizationId ?? existingUser.OrganizationId;
            existingUser.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                existingUser.PasswordHash = user.PasswordHash;
            }

            if (user.LastLogin.HasValue)
            {
                existingUser.LastLogin = user.LastLogin;
            }

            await _context.SaveChangesAsync();
            return existingUser;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            user.IsActive = "N";
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
    }
}