using BookHive.Models;

namespace BookHive.Backend.Repositories;

public interface IUserRepository
{
    bool UsernameExists(string username);
    bool EmailExists(string email);
    User? FindByEmailOrUsername(string emailOrUsername);
    void Add(User user);
    void Save();
}
