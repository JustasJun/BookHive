using BookHive.Models;

namespace BookHive.Backend.Repositories;

public interface IUserRepository
{
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
    Task<User?> FindByEmailOrUsernameAsync(string emailOrUsername);
    void Add(User user);
    Task SaveAsync();
}