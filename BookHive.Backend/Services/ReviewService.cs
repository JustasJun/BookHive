using BookHive.Backend.Data;
using BookHive.Models;

namespace BookHive.Services;

public class ReviewService
{
    private readonly BookHiveDbContext _context;

    public ReviewService(BookHiveDbContext context)
    {
        _context = context;
    }

    public bool AddReview(Review review)
    {
        bool alreadyExists = _context.Reviews.Any(item =>
            item.UserId == review.UserId &&
            item.OpenLibraryBookId == review.OpenLibraryBookId);

        if (alreadyExists)
        {
            return false;
        }

        _context.Reviews.Add(review);
        _context.SaveChanges();
        return true;
    }

    public List<Review> GetByBook(string bookId)
    {
        var reviews = _context.Reviews
            .Where(item => item.OpenLibraryBookId == bookId)
            .ToList();

            reviews.Sort();
            reviews.Reverse();

            return reviews;
    }

    public List<Review> GetByUser(int userId)
    {
        return _context.Reviews
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.CreatedAt)
            .ThenByDescending(item => item.Id)
            .ToList();
    }
    
}