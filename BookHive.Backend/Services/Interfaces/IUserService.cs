using BookHive.Models;

namespace BookHive.Services;

public interface IUserService
{
    Task<User> RegisterAsync(string username, string email, string password);
    Task<User?> LoginAsync(string emailOrUsername, string password);
    Task<User?> GetByIdAsync(int userId);
    Task<User?> GetByUsernameAsync(string username);
    Task<List<User>> SearchAsync(string query, int limit = 20);
    Task<User?> UpdateProfileAsync(int userId, string? displayName, string? bio, string? avatarUrl);
}