using BookHive.Backend.Data;
using BookHive.Exceptions;
using BookHive.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BookHive.Backend.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(BookHiveDbContext context) : base(context)
    {
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        username = username.ToLowerInvariant();
        return await ExistsAsync(user => user.Username == username);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        email = email.ToLowerInvariant();
        return await ExistsAsync(user => user.Email == email);
    }

    public async Task<User?> FindByEmailOrUsernameAsync(string emailOrUsername)
    {
        emailOrUsername = emailOrUsername.ToLowerInvariant();
        return await FindFirstAsync(user =>
            user.Email == emailOrUsername ||
            user.Username == emailOrUsername);
    }

    public async Task<User?> FindByIdAsync(int userId)
    {
        return await FindFirstAsync(user => user.Id == userId);
    }

    public async Task<User?> FindByUsernameAsync(string username)
    {
        username = username.ToLowerInvariant();
        return await FindFirstAsync(user => user.Username == username);
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

    public override async Task SaveAsync()
    {
        try
        {
            await base.SaveAsync();
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
}