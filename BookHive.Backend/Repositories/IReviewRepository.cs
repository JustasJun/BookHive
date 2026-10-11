using BookHive.Models;

namespace BookHive.Backend.Repositories;

public interface IReviewRepository
{
    bool ReviewExists(int userId, string bookId);
    Review? FindByUser(int userId, int reviewId);
    List<Review> GetByBook(string bookId);
    List<Review> GetByUser(int userId);
    void Add(Review review);
    void Remove(Review review);
    void Save();
}
