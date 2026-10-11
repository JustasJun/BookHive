using BookHive.Backend.Repositories;
using BookHive.Models;

namespace BookHive.Services;

public class ReviewService
{
    private readonly IReviewRepository _reviewRepository;

    public ReviewService(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public bool AddReview(Review review)
    {
        if (_reviewRepository.ReviewExists(review.UserId, review.OpenLibraryBookId))
        {
            return false;
        }

        _reviewRepository.Add(review);
        _reviewRepository.Save();
        return true;
    }

    public List<Review> GetByBook(string bookId)
    {
        return _reviewRepository.GetByBook(bookId);
    }

    public List<Review> GetByUser(int userId)
    {
        return _reviewRepository.GetByUser(userId);
    }

    public bool RemoveByUser(int userId, int reviewId)
    {
        var review = _reviewRepository.FindByUser(userId, reviewId);

        if (review == null)
        {
            return false;
        }

        _reviewRepository.Remove(review);
        _reviewRepository.Save();
        return true;
    }
    
}