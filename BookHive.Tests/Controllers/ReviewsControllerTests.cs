using BookHive.Backend.Controllers;
using BookHive.Backend.DTOs;
using BookHive.Backend.Repositories;
using BookHive.Models;
using BookHive.Services;
using BookHive.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Tests.Controllers;

public class ReviewsControllerTests
{
    private const int UserId = 1;

    private readonly IReviewRepository _reviewRepository =
        Substitute.For<IReviewRepository>();

    private readonly IUserRepository _userRepository =
        Substitute.For<IUserRepository>();

    private readonly ICommunityFeed _communityFeed =
        Substitute.For<ICommunityFeed>();

    private ReviewsController CreateController(int? userId = UserId)
    {
        var service = new ReviewService(
            _reviewRepository,
            _userRepository,
            _communityFeed);

        return new ReviewsController(service).WithUser(userId);
    }

    private static CreateReviewRequest ValidRequest(int rating = 5) =>
        new("OL1W", "Dune", "Frank Herbert", null, rating, "Great book");

    [Fact]
    public async Task Add_WhenNotLoggedIn_ReturnsUnauthorized()
    {
        IActionResult result =
            await CreateController(userId: null).Add(ValidRequest());

        Assert.IsType<UnauthorizedResult>(result);
        _reviewRepository.DidNotReceive().Add(Arg.Any<Review>());
    }

    [Fact]
    public async Task Add_WhenReviewIsValid_ReturnsOkAndSaves()
    {
        _reviewRepository.ReviewExistsAsync(UserId, "OL1W").Returns(false);
        _userRepository.FindByIdAsync(UserId)
            .Returns(new User("meda", "meda@test.com", "hash"));

        IActionResult result = await CreateController().Add(ValidRequest());

        Assert.IsType<OkResult>(result);
        _reviewRepository.Received(1).Add(Arg.Any<Review>());
        await _reviewRepository.Received(1).SaveAsync();
    }

    [Fact]
    public async Task Add_WhenRatingIsInvalid_ReturnsBadRequest()
    {
        IActionResult result = await CreateController().Add(ValidRequest(rating: 6));

        Assert.IsType<BadRequestResult>(result);
        _reviewRepository.DidNotReceive().Add(Arg.Any<Review>());
    }

    [Fact]
    public async Task Add_WhenReviewAlreadyExists_ReturnsConflict()
    {
        _reviewRepository.ReviewExistsAsync(UserId, "OL1W").Returns(true);

        IActionResult result = await CreateController().Add(ValidRequest());

        Assert.IsType<ConflictResult>(result);
    }

    [Fact]
    public async Task GetByUser_ReturnsReviewsOfThatUser()
    {
        var review = new Review(UserId, "OL1W", "Dune", "Frank Herbert", null, 5, "Great");
        _reviewRepository.GetByUserAsync(UserId).Returns(new List<Review> { review });

        ActionResult<List<Review>> result = await CreateController().GetByUser(UserId);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
        var reviews = Assert.IsType<List<Review>>(ok.Value);
        Assert.Same(review, Assert.Single(reviews));
    }

    [Fact]
    public async Task Remove_WhenReviewExists_ReturnsNoContent()
    {
        var review = new Review(UserId, "OL1W", "Dune", "Frank Herbert", null, 5, "Great")
        {
            Id = 7
        };
        _reviewRepository.FindByUserAsync(UserId, 7).Returns(review);

        IActionResult result = await CreateController().Remove(7);

        Assert.IsType<NoContentResult>(result);
        _reviewRepository.Received(1).Remove(review);
    }

    [Fact]
    public async Task Remove_WhenReviewIsNotFound_ReturnsNotFound()
    {
        _reviewRepository.FindByUserAsync(UserId, 7).Returns((Review?)null);

        IActionResult result = await CreateController().Remove(7);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Remove_WhenNotLoggedIn_ReturnsUnauthorized()
    {
        IActionResult result = await CreateController(userId: null).Remove(7);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void GetCommunityFeed_ReturnsItemsFromFeed()
    {
        var items = new List<CommunityReviewItem>
        {
            new(1, UserId, "meda", "Dune", "Frank Herbert", null, 5, "Great", DateTime.UtcNow)
        };
        _communityFeed.GetLatest().Returns(items);

        ActionResult<List<CommunityReviewItem>> result =
            CreateController().GetCommunityFeed();

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(items, ok.Value);
    }
}