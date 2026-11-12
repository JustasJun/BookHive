using BookHive.Backend.Data;
using BookHive.Backend.Repositories;
using BookHive.Models;
using BookHive.Tests.Helpers;

namespace BookHive.Tests.Repositories;

public class ReviewRepositoryTests : IDisposable
{
    private static readonly DateTime BaseTime =
        new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly BookHiveDbContext _context = TestDbContextFactory.Create();
    private readonly ReviewRepository _repository;

    public ReviewRepositoryTests()
    {
        _repository = new ReviewRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private User AddUser(string username)
    {
        var user = new User(username, $"{username}@test.com", "hash");
        _context.Users.Add(user);
        _context.SaveChanges();

        return user;
    }

    private Review AddReview(
        int userId,
        string bookId,
        DateTime createdAt,
        string title = "Book")
    {
        var review = new Review(userId, bookId, title, "Author", null, 5, "Text")
        {
            CreatedAt = createdAt
        };
        _context.Reviews.Add(review);
        _context.SaveChanges();

        return review;
    }

    [Fact]
    public async Task GetLatestWithAuthorsAsync_ReturnsNewestFirst()
    {
        User user = AddUser("anna");
        AddReview(user.Id, "OL1W", BaseTime, "Old");
        AddReview(user.Id, "OL2W", BaseTime.AddHours(2), "Newest");
        AddReview(user.Id, "OL3W", BaseTime.AddHours(1), "Middle");

        List<CommunityReviewItem> result =
            await _repository.GetLatestWithAuthorsAsync(10);

        Assert.Equal(
            new[] { "Newest", "Middle", "Old" },
            result.Select(item => item.BookTitle));
    }

    [Fact]
    public async Task GetLatestWithAuthorsAsync_ReturnsOnlyRequestedCount()
    {
        User user = AddUser("anna");

        for (int i = 0; i < 20; i++)
        {
            AddReview(user.Id, $"OL{i}W", BaseTime.AddMinutes(i));
        }

        List<CommunityReviewItem> result =
            await _repository.GetLatestWithAuthorsAsync(15);

        Assert.Equal(15, result.Count);
        Assert.Equal(BaseTime.AddMinutes(19), result[0].CreatedAt);
    }

    [Fact]
    public async Task GetLatestWithAuthorsAsync_IncludesReviewsFromAllUsersWithUsernames()
    {
        User anna = AddUser("anna");
        User ben = AddUser("ben");
        AddReview(anna.Id, "OL1W", BaseTime);
        AddReview(ben.Id, "OL2W", BaseTime.AddMinutes(1));

        List<CommunityReviewItem> result =
            await _repository.GetLatestWithAuthorsAsync(10);

        Assert.Equal(new[] { "ben", "anna" }, result.Select(item => item.Username));
        Assert.Equal(new[] { ben.Id, anna.Id }, result.Select(item => item.UserId));
    }

    [Fact]
    public async Task GetLatestWithAuthorsAsync_WhenSameTimestamp_NewestIdFirst()
    {
        User user = AddUser("anna");
        Review first = AddReview(user.Id, "OL1W", BaseTime);
        Review second = AddReview(user.Id, "OL2W", BaseTime);

        List<CommunityReviewItem> result =
            await _repository.GetLatestWithAuthorsAsync(10);

        Assert.Equal(
            new[] { second.Id, first.Id },
            result.Select(item => item.ReviewId));
    }

    [Fact]
    public async Task GetLatestWithAuthorsAsync_WhenNoReviews_ReturnsEmptyList()
    {
        List<CommunityReviewItem> result =
            await _repository.GetLatestWithAuthorsAsync(15);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ReviewExistsAsync_WhenUserReviewedBook_ReturnsTrue()
    {
        User user = AddUser("anna");
        AddReview(user.Id, "OL1W", BaseTime);

        Assert.True(await _repository.ReviewExistsAsync(user.Id, "OL1W"));
    }

    [Fact]
    public async Task ReviewExistsAsync_WhenOtherUserReviewedBook_ReturnsFalse()
    {
        User anna = AddUser("anna");
        User ben = AddUser("ben");
        AddReview(anna.Id, "OL1W", BaseTime);

        Assert.False(await _repository.ReviewExistsAsync(ben.Id, "OL1W"));
    }

    [Fact]
    public async Task FindByUserAsync_WhenReviewBelongsToOtherUser_ReturnsNull()
    {
        User anna = AddUser("anna");
        User ben = AddUser("ben");
        Review review = AddReview(anna.Id, "OL1W", BaseTime);

        Review? result = await _repository.FindByUserAsync(ben.Id, review.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task FindByUserAsync_WhenReviewBelongsToUser_ReturnsIt()
    {
        User anna = AddUser("anna");
        Review review = AddReview(anna.Id, "OL1W", BaseTime);

        Review? result = await _repository.FindByUserAsync(anna.Id, review.Id);

        Assert.Same(review, result);
    }

    [Fact]
    public async Task GetByBookAsync_ReturnsOnlyReviewsOfThatBook()
    {
        User anna = AddUser("anna");
        User ben = AddUser("ben");
        AddReview(anna.Id, "OL1W", BaseTime);
        AddReview(ben.Id, "OL1W", BaseTime.AddMinutes(1));
        AddReview(anna.Id, "OL2W", BaseTime.AddMinutes(2));

        List<Review> result = await _repository.GetByBookAsync("OL1W");

        Assert.Equal(2, result.Count);
        Assert.All(result, review => Assert.Equal("OL1W", review.OpenLibraryBookId));
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsOnlyThatUsersReviewsNewestFirst()
    {
        User anna = AddUser("anna");
        User ben = AddUser("ben");
        AddReview(anna.Id, "OL1W", BaseTime, "Old");
        AddReview(anna.Id, "OL2W", BaseTime.AddHours(1), "New");
        AddReview(ben.Id, "OL3W", BaseTime.AddHours(2), "NotMine");

        List<Review> result = await _repository.GetByUserAsync(anna.Id);

        Assert.Equal(new[] { "New", "Old" }, result.Select(review => review.BookTitle));
    }

    [Fact]
    public async Task AddAndSaveAsync_PersistsReview()
    {
        User user = AddUser("anna");
        var review = new Review(user.Id, "OL1W", "Dune", "Frank Herbert", null, 4, "Good");

        _repository.Add(review);
        await _repository.SaveAsync();

        Assert.Single(_context.Reviews);
        Assert.NotEqual(0, review.Id);
    }

    [Fact]
    public async Task RemoveAndSaveAsync_DeletesReview()
    {
        User user = AddUser("anna");
        Review review = AddReview(user.Id, "OL1W", BaseTime);

        _repository.Remove(review);
        await _repository.SaveAsync();

        Assert.Empty(_context.Reviews);
    }
}