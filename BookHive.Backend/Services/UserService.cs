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
        User user = new User(username, email, "");

        PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(user, password);

        _context.Users.Add(user);
        _context.SaveChanges();

        return user;
    }
}