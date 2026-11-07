using BookHive.Backend.Data;
using BookHive.Exceptions;
using BookHive.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
        username = username.ToLowerInvariant();
        return await _context.Users.AnyAsync(user =>
            user.Username == username);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        email = email.ToLowerInvariant();
        return await _context.Users.AnyAsync(user =>
            user.Email == email);
    }

    public async Task<User?> FindByEmailOrUsernameAsync(string emailOrUsername)
    {
        emailOrUsername = emailOrUsername.ToLowerInvariant();
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
        username = username.ToLowerInvariant();
        return await _context.Users.FirstOrDefaultAsync(user =>
            user.Username == username);
    }

    public async Task<List<User>> SearchAsync(string query, int limit)
    {
        string normalizedQuery = query.ToLowerInvariant();

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
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException postgresException &&
            postgresException.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            if (postgresException.ConstraintName == "IX_Users_Username")
            {
                throw new DuplicateUserException("Username is already taken.");
            }

            if (postgresException.ConstraintName == "IX_Users_Email")
            {
                throw new DuplicateUserException("Email address is already registered.");
            }

            throw;
        }
    }

    public async Task<User?> FindByIdAsync(int userId)
    {
        return await _context.Users.FirstOrDefaultAsync(user =>
            user.Id == userId);
    }
}