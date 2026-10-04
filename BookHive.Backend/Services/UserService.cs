using BookHive.Backend.Data;
using BookHive.Models;
using Microsoft.AspNetCore.Identity;

namespace BookHive.Services;

public class UserService
{
    private readonly BookHiveDbContext _context;

    public UserService(BookHiveDbContext context)
    {
        _context = context;
    }

    public User Register(string username, string email, string password)
    {
        if (_context.Users.Any(u => u.Username == username))
        {
            throw new InvalidOperationException("Username is already taken.");
        }

        if (_context.Users.Any(u => u.Email == email))
        {
            throw new InvalidOperationException("Email address is already registered.");
        }

        User user = new User(username, email, "");

        PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(user, password);

        _context.Users.Add(user);
        _context.SaveChanges();

        return user;
    }

    public User? Login(string emailOrUsername, string password)
    {
        User? user = _context.Users
            .FirstOrDefault(user => user.Email == emailOrUsername || user.Username == emailOrUsername);

        if (user == null)
        {
            return null;
        }

        PasswordHasher<User> passwordHasher = new PasswordHasher<User>();

        PasswordVerificationResult result =
            passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                password);

        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return user;
    }
}