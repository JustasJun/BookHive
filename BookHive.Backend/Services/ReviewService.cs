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

    public int CountForGoal(int userId, ReadingGoal goal)
    {
        DateTime yearStart = new DateTime(goal.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime yearEnd = yearStart.AddYears(1);
        DateTime from = goal.StartedAt > yearStart ? goal.StartedAt : yearStart;

        return _context.Reviews.Count(item =>
            item.UserId == userId &&
            item.CreatedAt >= from &&
            item.CreatedAt < yearEnd);
    }
}