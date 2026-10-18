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

    public AddReviewResult AddReview(
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

        if (_reviewRepository.ReviewExists(userId, openLibraryBookId))
        {
            return AddReviewResult.AlreadyExists;
        }

        Review review = new Review(
            userId,
            openLibraryBookId,
            bookTitle,
            bookAuthor,
            coverId,
            rating,
            text.Trim());

        _reviewRepository.Add(review);
        _reviewRepository.Save();

        return AddReviewResult.Added;
    }

    public List<Review> GetByBook(string bookId)
    {
        List<Review> reviews = _reviewRepository.GetByBook(bookId);

        reviews.Sort();     // uses Review.CompareTo (IComparable)
        reviews.Reverse();

        return reviews;
    }

    public List<Review> GetByUser(int userId)
    {
        return _reviewRepository.GetByUser(userId);
    }

    public bool RemoveByUser(int userId, int reviewId)
    {
        Review? review = _reviewRepository.FindByUser(userId, reviewId);

        if (review == null)
        {
            return false;
        }

        _reviewRepository.Remove(review);
        _reviewRepository.Save();

        return true;
    }
}
