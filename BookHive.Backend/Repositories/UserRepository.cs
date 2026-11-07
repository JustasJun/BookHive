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

    public async Task<User?> FindByIdAsync(int userId)
    {
        return await _context.Users.FirstOrDefaultAsync(user =>
            user.Id == userId);
    }

    public async Task<User?> FindByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(user =>
            user.Username.ToLower() == username.ToLower());
    }

    public async Task<List<User>> SearchAsync(string query, int limit)
    {
        string normalizedQuery = query.ToLower();

        return await _context.Users
            .Where(user => user.Username.ToLower().Contains(normalizedQuery))
            .OrderBy(user => user.Username)
            .Take(limit)
            .ToListAsync();
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