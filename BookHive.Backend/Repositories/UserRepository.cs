using BookHive.Backend.Data;
using BookHive.Models;

namespace BookHive.Backend.Repositories;

public class UserRepository : IUserRepository
{
    private readonly BookHiveDbContext _context;

    public UserRepository(BookHiveDbContext context)
    {
        _context = context;
    }

    public bool UsernameExists(string username)
    {
        return _context.Users.Any(user => user.Username == username);
    }

    public bool EmailExists(string email)
    {
        return _context.Users.Any(user => user.Email == email);
    }

    public User? FindByEmailOrUsername(string emailOrUsername)
    {
        return _context.Users.FirstOrDefault(user =>
            user.Email == emailOrUsername || user.Username == emailOrUsername);
    }

    public void Add(User user)
    {
        _context.Users.Add(user);
    }

    public void Save()
    {
        _context.SaveChanges();
    }
}