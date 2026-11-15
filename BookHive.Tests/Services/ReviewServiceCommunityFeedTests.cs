using BookHive.Backend.Models;
using BookHive.Backend.Repositories;
using BookHive.Models;
using BookHive.Services;
using NSubstitute;
using Xunit;

namespace BookHive.Tests.Services;

public class ReviewServiceCommunityFeedTests
{
    private readonly IReviewRepository _repository = Substitute.For<IReviewRepository>();
    private readonly ReviewService _service;

    public ReviewServiceCommunityFeedTests()
    {
        _service = new ReviewService(_repository);
    }

    [Fact]
    public async Task GetCommunityFeedAsync_ReturnsItemsFromRepository()
    {
        var items = new List<CommunityReviewItem>
        {
            new(1, 10, "anna", "Dune", "Frank Herbert", null, 5, "Great", DateTime.UtcNow)
        };
        _repository.GetLatestWithAuthorsAsync(Arg.Any<int>()).Returns(items);

        var result = await _service.GetCommunityFeedAsync();

        Assert.Same(items, result);
    }

    [Fact]
    public async Task GetCommunityFeedAsync_RequestsFifteenItems()
    {
        _repository.GetLatestWithAuthorsAsync(Arg.Any<int>())
            .Returns(new List<CommunityReviewItem>());

        await _service.GetCommunityFeedAsync();

        await _repository.Received(1).GetLatestWithAuthorsAsync(15);
    }

    [Fact]
    public async Task AddReviewAsync_ValidReview_SavesToRepository()
    {
        _repository.ReviewExistsAsync(1, "OL1W").Returns(false);

        var result = await _service.AddReviewAsync(1, "OL1W", "Dune", "Frank Herbert", null, 5, " Great ");

        Assert.Equal(AddReviewResult.Added, result);
        _repository.Received(1).Add(Arg.Is<Review>(r => r.Text == "Great"));
        await _repository.Received(1).SaveAsync();
    }

    [Fact]
    public async Task AddReviewAsync_InvalidRating_DoesNotTouchRepository()
    {
        var result = await _service.AddReviewAsync(1, "OL1W", "Dune", "Frank Herbert", null, 6, "Text");

        Assert.Equal(AddReviewResult.Invalid, result);
        _repository.DidNotReceive().Add(Arg.Any<Review>());
    }

    [Fact]
    public async Task RemoveByUserAsync_ReviewNotFound_ReturnsFalse()
    {
        _repository.FindByUserAsync(1, 99).Returns((Review?)null);

        var result = await _service.RemoveByUserAsync(1, 99);

        Assert.False(result);
        _repository.DidNotReceive().Remove(Arg.Any<Review>());
    }

    [Fact]
    public async Task GetFeedAsync_ReturnsItemsFromRepository()
    {
        var items = new List<CommunityReviewItem>
        {
            new(1, 10, "anna", "Dune", "Frank Herbert", null, 5, "Great", DateTime.UtcNow)
        };
        _repository.GetFeedAsync(1, Arg.Any<int>()).Returns(items);

        var result = await _service.GetFeedAsync(1);

        Assert.Same(items, result);
    }
}