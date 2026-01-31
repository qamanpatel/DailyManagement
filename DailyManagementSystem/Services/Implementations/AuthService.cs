using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DailyManagementSystem.Data;
using DailyManagementSystem.Models;
using DailyManagementSystem.Services.Interfaces;
using BCrypt.Net;

namespace DailyManagementSystem.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        public User? CurrentUser { get; private set; }

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

            if (user == null) return false;

            if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                CurrentUser = user;
                return true;
            }

            return false;
        }

        public void Logout()
        {
            CurrentUser = null;
        }

        public async Task EnsureAdminUserExistsAsync()
        {
            if (!await _context.Users.AnyAsync(u => u.Role == UserRole.Admin))
            {
                var admin = new User
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = UserRole.Admin
                };
                _context.Users.Add(admin);
            }

            if (!await _context.Users.AnyAsync(u => u.Role == UserRole.User))
            {
                var user = new User
                {
                    Username = "user",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"),
                    Role = UserRole.User
                };
                _context.Users.Add(user);
            }

            if (_context.ChangeTracker.HasChanges())
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}
