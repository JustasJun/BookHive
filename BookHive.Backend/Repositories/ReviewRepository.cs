using BookHive.Backend.Data;
using BookHive.Models;
using Microsoft.EntityFrameworkCore;

namespace BookHive.Backend.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly BookHiveDbContext _context;

    public ReviewRepository(BookHiveDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ReviewExistsAsync(int userId, string bookId)
    {
        return await _context.Reviews.AnyAsync(review =>
            review.UserId == userId &&
            review.OpenLibraryBookId == bookId);
    }

    public async Task<Review?> FindByUserAsync(int userId, int reviewId)
    {
        return await _context.Reviews.SingleOrDefaultAsync(review =>
            review.Id == reviewId &&
            review.UserId == userId);
    }

    public async Task<List<Review>> GetByBookAsync(string bookId)
    {
        return await _context.Reviews
            .Where(review => review.OpenLibraryBookId == bookId)
            .ToListAsync();
    }

    public async Task<List<Review>> GetByUserAsync(int userId)
    {
        return await _context.Reviews
            .Where(review => review.UserId == userId)
            .OrderByDescending(review => review.CreatedAt)
            .ThenByDescending(review => review.Id)
            .ToListAsync();
    }

    public void Add(Review review)
    {
        _context.Reviews.Add(review);
    }

    public void Remove(Review review)
    {
        _context.Reviews.Remove(review);
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}
