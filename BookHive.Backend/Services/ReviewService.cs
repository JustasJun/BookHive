using BookHive.Backend.Repositories;
using BookHive.Models;
using BookHive.Backend.Models;

namespace BookHive.Services;

public class ReviewService
{
    private readonly IReviewRepository _reviewRepository;

    public ReviewService(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
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

        return AddReviewResult.Added;
    }

    public async Task<List<Review>> GetByBookAsync(string bookId)
    {
        var reviews = await _reviewRepository.GetByBookAsync(bookId);

        reviews.Sort();     // uses Review.CompareTo (IComparable)
        reviews.Reverse();

        return reviews;
    }

    public async Task<List<Review>> GetByUserAsync(int userId)
    {
        return await _reviewRepository.GetByUserAsync(userId);
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

        return true;
    }
}
