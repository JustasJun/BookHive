using BookHive.Models;
using BookHive.Backend.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using BookHive.Exceptions;

namespace BookHive.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<User> RegisterAsync(string username, string email, string password)
    {
        username = username.Trim().ToLowerInvariant();
        email = email.Trim().ToLowerInvariant();

        if (await _userRepository.UsernameExistsAsync(username))
        {
            throw new DuplicateUserException("Username is already taken.");
        }

        if (await _userRepository.EmailExistsAsync(email))
        {
            throw new DuplicateUserException("Email address is already registered.");
        }

        var user = new User(username, email, "");

        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        _userRepository.Add(user);
        try
        {
            await _userRepository.SaveAsync();
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

        return user;
    }

    public async Task<User?> LoginAsync(string emailOrUsername, string password)
    {
        emailOrUsername = emailOrUsername.Trim().ToLowerInvariant();
        User? user = await _userRepository.FindByEmailOrUsernameAsync(emailOrUsername);

        if (user == null)
        {
            return null;
        }

        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            password);

        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return user;
    }

    public Task<User?> GetByIdAsync(int userId)
    {
        return _userRepository.FindByIdAsync(userId);
    }

    public Task<User?> GetByUsernameAsync(string username)
    {
        return _userRepository.FindByUsernameAsync(username);
    }

    public Task<List<User>> SearchAsync(string query, int limit = 20)
    {
        return _userRepository.SearchAsync(query, limit);
    }
}
