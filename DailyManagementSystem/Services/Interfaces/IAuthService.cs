using System.Threading.Tasks;
using DailyManagementSystem.Models;

namespace DailyManagementSystem.Services.Interfaces
{
    public interface IAuthService
    {
        User? CurrentUser { get; }
        bool IsLoggedIn => CurrentUser != null;
        bool IsAdmin => CurrentUser?.Role == UserRole.Admin;

        Task<bool> LoginAsync(string username, string password);
        void Logout();
        
        // Initial setup helper
        Task EnsureAdminUserExistsAsync();
    }
}
