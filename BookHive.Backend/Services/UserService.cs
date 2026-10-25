using BookHive.Models;
using BookHive.Backend.Repositories;
using Microsoft.AspNetCore.Identity;

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

    public User Register(string username, string email, string password)
    {
        if (_userRepository.UsernameExists(username))
        {
            throw new InvalidOperationException("Username is already taken.");
        }

        if (_userRepository.EmailExists(email))
        {
            throw new InvalidOperationException("Email address is already registered.");
        }

        var user = new User(username, email, "");

        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        _userRepository.Add(user);
        _userRepository.Save();

        return user;
    }

    public User? Login(string emailOrUsername, string password)
    {
        User? user = _userRepository.FindByEmailOrUsername(emailOrUsername);
        if (user == null)
        {
            return null;
        }

        PasswordVerificationResult result =
            _passwordHasher.VerifyHashedPassword(
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
