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
        return _context.Reviews
            .Where(item => item.OpenLibraryBookId == bookId)
            .OrderByDescending(item => item.CreatedAt)
            .ToList();
    }
}