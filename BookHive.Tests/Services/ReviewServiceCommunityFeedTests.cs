using BookHive.Backend.DTOs;
using BookHive.Backend.Models;
using BookHive.Backend.Repositories;
using BookHive.Models;
using BookHive.Services;

namespace BookHive.Tests.Services;

public class ReviewServiceCommunityFeedTests
{
    private const int UserId = 1;
    private const string BookId = "OL1W";

    private readonly IReviewRepository _reviewRepository =
        Substitute.For<IReviewRepository>();

    private readonly IUserRepository _userRepository =
        Substitute.For<IUserRepository>();

    private readonly ICommunityFeed _communityFeed =
        Substitute.For<ICommunityFeed>();

    private readonly ReviewService _service;

    public ReviewServiceCommunityFeedTests()
    {
        _service = new ReviewService(
            _reviewRepository,
            _userRepository,
            _communityFeed);
    }

    [Fact]
    public async Task AddReviewAsync_WhenReviewIsSaved_AddsItToCommunityFeed()
    {
        _reviewRepository.ReviewExistsAsync(UserId, BookId).Returns(false);
        _userRepository.FindByIdAsync(UserId)
            .Returns(new User("meda", "meda@test.com", "hash"));

        AddReviewResult result = await _service.AddReviewAsync(
            UserId, BookId, "Dune", "Frank Herbert", null, 5, "Great");

        Assert.Equal(AddReviewResult.Added, result);
        _communityFeed.Received(1).Add(Arg.Is<CommunityReviewItem>(item =>
            item.Username == "meda" && item.BookTitle == "Dune"));
    }

    [Fact]
    public async Task AddReviewAsync_WhenRatingIsInvalid_DoesNotTouchFeed()
    {
        AddReviewResult result = await _service.AddReviewAsync(
            UserId, BookId, "Dune", "Frank Herbert", null, 0, "Great");

        Assert.Equal(AddReviewResult.Invalid, result);
        _communityFeed.DidNotReceive().Add(Arg.Any<CommunityReviewItem>());
    }

    [Fact]
    public async Task AddReviewAsync_WhenReviewAlreadyExists_DoesNotTouchFeed()
    {
        _reviewRepository.ReviewExistsAsync(UserId, BookId).Returns(true);

        AddReviewResult result = await _service.AddReviewAsync(
            UserId, BookId, "Dune", "Frank Herbert", null, 5, "Great");

        Assert.Equal(AddReviewResult.AlreadyExists, result);
        _communityFeed.DidNotReceive().Add(Arg.Any<CommunityReviewItem>());
    }

    [Fact]
    public async Task RemoveByUserAsync_WhenReviewIsDeleted_RemovesItFromFeed()
    {
        var review = new Review(
            UserId, BookId, "Dune", "Frank Herbert", null, 5, "Great")
        {
            Id = 7
        };
        _reviewRepository.FindByUserAsync(UserId, 7).Returns(review);

        bool removed = await _service.RemoveByUserAsync(UserId, 7);

        Assert.True(removed);
        _communityFeed.Received(1).Remove(7);
    }

    [Fact]
    public async Task RemoveByUserAsync_WhenReviewIsNotFound_DoesNotTouchFeed()
    {
        _reviewRepository.FindByUserAsync(UserId, 7).Returns((Review?)null);

        bool removed = await _service.RemoveByUserAsync(UserId, 7);

        Assert.False(removed);
        _communityFeed.DidNotReceive().Remove(Arg.Any<int>());
    }

    [Fact]
    public void GetCommunityFeed_ReturnsItemsFromFeed()
    {
        var items = new List<CommunityReviewItem>
        {
            new(1, UserId, "meda", "Dune", "Frank Herbert", null, 5, "Great", DateTime.UtcNow)
        };
        _communityFeed.GetLatest().Returns(items);

        List<CommunityReviewItem> result = _service.GetCommunityFeed();

        Assert.Same(items, result);
    }

}