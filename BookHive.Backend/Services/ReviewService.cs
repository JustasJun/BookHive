using BookHive.Backend.Repositories;
using BookHive.Models;
using BookHive.Backend.Models;
using BookHive.Backend.DTOs;

namespace BookHive.Services;

public class ReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICommunityFeed _communityFeed;

    public ReviewService(
    IReviewRepository reviewRepository,
    IUserRepository userRepository,
    ICommunityFeed communityFeed)
{
    _reviewRepository = reviewRepository;
    _userRepository = userRepository;
    _communityFeed = communityFeed;
}

    public async Task<AddReviewResult> AddReviewAsync(
        int userId,
        string openLibraryBookId,
        string bookTitle,
        string bookAuthor,
        string? coverId,
        int rating,
        string text)
    {
        if (rating < 1 || rating > 5 || string.IsNullOrWhiteSpace(text))
        {
            return AddReviewResult.Invalid;
        }

        if (await _reviewRepository.ReviewExistsAsync(userId, openLibraryBookId))
        {
            return AddReviewResult.AlreadyExists;
        }

        var review = new Review(
            userId,
            openLibraryBookId,
            bookTitle,
            bookAuthor,
            coverId,
            rating,
            text.Trim());

        _reviewRepository.Add(review);
        await _reviewRepository.SaveAsync();

        await PublishToCommunityFeedAsync(review);

        return AddReviewResult.Added;
    }

    public async Task<List<Review>> GetByBookAsync(string bookId)
    {
        List<Review> reviews = await _reviewRepository.GetByBookAsync(bookId);

        reviews.Sort();     // uses Review.CompareTo (IComparable)
        reviews.Reverse();

        return reviews;
    }

    public async Task<List<Review>> GetByUserAsync(int userId)
    {
        return await _reviewRepository.GetByUserAsync(userId);
    }

    public List<CommunityReviewItem> GetCommunityFeed()
    {
        return _communityFeed.GetLatest();
    }

    public async Task<bool> RemoveByUserAsync(int userId, int reviewId)
    {
        var review = await _reviewRepository.FindByUserAsync(userId, reviewId);

        if (review == null)
        {
            return false;
        }

        _reviewRepository.Remove(review);
        await _reviewRepository.SaveAsync();

        _communityFeed.Remove(reviewId);

        return true;
    }

    private async Task PublishToCommunityFeedAsync(Review review)
    {
        User? author = await _userRepository.FindByIdAsync(review.UserId);

        if (author == null)
        {
            return;
        }

        _communityFeed.Add(new CommunityReviewItem(
            review.Id,
            review.UserId,
            author.Username,
            review.BookTitle,
            review.BookAuthor,
            review.CoverId,
            review.Rating,
            review.Text,
            review.CreatedAt));
    }
}
