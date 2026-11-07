using BookHive.Models;

namespace BookHive.Services;

public interface ICommunityFeed
{
    void Add(CommunityReviewItem item);
    bool Remove(int reviewId);
    List<CommunityReviewItem> GetLatest();
}