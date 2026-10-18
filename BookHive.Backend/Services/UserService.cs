using BookHive.Models;
using BookHive.Backend.Repositories;
using Microsoft.AspNetCore.Identity;

namespace BookHive.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
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

        var passwordHasher = new PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(user, password);

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

        var passwordHasher = new PasswordHasher<User>();

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
