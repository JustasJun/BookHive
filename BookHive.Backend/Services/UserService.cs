using BookHive.Models;
using BookHive.Backend.Repositories;
using Microsoft.AspNetCore.Identity;
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
        await _userRepository.SaveAsync();

        return user;
    }

    public async Task<User?> LoginAsync(string emailOrUsername, string password)
    {
        var user = await _userRepository.FindByEmailOrUsernameAsync(emailOrUsername);

        if (user == null)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(
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
