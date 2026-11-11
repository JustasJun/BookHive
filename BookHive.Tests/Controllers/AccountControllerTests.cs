using System.IdentityModel.Tokens.Jwt;
using BookHive.Backend.Controllers;
using BookHive.Backend.DTOs;
using BookHive.Backend.Repositories;
using BookHive.Exceptions;
using BookHive.Models;
using BookHive.Services;
using BookHive.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BookHive.Tests.Controllers;

public class AccountControllerTests
{
    private const int UserId = 1;

    private readonly IUserRepository _userRepositorySubstitute =
        Substitute.For<IUserRepository>();

    private readonly ILibraryRepository _libraryRepositorySubstitute =
        Substitute.For<ILibraryRepository>();

    private readonly IReviewRepository _reviewRepositorySubstitute =
        Substitute.For<IReviewRepository>();

    private readonly IFollowRepository _followRepositorySubstitute =
        Substitute.For<IFollowRepository>();

    private readonly IPasswordHasher<User> _passwordHasherSubstitute =
        Substitute.For<IPasswordHasher<User>>();

    private AccountController CreateController(int? userId = UserId)
    {
        var userService = new UserService(
            _userRepositorySubstitute,
            _passwordHasherSubstitute);

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-key-test-key-test-key-test-key-1234",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience"
            })
            .Build();

        var jwtTokenService = new JwtTokenService(configuration);

        var libraryService = new LibraryService(
            _libraryRepositorySubstitute,
            new OpenLibraryClient(new HttpClient()));

        var reviewService = new ReviewService(
            _reviewRepositorySubstitute);

        var followService = new FollowService(
            _followRepositorySubstitute);

        return new AccountController(
            userService,
            jwtTokenService,
            libraryService,
            reviewService,
            followService).WithUser(userId);
    }

    private static User CreateUser()
    {
        return new User(
            "alice",
            "alice@example.com",
            "hashed-password")
        {
            Id = UserId
        };
    }

    [Fact]
    public async Task Register_WhenDataIsValid_ReturnsUserAndToken()
    {
        _passwordHasherSubstitute
            .HashPassword(Arg.Any<User>(), "Secret1!")
            .Returns("hashed-password");

        var controller = CreateController();

        var request = new RegisterRequest(
            "Alice",
            "Alice@Example.com",
            "Secret1!");

        var result = await controller.Register(request);

        var ok = Assert.IsType<OkObjectResult>(result);

        Assert.NotNull(ok.Value);

        var value = ok.Value;
        var type = value.GetType();

        Assert.Equal(
            "alice",
            type.GetProperty("Username")!.GetValue(value));

        Assert.Equal(
            "alice@example.com",
            type.GetProperty("Email")!.GetValue(value));

        var token = Assert.IsType<string>(
            type.GetProperty("Token")!.GetValue(value));

        Assert.False(string.IsNullOrWhiteSpace(token));

        _userRepositorySubstitute
            .Received(1)
            .Add(Arg.Is<User>(u =>
                u.Username == "alice" &&
                u.Email == "alice@example.com" &&
                u.PasswordHash == "hashed-password"));

        await _userRepositorySubstitute
            .Received(1)
            .SaveAsync();
    }

    [Fact]
    public async Task Register_WhenUsernameAlreadyExists_ReturnsConflict()
    {
        _userRepositorySubstitute
            .UsernameExistsAsync("alice")
            .Returns(true);

        var controller = CreateController();

        var request = new RegisterRequest(
            "Alice",
            "alice@example.com",
            "Secret1!");

        var result = await controller.Register(request);

        Assert.IsType<ConflictObjectResult>(result);

        _userRepositorySubstitute
            .DidNotReceive()
            .Add(Arg.Any<User>());

        await _userRepositorySubstitute
            .DidNotReceive()
            .SaveAsync();
    }

    [Fact]
    public async Task Register_WhenEmailAlreadyExists_ReturnsConflict()
    {
        _userRepositorySubstitute
            .EmailExistsAsync("alice@example.com")
            .Returns(true);

        var controller = CreateController();

        var request = new RegisterRequest(
            "alice",
            "Alice@Example.com",
            "Secret1!");

        var result = await controller.Register(request);

        Assert.IsType<ConflictObjectResult>(result);

        _userRepositorySubstitute
            .DidNotReceive()
            .Add(Arg.Any<User>());

        await _userRepositorySubstitute
            .DidNotReceive()
            .SaveAsync();
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValid_ReturnsUserAndToken()
    {
        var user = CreateUser();

        _userRepositorySubstitute
            .FindByEmailOrUsernameAsync("alice")
            .Returns(user);

        _passwordHasherSubstitute
            .VerifyHashedPassword(
                user,
                user.PasswordHash,
                "Secret1!")
            .Returns(PasswordVerificationResult.Success);

        var controller = CreateController();

        var request = new LoginRequest(
            "Alice",
            "Secret1!");

        var result = await controller.Login(request);

        var ok = Assert.IsType<OkObjectResult>(result);

        Assert.NotNull(ok.Value);

        var value = ok.Value;
        var type = value.GetType();

        Assert.Equal(
            UserId,
            type.GetProperty("Id")!.GetValue(value));

        Assert.Equal(
            "alice",
            type.GetProperty("Username")!.GetValue(value));

        var token = Assert.IsType<string>(
            type.GetProperty("Token")!.GetValue(value));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal(UserId.ToString(), jwt.Subject);

        await _userRepositorySubstitute
            .Received(1)
            .FindByEmailOrUsernameAsync("alice");
    }

    [Fact]
    public async Task Login_WhenUserDoesNotExist_ReturnsUnauthorized()
    {
        _userRepositorySubstitute
            .FindByEmailOrUsernameAsync("unknown")
            .Returns((User?)null);

        var controller = CreateController();

        var request = new LoginRequest(
            "unknown",
            "Secret1!");

        var result = await controller.Login(request);

        Assert.IsType<UnauthorizedResult>(result);

        _passwordHasherSubstitute
            .DidNotReceive()
            .VerifyHashedPassword(
                Arg.Any<User>(),
                Arg.Any<string>(),
                Arg.Any<string>());
    }

    [Fact]
    public async Task Login_WhenPasswordIsIncorrect_ReturnsUnauthorized()
    {
        var user = CreateUser();

        _userRepositorySubstitute
            .FindByEmailOrUsernameAsync("alice")
            .Returns(user);

        _passwordHasherSubstitute
            .VerifyHashedPassword(
                user,
                user.PasswordHash,
                "wrong-password")
            .Returns(PasswordVerificationResult.Failed);

        var controller = CreateController();

        var request = new LoginRequest(
            "alice",
            "wrong-password");

        var result = await controller.Login(request);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetCurrentUser_WhenNotLoggedIn_ReturnsUnauthorized()
    {
        var controller = CreateController(userId: null);

        var result = await controller.GetCurrentUser();

        Assert.IsType<UnauthorizedResult>(result.Result);

        await _userRepositorySubstitute
            .DidNotReceive()
            .FindByIdAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task GetCurrentUser_WhenUserDoesNotExist_ReturnsNotFound()
    {
        _userRepositorySubstitute
            .FindByIdAsync(UserId)
            .Returns((User?)null);

        var controller = CreateController();

        var result = await controller.GetCurrentUser();

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetCurrentUser_WhenUserExists_ReturnsProfile()
    {
        var user = CreateUser();

        user.DisplayName = "Alice Smith";
        user.Bio = "I love books";

        _userRepositorySubstitute
            .FindByIdAsync(UserId)
            .Returns(user);

        var controller = CreateController();

        var result = await controller.GetCurrentUser();

        var ok = Assert.IsType<OkObjectResult>(result.Result);

        var profile = Assert.IsType<UserProfileResponse>(ok.Value);

        Assert.Equal(UserId, profile.Id);
        Assert.Equal("alice", profile.Username);
        Assert.Equal("Alice Smith", profile.DisplayName);
        Assert.Equal("I love books", profile.Bio);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task SearchUsers_WhenQueryIsEmpty_ReturnsBadRequest(
        string query)
    {
        var controller = CreateController();

        var result = await controller.SearchUsers(query);

        Assert.IsType<BadRequestObjectResult>(result.Result);

        await _userRepositorySubstitute
            .DidNotReceive()
            .SearchAsync(Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task SearchUsers_WhenQueryIsValid_ReturnsMatchingUsers()
    {
        var users = new List<User>
        {
            CreateUser(),
            new User(
                "alice2",
                "alice2@example.com",
                "hashed-password")
            {
                Id = 2
            }
        };

        _userRepositorySubstitute
            .SearchAsync("alice", 20)
            .Returns(users);

        var controller = CreateController();

        var result = await controller.SearchUsers("  alice  ");

        var ok = Assert.IsType<OkObjectResult>(result.Result);

        var profiles = Assert.IsType<List<UserSummaryResponse>>(
            ok.Value);

        Assert.Equal(2, profiles.Count);

        Assert.Contains(profiles, u => u.Username == "alice");
        Assert.Contains(profiles, u => u.Username == "alice2");

        await _userRepositorySubstitute
            .Received(1)
            .SearchAsync("alice", 20);
    }

    [Fact]
    public async Task SearchUsers_WhenNoUsersMatch_ReturnsEmptyList()
    {
        _userRepositorySubstitute
            .SearchAsync("unknown", 20)
            .Returns(new List<User>());

        var controller = CreateController();

        var result = await controller.SearchUsers("unknown");

        var ok = Assert.IsType<OkObjectResult>(result.Result);

        var profiles = Assert.IsType<List<UserSummaryResponse>>(
            ok.Value);

        Assert.Empty(profiles);
    }

    [Fact]
    public async Task GetUserProfile_WhenUserDoesNotExist_ReturnsNotFound()
    {
        _userRepositorySubstitute
            .FindByUsernameAsync("unknown")
            .Returns((User?)null);

        var controller = CreateController();

        var result = await controller.GetUserProfile("unknown");

        Assert.IsType<NotFoundResult>(result.Result);

        await _libraryRepositorySubstitute
            .DidNotReceive()
            .GetBooksAsync(Arg.Any<int>());

        await _reviewRepositorySubstitute
            .DidNotReceive()
            .GetByUserAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task GetUserProfile_WhenUserExists_ReturnsAccountDetails()
    {
        var user = CreateUser();

        _userRepositorySubstitute
            .FindByUsernameAsync("alice")
            .Returns(user);

        _followRepositorySubstitute
            .UserExistsAsync(UserId)
            .Returns(true);

        _followRepositorySubstitute
            .CountFollowersAsync(UserId)
            .Returns(5);

        _followRepositorySubstitute
            .CountFollowingAsync(UserId)
            .Returns(3);

        _reviewRepositorySubstitute
            .GetByUserAsync(UserId)
            .Returns(new List<Review>());

        _libraryRepositorySubstitute
            .GetBooksAsync(UserId)
            .Returns(new List<LibraryBook>
            {
                new LibraryBook(
                    UserId,
                    "/works/OL123W",
                    ReadingStatus.Read),
                new LibraryBook(
                    UserId,
                    "/works/OL456W",
                    ReadingStatus.WantToRead)
            });

        var controller = CreateController();

        var result = await controller.GetUserProfile("alice");

        var ok = Assert.IsType<OkObjectResult>(result.Result);

        var account = Assert.IsType<UserAccountResponse>(
            ok.Value);

        Assert.Equal(UserId, account.Profile.Id);
        Assert.Equal("alice", account.Profile.Username);
        Assert.Equal(2, account.BookCount);
        Assert.Equal(5, account.FollowersCount);
        Assert.Equal(3, account.FollowingCount);
        Assert.Empty(account.Reviews);

        await _userRepositorySubstitute
            .Received(1)
            .FindByUsernameAsync("alice");

        await _libraryRepositorySubstitute
            .Received(1)
            .GetBooksAsync(UserId);

        await _reviewRepositorySubstitute
            .Received(1)
            .GetByUserAsync(UserId);
    }
}