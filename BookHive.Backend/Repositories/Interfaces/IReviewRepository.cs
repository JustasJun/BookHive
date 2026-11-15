using BookHive.Models;

namespace BookHive.Backend.Repositories;

public interface IReviewRepository
{
    Task<bool> ReviewExistsAsync(int userId, string bookId);
    Task<Review?> FindByUserAsync(int userId, int reviewId);
    Task<List<Review>> GetByBookAsync(string bookId);
    Task<List<Review>> GetByUserAsync(int userId);
    Task<List<CommunityReviewItem>> GetLatestWithAuthorsAsync(int count);
    Task<List<CommunityReviewItem>> GetFeedAsync(int userId, int count);
    void Add(Review review);
    void Remove(Review review);
    Task SaveAsync();
}
