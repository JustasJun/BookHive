using BookHive.Models;

namespace BookHive.Backend.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
    Task<User?> FindByEmailOrUsernameAsync(string emailOrUsername);
    Task<User?> FindByIdAsync(int userId);
    Task<User?> FindByUsernameAsync(string username);
    Task<List<User>> SearchAsync(string query, int limit);
}
