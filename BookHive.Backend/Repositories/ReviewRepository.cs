using BookHive.Backend.Data;
using BookHive.Models;

namespace BookHive.Backend.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly BookHiveDbContext _context;

    public ReviewRepository(BookHiveDbContext context)
    {
        _context = context;
    }

    public bool ReviewExists(int userId, string bookId)
    {
        return _context.Reviews.Any(review =>
            review.UserId == userId &&
            review.OpenLibraryBookId == bookId);
    }

    public Review? FindByUser(int userId, int reviewId)
    {
        return _context.Reviews.SingleOrDefault(review =>
            review.Id == reviewId &&
            review.UserId == userId);
    }

    public List<Review> GetByBook(string bookId)
    {
        var reviews = _context.Reviews
            .Where(review => review.OpenLibraryBookId == bookId)
            .ToList();

        reviews.Sort();
        reviews.Reverse();
        return reviews;
    }

    public List<Review> GetByUser(int userId)
    {
        return _context.Reviews
            .Where(review => review.UserId == userId)
            .OrderByDescending(review => review.CreatedAt)
            .ThenByDescending(review => review.Id)
            .ToList();
    }

    public void Add(Review review)
    {
        _context.Reviews.Add(review);
    }

    public void Remove(Review review)
    {
        _context.Reviews.Remove(review);
    }

    public void Save()
    {
        _context.SaveChanges();
    }
}
