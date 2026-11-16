using BookHive.Backend.Data;
using BookHive.Models;
using Microsoft.EntityFrameworkCore;

namespace BookHive.Backend.Repositories;

public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(BookHiveDbContext context) : base(context)
    {
    }

    public async Task<bool> ReviewExistsAsync(int userId, string bookId)
    {
        return await ExistsAsync(review =>
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

    public async Task<List<CommunityReviewItem>> GetLatestWithAuthorsAsync(int count)
    {
        return await (
            from review in _context.Reviews
            join user in _context.Users on review.UserId equals user.Id
            orderby review.CreatedAt descending, review.Id descending
            select new CommunityReviewItem(
                review.Id,
                review.UserId,
                user.Username,
                review.BookTitle,
                review.BookAuthor,
                review.CoverId,
                review.Rating,
                review.Text,
                review.CreatedAt,
                user.AvatarUrl))
            .Take(count)
            .ToListAsync();
    }

    // reviews of people the user follows, written after the follow started
    public async Task<List<CommunityReviewItem>> GetFeedAsync(int userId, int count)
    {
        return await (
            from follow in _context.Set<Dictionary<string, object>>("Follows")
            where EF.Property<int>(follow, "FollowerId") == userId
            join review in _context.Reviews
                on EF.Property<int>(follow, "FolloweeId") equals review.UserId
            join user in _context.Users on review.UserId equals user.Id
            where review.CreatedAt >= EF.Property<DateTime>(follow, "FollowedAt")
            orderby review.CreatedAt descending, review.Id descending
            select new CommunityReviewItem(
                review.Id,
                review.UserId,
                user.Username,
                review.BookTitle,
                review.BookAuthor,
                review.CoverId,
                review.Rating,
                review.Text,
                review.CreatedAt,
                user.AvatarUrl))
            .Take(count)
            .ToListAsync();
    }
}
