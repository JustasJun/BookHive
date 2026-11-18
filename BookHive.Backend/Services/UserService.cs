using BookHive.Models;
using BookHive.Backend.Repositories;
using Microsoft.AspNetCore.Identity;
using BookHive.Exceptions;

namespace BookHive.Services;

public class UserService : IUserService
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
            throw new BookHiveException("Username is already taken.");
        }

        if (await _userRepository.EmailExistsAsync(email))
        {
            throw new BookHiveException("Email address is already registered.");
        }

        var user = new User(username, email, "");

        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        _userRepository.Add(user);
        await _userRepository.SaveAsync();

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

    public async Task<User?> UpdateProfileAsync(
        int userId,
        string? displayName,
        string? bio,
        string? avatarUrl)
    {
        User? user = await _userRepository.FindByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        user.DisplayName = NormalizeOptionalField(displayName);
        user.Bio = NormalizeOptionalField(bio);
        user.AvatarUrl = NormalizeOptionalField(avatarUrl);
        await _userRepository.SaveAsync();

        return user;
    }

    public Task<List<User>> SearchAsync(string query, int limit = 20)
    {
        return _userRepository.SearchAsync(query, limit);
    }

    private static string? NormalizeOptionalField(string? value)
    {
        string? trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
