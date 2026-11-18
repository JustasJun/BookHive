using BookHive.Backend.Repositories;
using BookHive.Exceptions;
using BookHive.Models;
using BookHive.Services;
using Microsoft.AspNetCore.Identity;

namespace BookHive.Tests.UnitTests.Services;

public class UserServiceTests
{
    private readonly IUserRepository _repositorySubstitute = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher<User> _hasherSubstitute = Substitute.For<IPasswordHasher<User>>();
    private readonly UserService _service;

    public UserServiceTests()
    {
        _service = new UserService(_repositorySubstitute, _hasherSubstitute);
    }

    [Fact]
    public async Task RegisterAsync_NormalizesInputHashesPasswordAndSaves()
    {
        _hasherSubstitute.HashPassword(Arg.Any<User>(), "Secret1!").Returns("hashed-secret");

        User user = await _service.RegisterAsync(
            "  Alice ", " Alice@Example.COM ", "Secret1!");

        Assert.Equal("alice", user.Username);
        Assert.Equal("alice@example.com", user.Email);
        Assert.Equal("hashed-secret", user.PasswordHash);
        _repositorySubstitute.Received(1).Add(user);
        await _repositorySubstitute.Received(1).SaveAsync();
    }

    [Fact]
    public async Task RegisterAsync_WhenUsernameIsTaken_ThrowsBookHiveException()
    {
        _repositorySubstitute.UsernameExistsAsync("alice").Returns(true);

        var exception = await Assert.ThrowsAsync<BookHiveException>(() =>
            _service.RegisterAsync("Alice", "alice@example.com", "Secret1!"));

        Assert.Equal("Username is already taken.", exception.Message);
        _repositorySubstitute.DidNotReceive().Add(Arg.Any<User>());
        await _repositorySubstitute.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailIsTaken_ThrowsBookHiveException()
    {
        _repositorySubstitute.EmailExistsAsync("alice@example.com").Returns(true);

        var exception = await Assert.ThrowsAsync<BookHiveException>(() =>
            _service.RegisterAsync("alice", "Alice@Example.com", "Secret1!"));

        Assert.Equal("Email address is already registered.", exception.Message);
        _repositorySubstitute.DidNotReceive().Add(Arg.Any<User>());
        await _repositorySubstitute.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ReturnsNullWithoutCheckingPassword()
    {
        _repositorySubstitute.FindByEmailOrUsernameAsync("alice").Returns((User?)null);

        User? user = await _service.LoginAsync("alice", "Secret1!");

        Assert.Null(user);
        _hasherSubstitute.DidNotReceive().VerifyHashedPassword(
            Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsWrong_ReturnsNull()
    {
        var existing = new User("alice", "alice@example.com", "hash");
        _repositorySubstitute.FindByEmailOrUsernameAsync("alice").Returns(existing);
        _hasherSubstitute.VerifyHashedPassword(existing, "hash", "wrong")
            .Returns(PasswordVerificationResult.Failed);

        User? user = await _service.LoginAsync("alice", "wrong");

        Assert.Null(user);
    }

    [Theory]
    [InlineData(PasswordVerificationResult.Success)]
    [InlineData(PasswordVerificationResult.SuccessRehashNeeded)]
    public async Task LoginAsync_WhenPasswordIsCorrect_ReturnsUser(
        PasswordVerificationResult verificationResult)
    {
        var existing = new User("alice", "alice@example.com", "hash");
        _repositorySubstitute.FindByEmailOrUsernameAsync("alice").Returns(existing);
        _hasherSubstitute.VerifyHashedPassword(existing, "hash", "Secret1!")
            .Returns(verificationResult);

        User? user = await _service.LoginAsync("  ALICE ", "Secret1!");

        Assert.Same(existing, user);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsUserFromRepository()
    {
        var existing = new User("alice", "alice@example.com", "hash");
        _repositorySubstitute.FindByIdAsync(7).Returns(existing);

        Assert.Same(existing, await _service.GetByIdAsync(7));
    }

    [Fact]
    public async Task GetByUsernameAsync_ReturnsUserFromRepository()
    {
        var existing = new User("alice", "alice@example.com", "hash");
        _repositorySubstitute.FindByUsernameAsync("alice").Returns(existing);

        Assert.Same(existing, await _service.GetByUsernameAsync("alice"));
    }

    [Fact]
    public async Task SearchAsync_UsesDefaultLimitOfTwenty()
    {
        var found = new List<User> { new("alice", "alice@example.com", "hash") };
        _repositorySubstitute.SearchAsync("al", 20).Returns(found);

        List<User> result = await _service.SearchAsync("al");

        Assert.Same(found, result);
    }
}
