using BookHive.Models;

namespace BookHive.Backend.Repositories;

public interface IReviewRepository
{
    Task<bool> ReviewExistsAsync(int userId, string bookId);
    Task<Review?> FindByUserAsync(int userId, int reviewId);
    Task<List<Review>> GetByBookAsync(string bookId);
    Task<List<Review>> GetByUserAsync(int userId);
    void Add(Review review);
    void Remove(Review review);
    Task SaveAsync();
}
