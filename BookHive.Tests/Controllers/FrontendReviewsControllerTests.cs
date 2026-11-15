extern alias Frontend;

using System.Net;
using System.Text.Json;
using BookHive.Models;
using BookHive.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using FrontendReviewsController = Frontend::BookHive.Frontend.Controllers.ReviewsController;

namespace BookHive.Tests.Controllers;

public class FrontendReviewsControllerTests
{
    [Fact]
    public async Task UserPosts_WhenProfileRequestFails_StillReturnsReviewsWithWarning()
    {
        var review = new Review(42, "/works/OL1W", "Book", "Author", null, 5, "Review")
        {
            Id = 7
        };
        var handler = new StubHttpMessageHandler(request =>
            request.RequestUri!.AbsolutePath.EndsWith(
                "/api/reviews/user/42",
                StringComparison.Ordinal)
                ? StubHttpMessageHandler.Json(JsonSerializer.Serialize(new[] { review }))
                : StubHttpMessageHandler.Json(
                    "{}",
                    HttpStatusCode.ServiceUnavailable));
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://backend.test/")
        };
        var clientFactory = Substitute.For<IHttpClientFactory>();
        clientFactory.CreateClient("BackendApi").Returns(httpClient);
        var controller = new FrontendReviewsController(clientFactory).WithUser(42);

        var result = await controller.UserPosts();

        var view = Assert.IsType<ViewResult>(result);
        var reviews = Assert.IsType<List<Review>>(view.Model);
        Review returnedReview = Assert.Single(reviews);
        Assert.Equal(review.Id, returnedReview.Id);
        Assert.Equal(review.Text, returnedReview.Text);
        Assert.Equal(
            "Could not load your profile photo. Showing initials instead.",
            controller.ViewBag.AvatarWarning);
        Assert.Null(controller.ViewBag.AvatarUrl);
    }
}
