using BookHive.Backend.Data;
using BookHive.Models;
using Microsoft.EntityFrameworkCore;

namespace BookHive.Backend.Repositories;

public class UserRepository : IUserRepository
{
    private readonly BookHiveDbContext _context;

    public UserRepository(BookHiveDbContext context)
    {
        _context = context;
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _context.Users.AnyAsync(user =>
            user.Username == username);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(user =>
            user.Email == email);
    }

    public async Task<User?> FindByEmailOrUsernameAsync(string emailOrUsername)
    {
        return await _context.Users.FirstOrDefaultAsync(user =>
            user.Email == emailOrUsername ||
            user.Username == emailOrUsername);
    }

    public void Add(User user)
    {
        _context.Users.Add(user);
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}