using BookHive.Backend.Models;
using BookHive.Models;

namespace BookHive.Services;

public interface IReviewService
{
    Task<AddReviewResult> AddReviewAsync(
        int userId,
        string openLibraryBookId,
        string bookTitle,
        string bookAuthor,
        string? coverId,
        int rating,
        string text);

    Task<List<Review>> GetByBookAsync(string bookId);
    Task<List<Review>> GetByUserAsync(int userId);
    Task<List<CommunityReviewItem>> GetCommunityFeedAsync();
    Task<List<CommunityReviewItem>> GetFeedAsync(int userId);
    Task<bool> RemoveByUserAsync(int userId, int reviewId);
}